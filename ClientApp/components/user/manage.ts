import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
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
    selectedXferUsername = '';
    settingPassword = false;
    successMessage = "Success!";
    submitting = false;
    success = false;
    validXferUsername = false;
    xferData = false;
    xferLoading = false;
    xferUsernames: Array<string> = [];

    created() {
        fetch('/api/Account/HasPassword',
            {
                headers: {
                    'Authorization': `bearer ${this.$store.state.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.response === "yes") {
                    this.hasPassword = true;
                }
            })
            .catch(error => ErrorMsg.logError("manage.created.fetchPW", new Error(error)));
    }

    mounted() {
        fetch('/api/Account/GetUserAuthProviders',
            {
                headers: {
                    'Authorization': `bearer ${this.$store.state.token}`
                }
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
                ErrorMsg.logError("manage.mounted", new Error(error));
            });
    }

    cancelChange() {
        this.success = false;
        this.changingEmail = false;
        this.changingPassword = false;
        this.changingUsername = false;
        this.settingPassword = false;
    }

    changeEmail() {
        this.success = false;
        this.changingEmail = true;
    }

    changePassword() {
        this.success = false;
        this.changingPassword = true;
    }

    changeUsername() {
        this.success = false;
        this.changingUsername = true;
    }

    loadXferUsernames() {
        this.xferLoading = true;
        fetch('/api/Manage/LoadXferUsernames',
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${this.$store.state.token}`
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
            .catch(error => { });
    }

    onDeleteAccount() {
        this.success = false;
        this.submitting = true;
        let url = '/api/Manage/DeleteAccount';
        if (this.selectedXferUsername) {
            url += `/${this.selectedXferUsername}`;
        }
        fetch(url,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${this.$store.state.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                this.submitting = false;
                if (data.error) {
                    this.model.errors = [data.error];
                } else {
                    // On success, the user is no longer a signed-in user (since they no longer have an account).
                    this.$store.state.username = 'user';
                    this.$store.state.email = 'user@example.com';
                    this.$store.state.token = '';
                    localStorage.removeItem('token');
                    this.successMessage = "Your account has been successfully deleted. You will be returned to the homepage shortly.";
                    this.success = true;
                    setTimeout(() => this.$router.push('/'), 5000);
                }
            })
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
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
                    'Authorization': `bearer ${this.$store.state.token}`
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
                ErrorMsg.showErrorMsgAndLog("manage.onSignInProviderAdd", "A problem occurred. Login failed.", new Error(error));
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
                    'Authorization': `bearer ${this.$store.state.token}`
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
                ErrorMsg.showErrorMsgAndLog("manage.onSignInProviderRemove", "A problem occurred. Login failed.", new Error(error));
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
                    'Authorization': `bearer ${this.$store.state.token}`
                },
                body: JSON.stringify(this.model)
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ManageUserViewModel>)
            .then(data => {
                if (Object.keys(data.errors).length === 0) {
                    this.success = true;
                    this.successMessage = "Success!";
                    this.cancelChange();
                } else {
                    this.success = false;
                    this.model.errors = data.errors;
                }
            })
            .catch(error => {
                ErrorMsg.showErrorMsgAndLog("manage.onSubmit", "A problem occurred. Your request was not received.", new Error(error));
                this.success = false;
            });
    }

    onValidated(isValid: boolean, errors: Array<any>) {
        this.isValid = isValid;
    }

    onXferChange(newValue: boolean) {
        if (newValue) {
            this.loadXferUsernames();
        }
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
    }

    usernameValidation() {
        if (this.selectedXferUsername
            && (this.selectedXferUsername.length < 6
                || this.selectedXferUsername.length > 24
                || !RegExp(/^[\w.@-]+&/).test(this.selectedXferUsername))) {
            this.validXferUsername = false;
            return "Invalid username";
        }
        this.validXferUsername = true;
        return true;
    }
}
