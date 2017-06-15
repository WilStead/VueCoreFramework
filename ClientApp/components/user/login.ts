import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { checkResponse, ApiResponseViewModel } from '../../router';
import * as ErrorMsg from '../../error-msg';
import VueFormGenerator from 'vue-form-generator';
import { Schema, VFGOptions } from '../../vfg/vfg';

/**
 * A ViewModel used to transfer information during user account login tasks.
 */
interface LoginViewModel {
    /**
     * The email address of the user account.
     */
    email: string;

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

    /**
     * An optional URL to which the user will be redirected.
     */
    returnUrl: string;

    /**
     * Indicates that the user is to be redirected to another page.
     */
    redirect: boolean;

    /**
     * A JWT bearer token.
     */
    token: string;

    /**
     * A list of errors generated during the operation.
     */
    errors: Array<string>;
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
    forgottenPassword = false;
    formOptions: VFGOptions = {
        validateAfterChanged: true
    };
    isValid = false;
    model: LoginViewModel = {
        email: '',
        password: '',
        authProvider: '',
        rememberUser: false,
        returnUrl: this.returnUrl || '',
        redirect: false,
        token: '',
        errors: []
    };
    passwordReset = false;
    schema: Schema = {
        fields: [
            {
                type: 'vuetifyText',
                inputType: 'email',
                model: 'email',
                placeholder: 'Email',
                autocomplete: true,
                required: true,
                validator: [
                    VueFormGenerator.validators.email.locale({
                        fieldIsRequired: "A valid email address is required",
                        invalidEmail: "A valid email address is required"
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
                model: 'rememberUser'
            }
        ]
    };
    submitting = false;

    mounted() {
        fetch('/api/Account/GetAuthProviders')
            .then(response => response.json() as Promise<AuthProviders>)
            .then(data => {
                if (data.providers) {
                    this.authProviderFacebook = data.providers.indexOf('Facebook') !== -1;
                    this.authProviderGoogle = data.providers.indexOf('Google') !== -1;
                    this.authProviderMicrosoft = data.providers.indexOf('Microsoft') !== -1;
                }
            })
            .catch(error => {
                ErrorMsg.logError("login.mounted", new Error(error));
            });
    }

    forgotPassword(val: boolean) {
        this.forgottenPassword = val;
    }

    onValidated(isValid: boolean, errors: Array<any>) {
        this.isValid = isValid;
    }

    resetPassword() {
        if (!this.isValid) return;
        this.submitting = true;
        this.model.errors = [];
        fetch('/api/Account/ForgotPassword',
            {
                method: 'POST',
                headers: {
                    'Authorization': `bearer ${this.$store.state.token}`
                }
            })
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.error) {
                    this.model.errors.push(data.error);
                } else {
                    this.passwordReset = true;
                    this.forgottenPassword = false;
                    this.submitting = false;
                }
            })
            .catch(error => {
                this.model.errors.push("A problem occurred. Your request was not received.");
                ErrorMsg.logError("login.resetPassword", new Error(error));
            });
    }

    onSignInProvider(provider: string) {
        this.submitting = true;
        this.model.errors = [];
        this.model.authProvider = provider;
        fetch('/api/Account/ExternalLogin',
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(this.model)
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<LoginViewModel>)
            .then(data => {
                if (data.token) {
                    this.$store.commit('setToken', data.token);
                    if (this.model.rememberUser) {
                        localStorage.setItem('token', data.token);
                    }
                }
                if (data.redirect) {
                    this.$router.push(data.returnUrl);
                } else {
                    this.model.errors = data.errors;
                }
                this.submitting = false;
            })
            .catch(error => {
                this.model.errors.push("A problem occurred. Login failed.");
                ErrorMsg.logError("login.onSubmit", new Error(error));
                this.submitting = false;
            });
    }

    onSubmit() {
        if (!this.isValid) return;
        this.submitting = true;
        this.model.errors = [];
        fetch('/api/Account/Login',
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(this.model)
            })
            .then(response => response.json() as Promise<LoginViewModel>)
            .then(data => {
                if (data.token) {
                    this.$store.commit('setToken', data.token);
                    if (this.model.rememberUser) {
                        localStorage.setItem('token', data.token);
                    }
                }
                if (data.redirect) {
                    this.$router.push(data.returnUrl);
                } else {
                    this.model.errors = data.errors;
                }
                this.submitting = false;
            })
            .catch(error => {
                this.model.errors.push("A problem occurred. Login failed.");
                ErrorMsg.logError("login.onSubmit", new Error(error));
                this.submitting = false;
            });
    }
}
