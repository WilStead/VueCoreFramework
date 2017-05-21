import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import * as ErrorMsg from '../error/error-msg';
import VueFormGenerator from 'vue-form-generator';

interface LoginViewModel {
    email: string,
    password: string,
    rememberUser: boolean,
    returnUrl: string,
    redirect: boolean,
    token: string
    errors: Array<string>
}

@Component
export default class LoginComponent extends Vue {
    @Prop()
    returnUrl: string

    components = {
        'vue-form-generator': VueFormGenerator.component
    };

    model = {
        email: '',
        password: '',
        rememberUser: false,
        returnUrl: this.returnUrl || '',
        redirect: false,
        token: '',
        errors: []
    };

    schema = {
        fields: [
            {
                type: 'input',
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
                type: 'input',
                inputType: 'password',
                model: 'password',
                placeholder: 'Password',
                autocomplete: true,
                required: true,
                validator: [VueFormGenerator.validators.required]
            },
            {
                type: 'checkbox',
                label: 'Remember me',
                model: 'rememberUser'
            }
        ]
    };

    formOptions = {
        validateAfterChanged: true
    };

    isValid = false;
    onValidated(isValid: boolean, errors: Array<any>) {
        this.isValid = isValid;
    }

    submitting = false;

    passwordReset = false;
    forgottenPassword = false;
    forgotPassword(val: boolean) {
        this.forgottenPassword = val;
    }

    resetPassword() {
        if (!this.isValid) return;
        this.model.errors = [];
        fetch('/api/Account/ForgotPassword',
            {
                method: 'POST',
                headers: {
                    'Authorization': `bearer ${this.$store.state.token}`
                }
            })
            .catch(error => ErrorMsg.showErrorMsgAndLog("A problem occurred. Your request was not received.", error));
        this.passwordReset = true;
        this.forgottenPassword = false;
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
                ErrorMsg.showErrorMsgAndLog("A problem occurred. Login failed.", error);
                this.submitting = false;
            });
    }
}
