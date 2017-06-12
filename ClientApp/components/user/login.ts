import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { checkResponse, ApiResponseViewModel } from '../../router';
import * as ErrorMsg from '../../error-msg';
import VueFormGenerator from 'vue-form-generator';

interface LoginViewModel {
    email: string,
    password: string,
    authProvider: string,
    rememberUser: boolean,
    returnUrl: string,
    redirect: boolean,
    token: string
    errors: Array<string>
}

interface AuthProviders {
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
    formOptions = {
        validateAfterChanged: true
    };
    isValid = false;
    model = {
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
    schema = {
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
