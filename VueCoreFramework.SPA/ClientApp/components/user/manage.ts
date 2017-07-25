import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as Api from '../../api';
import * as Store from '../../store/store';
import { authenticate } from '../../authorization';
import { checkResponse } from '../../router';
import { defaultCulture, setCulture } from '../../globalization/globalization';
import VueFormGenerator from 'vue-form-generator';
import { Schema, VFGOptions } from '../../vfg/vfg';
import * as VFG_Custom from '../../vfg/vfg-custom-validators';
import * as ErrorMsg from '../../error-msg';

/**
 * A ViewModel used to transfer information during user account management tasks.
 */
interface ManageUserViewModel {
    /**
     * The username of the account.
     */
    username: string;

    /**
     * The email address of the user account.
     */
    email: string;

    /**
     * The original password of the user account.
     */
    oldPassword: string;

    /**
     * The new password for the user account.
     */
    newPassword: string;

    /**
     * The new password for the user account, repeated.
     */
    confirmPassword: string;

    /**
     * The name of a third-party authorization provider.
     */
    authProvider: string;
}

/**
 * Contains lists of external authentication providers supported by the SPA framework.
 */
interface AuthProviders {
    /**
     * A list of all external authentication providers supported by the SPA framework.
     */
    providers: Array<string>;

    /**
     * A list of the external authentication providers registered for the current user.
     */
    userProviders: Array<string>;
}

@Component
export default class ManageUserComponent extends Vue {
    components = {
        'vue-form-generator': VueFormGenerator.component
    };

    authProviderFacebook = false;
    authProviderFacebookUser = false;
    authProviderGoogle = false;
    authProviderGoogleUser = false;
    authProviderMicrosoft = false;
    authProviderMicrosoftUser = false;
    changingEmail = false;
    changingPassword = false;
    changingUsername = false;
    cultures: string[] = ["<default>"];
    deleteAccountDialog = false;
    errors: string[] = [];
    formOptions: VFGOptions = {
        validateAfterChanged: true
    };
    hasPassword = false;
    isValid = false;
    model: ManageUserViewModel = {
        username: '',
        email: '',
        oldPassword: '',
        newPassword: '',
        confirmPassword: '',
        authProvider: ''
    };
    schema: Schema = {
        fields: [
            {
                type: 'vuetifyText',
                inputType: 'string',
                model: 'username',
                placeholder: 'Username',
                autocomplete: true,
                validator: this.requireEmail,
                visible: () => this.changingUsername
            },
            {
                type: 'vuetifyText',
                inputType: 'email',
                model: 'email',
                placeholder: 'Email',
                autocomplete: true,
                validator: this.requireEmail,
                visible: () => this.changingEmail
            },
            {
                type: 'vuetifyText',
                inputType: 'password',
                model: 'oldPassword',
                placeholder: 'Old Password',
                autocomplete: true,
                validator: this.requirePassword,
                visible: () => this.changingPassword || this.settingPassword
            },
            {
                type: 'vuetifyText',
                inputType: 'password',
                model: 'newPassword',
                placeholder: 'New Password',
                min: 6,
                max: 24,
                validator: this.requireNewPassword,
                visible: () => this.changingPassword || this.settingPassword
            },
            {
                type: 'vuetifyText',
                inputType: 'password',
                model: 'confirmPassword',
                placeholder: 'Confirm Password',
                validator: this.requirePasswordMatch,
                visible: () => this.changingPassword || this.settingPassword
            }
        ]
    };
    selectedCulture = "<default>";
    selectedXferUsername = '';
    settingPassword = false;
    successMessage = "Success!";
    submitting = false;
    success = false;
    validXferUsername = false;
    xferData = false;
    xferLoading = false;
    xferUsernames: Array<string> = [];

    @Watch('xferData')
    onXferChange(val: boolean, oldVal: boolean) {
        if (val) {
            this.loadXferUsernames();
        }
    }

    async created() {
        try {
            let response = await Api.getAuth('Account/HasPassword', this.$route.fullPath);
            if (!response.ok) {
                if (response.statusText) {
                    this.errors.push(response.statusText);
                } else {
                    this.errors.push("A problem occurred.");
                }
                throw new Error(response.statusText);
            } else if (response.statusText === "yes") {
                this.hasPassword = true;
            }
        } catch (error) {
            if (this.errors.length === 0) {
                ErrorMsg.logError("user/manage.created.fetchPW", error);
            }
        }
    }

    async mounted() {
        try {
            let response = await Api.getAuth('Account/GetUserAuthProviders', this.$route.fullPath)
            if (!response.ok) {
                if (response.statusText) {
                    this.errors.push(response.statusText);
                } else {
                    this.errors.push("A problem occurred.");
                }
                throw new Error("CODE");
            }
            let data = await response.json() as AuthProviders;
            if (data.providers) {
                this.authProviderFacebook = data.providers.includes('Facebook');
                this.authProviderGoogle = data.providers.includes('Google');
                this.authProviderMicrosoft = data.providers.includes('Microsoft');
            }
            if (data.userProviders) {
                this.authProviderFacebookUser = data.providers.includes('Facebook');
                this.authProviderGoogleUser = data.providers.includes('Google');
                this.authProviderMicrosoftUser = data.providers.includes('Microsoft');
            }
        } catch (error) {
            if (error !== "CODE") {
                this.errors.push("A problem occurred.");
                ErrorMsg.logError("user/manage.mounted", error);
            }
        }
        try {
            Api.getSpa('Home/GetCultures')
                .then(response => response.json() as Promise<string[]>)
                .then(data => {
                    this.cultures = data;
                    this.cultures.unshift("<default>");
                })
        } catch (error) {
            ErrorMsg.logError("user/manage.mounted", error);
        }
    }

    cancelChange() {
        this.changingEmail = false;
        this.changingPassword = false;
        this.changingUsername = false;
        this.settingPassword = false;
        this.schema.fields[0].visible = false;
        this.schema.fields[1].visible = false;
        this.schema.fields[2].visible = false;
        this.schema.fields[3].visible = false;
        this.schema.fields[4].visible = false;
    }

    changeEmail() {
        this.success = false;
        this.changingEmail = true;
        this.schema.fields[1].visible = true;
    }

    changePassword() {
        this.success = false;
        this.changingPassword = true;
        this.schema.fields[2].visible = true;
        this.schema.fields[3].visible = true;
        this.schema.fields[4].visible = true;
    }

    changeUsername() {
        this.success = false;
        this.changingUsername = true;
        this.schema.fields[0].visible = true;
    }

    async loadXferUsernames() {
        this.xferLoading = true;
        try {
            let response = await Api.getAuth('Manage/LoadXferUsernames', this.$route.fullPath);
            if (!response.ok) {
                throw new Error("CODE");
            }
            this.xferUsernames = await response.json() as string[];
            this.xferData = true;
        } catch (error) {
            // Errors are ignored; the list simply doesn't populate, requiring manual entry.
        }
        this.xferLoading = false;
    }

    async onCultureChange(value: string) {
        this.submitting = true;
        this.success = false;
        this.errors = [];

        if (this.$store.state.userState.culture === value
            || (value === "<default>"
                && this.$store.state.userState.culture === defaultCulture)) {
            this.submitting = false;
            return;
        }

        try {
            let response = await Api.postAuth(`Manage/SetCulture/${value}`, this.$route.fullPath);
            if (!response.ok) {
                if (response.statusText) {
                    this.errors = response.statusText.split(';');
                } else {
                    this.errors.push("A problem occurred.");
                }
                throw new Error("CODE");
            }
            await setCulture(value);
            this.successMessage = "Your preferred culture has been updated.";
            this.success = true;
        } catch (error) {
            if (error !== "CODE") {
                this.errors.push("A problem occurred.");
                ErrorMsg.logError("user/manage.onCultureChange", error);
            }
        }
        this.submitting = false;
    }

    async onDeleteAccount() {
        this.submitting = true;
        this.success = false;
        this.errors = [];
        let url = 'Manage/DeleteAccount';
        if (this.selectedXferUsername) {
            url += `?xferUsername=${this.selectedXferUsername}`;
        }
        try {
            let response = await Api.postAuth(url, this.$route.fullPath);
            if (!response.ok) {
                if (response.statusText) {
                    this.errors = response.statusText.split(';');
                } else {
                    this.errors.push("A problem occurred.");
                }
                throw new Error("CODE");
            }
            this.deleteAccountDialog = false;
            this.successMessage = "Your request to delete your account has been received. An email has just been sent to the address on your account. Please click on the link in this email to confirm the deletion of your account.";
            this.success = true;
        } catch (error) {
            if (error !== "CODE") {
                this.errors.push("A problem occurred. Your account has not been deleted.");
                ErrorMsg.logError("user/manage.onDeleteAccount", error);
            }
        }
        this.submitting = false;
    }

    async onSignInProviderAdd(provider: string) {
        this.submitting = true;
        this.success = false;
        this.errors = [];
        this.model.authProvider = provider;
        try {
            let response = await Api.postAuth('Manage/LinkLogin', this.$route.fullPath, JSON.stringify(this.model));
            if (!response.ok) {
                if (response.statusText) {
                    this.errors = response.statusText.split(';');
                } else {
                    this.errors.push("A problem occurred.");
                }
                throw new Error("CODE");
            }
            this.successMessage = "Your accounts have been successfully linked.";
            this.success = true;
        } catch (error) {
            if (error !== "CODE") {
                this.errors.push("A problem occurred. Login failed.");
                ErrorMsg.logError("user/manage.onSignInProviderAdd", error);
            }
        }
        this.submitting = false;
    }

    async onSignInProviderRemove(provider: string) {
        this.submitting = true;
        this.success = false;
        this.errors = [];
        this.model.authProvider = provider;
        try {
            let response = await Api.postAuth('Manage/RemoveLogin', this.$route.fullPath, JSON.stringify(this.model));
            if (!response.ok) {
                if (response.statusText) {
                    this.errors = response.statusText.split(';');
                } else {
                    this.errors.push("A problem occurred.");
                }
                throw new Error("CODE");
            }
            this.success = true;
        } catch (error) {
            if (error !== "CODE") {
                this.errors.push("A problem occurred. Login failed.");
                ErrorMsg.logError("user/manage.onSignInProviderRemove", error);
            }
        }
        this.submitting = false;
    }

    async onSubmit() {
        this.submitting = true;
        this.success = false;
        this.errors = [];
        if (!this.isValid) return;
        let url: string;
        if (this.changingEmail) {
            url = 'Manage/ChangeEmail';
        } else if (this.changingPassword) {
            url = 'Manage/ChangePassword';
        } else if (this.changingUsername) {
            url = 'Manage/ChangeUsername';
        } else {
            url = 'Manage/SetPassword';
        }
        try {
            let response = await Api.postAuth(url, this.$route.fullPath, JSON.stringify(this.model));
            if (!response.ok) {
                if (response.statusText) {
                    this.errors = response.statusText.split(';');
                } else {
                    this.errors.push("A problem occurred.");
                }
                throw new Error("CODE");
            }
            this.success = true;
            this.successMessage = "Success!";
            if (this.changingUsername) {
                this.$store.state.userState.username = this.model.username;
            }
            this.cancelChange();
        } catch (error) {
            if (error != "CODE") {
                this.errors.push("A problem occurred. Your request was not received.");
                ErrorMsg.logError("user/manage.onSubmit", error);
            }
        }
        this.submitting = false;
    }

    onValidated(isValid: boolean, errors: Array<any>) {
        this.isValid = isValid;
    }

    requireEmail(value, field, model) {
        if (!this.changingEmail) return null;
        return VFG_Custom.requireEmail(value, field, model, undefined);
    }

    requireNewPassword(value, field, model) {
        if (!this.changingPassword && !this.settingPassword) return null;
        return VFG_Custom.requireNewPassword(value, field, model, undefined);
    }

    requirePassword(value) {
        if (!this.changingPassword && !this.settingPassword) return null;
        if (value === undefined || value === null || value === "") {
            return ["A password is required"];
        }
    }

    requirePasswordMatch(value, field, model) {
        if (!this.changingPassword && !this.settingPassword) return null;
        return VFG_Custom.requirePasswordMatch(value, field, model, undefined);
    }

    requireUsername(value, field, model) {
        if (!this.changingUsername) return null;
        return VFG_Custom.requireUsername(value, field, model, undefined);
    }

    setPassword() {
        this.success = false;
        this.settingPassword = true;
        this.schema.fields[2].visible = true;
        this.schema.fields[3].visible = true;
        this.schema.fields[4].visible = true;
    }

    usernameValidation() {
        if (this.selectedXferUsername
            && (this.selectedXferUsername.length < 6
                || this.selectedXferUsername.length > 24
                || !RegExp(/^[\w.@-]+$/).test(this.selectedXferUsername))) {
            this.validXferUsername = false;
            return "Invalid username";
        }
        this.validXferUsername = true;
        return true;
    }
}
