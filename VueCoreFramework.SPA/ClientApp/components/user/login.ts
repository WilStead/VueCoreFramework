import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import * as Api from '../../api';
import { authMgr, login } from '../../authorization';
import * as Store from '../../store/store';
import { checkResponse } from '../../router';
import * as ErrorMsg from '../../error-msg';
import VueFormGenerator from 'vue-form-generator';
import { Schema, VFGOptions } from '../../vfg/vfg';

/**
 * A ViewModel used to transfer information during user account login tasks.
 */
interface LoginViewModel {
    /**
     * The username or email address of the user account.
     */
    username: string;

    /**
     * The password for the user account.
     */
    password: string;

    /**
     * The name of a third-party authorization provider.
     */
    authProvider: string;

    /**
     * Indicates that the user wishes their authorization to be stored in the browser and used
     * again during future sessions, rather than forgotten after navigating away from the site.
     */
    rememberUser: boolean;
}

/**
 * Contains a list of all external authentication providers supported by the SPA framework.
 */
interface AuthProviders {
    /**
     * A list of all external authentication providers supported by the SPA framework.
     */
    providers: Array<string>
}

@Component
export default class LoginComponent extends Vue {
    @Prop()
    returnUrl: string

    components = {
        'vue-form-generator': VueFormGenerator.component
    };

    authProviderFacebook = false;
    authProviderGoogle = false;
    authProviderMicrosoft = false;
    errorMessage = '';
    forgottenPassword = false;
    formOptions: VFGOptions = {
        validateAfterChanged: true
    };
    isValid = false;
    model: LoginViewModel = {
        username: '',
        password: '',
        authProvider: '',
        rememberUser: false
    };
    passwordReset = false;
    schema: Schema = {
        fields: [
            {
                type: 'vuetifyText',
                inputType: 'string',
                model: 'username',
                placeholder: 'Username or email address',
                autocomplete: true,
                required: true,
                validator: [
                    VueFormGenerator.validators.string.locale({
                        fieldIsRequired: "A username or email address is required"
                    })
                ]
            },
            {
                type: 'vuetifyText',
                inputType: 'password',
                model: 'password',
                placeholder: 'Password',
                autocomplete: true,
                required: true,
                validator: [VueFormGenerator.validators.required]
            },
            {
                type: 'vuetifyCheckbox',
                placeholder: 'Remember me',
                model: 'rememberUser',
                required: true
            }
        ]
    };
    submitting = false;

    async mounted() {
        try {
            let response = await Api.getAuth('Account/GetAuthProviders');
            let data = await response.json() as AuthProviders;
            if (data.providers) {
                this.authProviderFacebook = data.providers.includes('Facebook');
                this.authProviderGoogle = data.providers.includes('Google');
                this.authProviderMicrosoft = data.providers.includes('Microsoft');
            }
        } catch (error) {
            ErrorMsg.logError("login.mounted", error);
        }
    }

    forgotPassword(val: boolean) {
        this.forgottenPassword = val;
    }

    onValidated(isValid: boolean, errors: Array<any>) {
        this.isValid = isValid;
    }

    async resetPassword() {
        if (!this.isValid) return;
        this.submitting = true;
        this.errorMessage = '';
        try {
            let response = await Api.postAuth('Account/ForgotPassword', undefined, JSON.stringify(this.model));
            if (!response.ok) {
                if (response.statusText) {
                    this.errorMessage = response.statusText;
                } else {
                    this.errorMessage = "A problem occurred.";
                }
                throw new Error(response.statusText);
            }
            this.passwordReset = true;
            this.forgottenPassword = false;
        } catch (error) {
            if (!this.errorMessage) {
                this.errorMessage = "A problem occurred. Your request was not received.";
                ErrorMsg.logError("login.resetPassword", error);
            }
        }
        this.submitting = false;
    }

    async onSignInProvider(provider: string) {
        this.submitting = true;
        this.errorMessage = '';
        this.model.authProvider = provider;
        try {
            let response = await Api.postAuth('Account/ExternalLogin', this.$route.fullPath, JSON.stringify(this.model));
            if (!response.ok) {
                if (response.statusText) {
                    this.errorMessage = response.statusText;
                } else {
                    this.errorMessage = "A problem occurred.";
                }
                throw new Error(response.statusText);
            } else {
                let result = await login(this.returnUrl);
                if (result !== "Success") {
                    this.errorMessage = result;
                }
            }
        } catch (error) {
            if (!this.errorMessage) {
                this.errorMessage = "A problem occurred. Login failed.";
                ErrorMsg.logError("login.onSignInProvider", error);
            }
        }
        this.submitting = false;
    }

    async onSubmit() {
        if (!this.isValid) return;
        this.submitting = true;
        this.errorMessage = '';

        try {
            let response = await Api.postAuth('Account/Login', undefined, JSON.stringify(this.model));
            if (!response.ok) {
                if (response.statusText) {
                    this.errorMessage = response.statusText;
                } else {
                    this.errorMessage = "A problem occurred.";
                }
                throw new Error(response.statusText);
            } else {
                let result = await login(this.returnUrl);
                if (result !== "Success") {
                    this.errorMessage = result;
                }
            }
        } catch (error) {
            if (!this.errorMessage) {
                this.errorMessage = "A problem occurred. Login failed.";
                ErrorMsg.logError("login.onSubmit", error);
            }
        }
        this.submitting = false;
    }
}
