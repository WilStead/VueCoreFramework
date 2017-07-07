import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as Store from '../../store/store';
import { authenticate } from '../../authorization';
import { checkResponse, ApiResponseViewModel } from '../../router';
import { OperationReply } from '../../store/repository';
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

    /**
     * A list of errors generated during the operation.
     */
    errors: Array<String>;
}

/**
 * Contains lists of external authentication providers supported by the SPA framework.
 */
interface AuthProviders {
    /**
     * An error message.
     */
    error?: string;

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
        authProvider: '',
        errors: []
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
        fetch('/api/Account/HasPassword',
            {
                headers: {
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.error) {
                    ErrorMsg.showErrorMsgAndLog('user/manage.created', data.error, new Error(`Error in manage.created: ${data.error}`));
                }
                else if (data.response === "yes") {
                    this.hasPassword = true;
                }
            })
            .catch(error => ErrorMsg.logError("user/manage.created.fetchPW", new Error(error)));
    }

    mounted() {
        fetch('/api/Account/GetUserAuthProviders',
            {
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => response.json() as Promise<AuthProviders>)
            .then(data => {
                if (data.error) {
                    ErrorMsg.showErrorMsgAndLog('user/manage.mounted', data.error, new Error(`Error in manage.mounted: ${data.error}`));
                }
                else {
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
                }
            })
            .catch(error => {
                ErrorMsg.logError("user/manage.mounted", new Error(error));
            });
        fetch('/api/Manage/GetCultures',
            {
                headers: {
                    'Accept': 'application/json'
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
        fetch('/api/Manage/LoadXferUsernames',
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<Array<string>>)
            .then(data => {
                // Errors are ignored; the list simply doesn't populate, requiring manual entry.
                if (!data['error']) {
                    this.xferUsernames = data;
                }
                this.xferLoading = false;
                this.xferData = true;
            })
            .catch(error => {
                this.xferLoading = false;
            });
    }

    onCultureChange(value: string) {
        this.success = false;
        this.submitting = true;
        this.model.errors = [];
        fetch(`/api/Manage/SetCulture/${value}`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.error) {
                    this.model.errors = [data.error];
                } else {
                    this.successMessage = "Your preferred culture has been updated.";
                    this.success = true;
                }
                this.submitting = false;
            })
            .catch(error => {
                this.model.errors = ["A problem occurred."];
                ErrorMsg.logError("user/manage.onCultureChange", new Error(error));
                this.submitting = false;
            });
    }

    onDeleteAccount() {
        this.success = false;
        this.submitting = true;
        let url = '/api/Manage/DeleteAccount';
        if (this.selectedXferUsername) {
            url += `?xferUsername=${this.selectedXferUsername}`;
        }
        fetch(url,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                this.submitting = false;
                if (data.error) {
                    this.model.errors = [data.error];
                } else {
                    this.deleteAccountDialog = false;
                    this.successMessage = "Your request to delete your account has been received. An email has just been sent to the address on your account. Please click on the link in this email to confirm the deletion of your account.";
                    this.success = true;
                }
            })
            .catch(error => {
                this.model.errors = ["A problem occurred. Your account has not been deleted."];
                ErrorMsg.logError("user/manage.onDeleteAccount", new Error(error));
                this.submitting = false;
            });
    }

    onSignInProviderAdd(provider: string) {
        this.success = false;
        this.submitting = true;
        this.model.errors = [];
        this.model.authProvider = provider;
        fetch('/api/Manage/LinkLogin',
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                },
                body: JSON.stringify(this.model)
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ManageUserViewModel>)
            .then(data => {
                if (data.errors) {
                    this.model.errors = data.errors;
                } else {
                    this.successMessage = "Your accounts have been successfully linked.";
                    this.success = true;
                }
                this.submitting = false;
            })
            .catch(error => {
                this.model.errors = ["A problem occurred. Login failed."];
                ErrorMsg.logError("user/manage.onSignInProviderAdd", new Error(error));
                this.submitting = false;
            });
    }

    onSignInProviderRemove(provider: string) {
        this.success = false;
        this.submitting = true;
        this.model.errors = [];
        this.model.authProvider = provider;
        fetch('/api/Manage/RemoveLogin',
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                },
                body: JSON.stringify(this.model)
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ManageUserViewModel>)
            .then(data => {
                if (data.errors) {
                    this.model.errors = data.errors;
                } else {
                    this.success = true;
                }
                this.submitting = false;
            })
            .catch(error => {
                this.model.errors = ["A problem occurred. Login failed."];
                ErrorMsg.logError("user/manage.onSignInProviderRemove", new Error(error));
                this.submitting = false;
            });
    }

    onSubmit() {
        this.success = false;
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
        fetch(url,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                },
                body: JSON.stringify(this.model)
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ManageUserViewModel>)
            .then(data => {
                if (Object.keys(data.errors).length === 0) {
                    this.success = true;
                    this.successMessage = "Success!";
                    authenticate(true);
                    this.cancelChange();
                } else {
                    this.success = false;
                    this.model.errors = data.errors;
                }
            })
            .catch(error => {
                this.model.errors = ["A problem occurred. Your request was not received."];
                ErrorMsg.logError("user/manage.onSubmit", new Error(error));
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
