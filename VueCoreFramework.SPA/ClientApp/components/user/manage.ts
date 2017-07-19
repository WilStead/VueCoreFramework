import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as Api from '../../api';
import * as Store from '../../store/store';
import { authenticate } from '../../authorization';
import { checkResponse } from '../../router';
import { OperationReply } from '../../store/repository';
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

    created() {
        Api.getApi('/api/Account/HasPassword', this.$route.fullPath)
            .then(response => {
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
            })
            .catch(error => {
                if (this.errors.length === 0) {
                    ErrorMsg.logError("user/manage.created.fetchPW", new Error(error));
                }
            });
    }

    mounted() {
        Api.getApi('/api/Account/GetUserAuthProviders', this.$route.fullPath)
            .then(response => {
                if (!response.ok) {
                    if (response.statusText) {
                        this.errors.push(response.statusText);
                    } else {
                        this.errors.push("A problem occurred.");
                    }
                    throw new Error("CODE");
                }
                return response;
            })
            .then(response => response.json() as Promise<AuthProviders>)
            .then(data => {
                if (data.providers) {
                    this.authProviderFacebook = data.providers.indexOf('Facebook') !== -1;
                    this.authProviderGoogle = data.providers.indexOf('Google') !== -1;
                    this.authProviderMicrosoft = data.providers.indexOf('Microsoft') !== -1;
                }
                if (data.userProviders) {
                    this.authProviderFacebookUser = data.providers.indexOf('Facebook') !== -1;
                    this.authProviderGoogleUser = data.providers.indexOf('Google') !== -1;
                    this.authProviderMicrosoftUser = data.providers.indexOf('Microsoft') !== -1;
                }
            })
            .catch(error => {
                if (error !== "CODE") {
                    this.errors.push("A problem occurred.");
                    ErrorMsg.logError("user/manage.mounted", new Error(error));
                }
            });
        Api.callApi('/api/Manage/GetCultures',
            {
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture
                }
            })
            .then(response => response.json() as Promise<string[]>)
            .then(data => {
                this.cultures = data;
                this.cultures.unshift("<default>");
            })
            .catch(error => {
                ErrorMsg.logError("user/manage.mounted", new Error(error));
            });
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

    loadXferUsernames() {
        this.xferLoading = true;
        Api.getApi('/api/Manage/LoadXferUsernames', this.$route.fullPath)
            .then(response => {
                if (!response.ok) {
                    throw new Error("CODE");
                }
                return response;
            })
            .then(response => response.json() as Promise<Array<string>>)
            .then(data => {
                this.xferUsernames = data;
                this.xferLoading = false;
                this.xferData = true;
            })
            .catch(error => {
                // Errors are ignored; the list simply doesn't populate, requiring manual entry.
                this.xferLoading = false;
            });
    }

    onCultureChange(value: string) {
        this.success = false;
        this.submitting = true;
        this.errors = [];

        if (this.$store.state.userState.culture === value
            || (value === "<default>"
                && this.$store.state.userState.culture === defaultCulture)) {
            return;
        }

        Api.postApi(`/api/Manage/SetCulture/${value}`, this.$route.fullPath)
            .then(response => {
                if (!response.ok) {
                    if (response.statusText) {
                        this.errors = response.statusText.split(';');
                    } else {
                        this.errors.push("A problem occurred.");
                    }
                    throw new Error("CODE");
                }
                return response;
            })
            .then(response => {
                setCulture(value);
                this.successMessage = "Your preferred culture has been updated.";
                this.success = true;
                this.submitting = false;
            })
            .catch(error => {
                if (error !== "CODE") {
                    this.errors.push("A problem occurred.");
                    ErrorMsg.logError("user/manage.onCultureChange", new Error(error));
                }
                this.submitting = false;
            });
    }

    onDeleteAccount() {
        this.success = false;
        this.errors = [];
        this.submitting = true;
        let url = '/api/Manage/DeleteAccount';
        if (this.selectedXferUsername) {
            url += `?xferUsername=${this.selectedXferUsername}`;
        }
        Api.postApi(url, this.$route.fullPath)
            .then(response => {
                if (!response.ok) {
                    if (response.statusText) {
                        this.errors = response.statusText.split(';');
                    } else {
                        this.errors.push("A problem occurred.");
                    }
                    throw new Error("CODE");
                }
                return response;
            })
            .then(data => {
                this.deleteAccountDialog = false;
                this.successMessage = "Your request to delete your account has been received. An email has just been sent to the address on your account. Please click on the link in this email to confirm the deletion of your account.";
                this.success = true;
                this.submitting = false;
            })
            .catch(error => {
                if (error !== "CODE") {
                    this.errors.push("A problem occurred. Your account has not been deleted.");
                    ErrorMsg.logError("user/manage.onDeleteAccount", new Error(error));
                }
                this.submitting = false;
            });
    }

    onSignInProviderAdd(provider: string) {
        this.success = false;
        this.submitting = true;
        this.errors = [];
        this.model.authProvider = provider;
        Api.callApi('/api/Manage/LinkLogin',
            {
                method: 'POST',
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Content-Type': `application/json;v=${this.$store.state.apiVer}`,
                    'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                },
                body: JSON.stringify(this.model)
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    if (response.statusText) {
                        this.errors = response.statusText.split(';');
                    } else {
                        this.errors.push("A problem occurred.");
                    }
                    throw new Error("CODE");
                }
                return response;
            })
            .then(response => {
                this.successMessage = "Your accounts have been successfully linked.";
                this.success = true;
                this.submitting = false;
            })
            .catch(error => {
                if (error !== "CODE") {
                    this.errors.push("A problem occurred. Login failed.");
                    ErrorMsg.logError("user/manage.onSignInProviderAdd", new Error(error));
                }
                this.submitting = false;
            });
    }

    onSignInProviderRemove(provider: string) {
        this.success = false;
        this.submitting = true;
        this.errors = [];
        this.model.authProvider = provider;
        Api.callApi('/api/Manage/RemoveLogin',
            {
                method: 'POST',
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Content-Type': `application/json;v=${this.$store.state.apiVer}`,
                    'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                },
                body: JSON.stringify(this.model)
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    if (response.statusText) {
                        this.errors = response.statusText.split(';');
                    } else {
                        this.errors.push("A problem occurred.");
                    }
                    throw new Error("CODE");
                }
                return response;
            })
            .then(response => {
                this.success = true;
                this.submitting = false;
            })
            .catch(error => {
                if (error !== "CODE") {
                    this.errors.push("A problem occurred. Login failed.");
                    ErrorMsg.logError("user/manage.onSignInProviderRemove", new Error(error));
                }
                this.submitting = false;
            });
    }

    onSubmit() {
        this.success = false;
        this.errors = [];
        if (!this.isValid) return;
        let url: string;
        if (this.changingEmail) {
            url = '/api/Manage/ChangeEmail';
        } else if (this.changingPassword) {
            url = '/api/Manage/ChangePassword';
        } else if (this.changingUsername) {
            url = '/api/Manage/ChangeUsername';
        } else {
            url = 'api/Manage/SetPassword';
        }
        Api.callApi(url,
            {
                method: 'POST',
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Content-Type': `application/json;v=${this.$store.state.apiVer}`,
                    'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                },
                body: JSON.stringify(this.model)
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    if (response.statusText) {
                        this.errors = response.statusText.split(';');
                    } else {
                        this.errors.push("A problem occurred.");
                    }
                    throw new Error("CODE");
                }
                return response;
            })
            .then(response => {
                this.success = true;
                this.successMessage = "Success!";
                authenticate(true);
                this.cancelChange();
            })
            .catch(error => {
                if (error != "CODE") {
                    this.errors.push("A problem occurred. Your request was not received.");
                    ErrorMsg.logError("user/manage.onSubmit", new Error(error));
                }
                this.success = false;
            });
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
