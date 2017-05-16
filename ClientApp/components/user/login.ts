import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { FormState } from '../../vue-form';

interface LoginViewModel {
    email: string,
    password: string,
    rememberUser: boolean,
    returnUrl: string,
    redirect: boolean,
    errors: Array<string>
}

interface LoginFormState extends FormState {
    email?: any,
    password?: any,
    rememberUser?: any
}

@Component
export default class LoginComponent extends Vue {
    formstate: LoginFormState = {};

    @Prop()
    returnUrl: string

    model: LoginViewModel = {
        email: '',
        password: '',
        rememberUser: false,
        returnUrl: this.returnUrl || '',
        redirect: false,
        errors: []
    };

    fieldClassName(field) {
        if (!field) {
            return '';
        }
        if ((field.$touched || field.$submitted) && field.$valid) {
            return 'text-success';
        }
        if ((field.$touched || field.$submitted) && field.$invalid) {
            return 'text-danger';
        }
    }

    submitting = false;

    passwordReset = false;
    forgottenPassword = false;
    forgotPassword(val: boolean) {
        this.forgottenPassword = val;
    }

    onSubmit() {
        if (this.forgottenPassword) {
            if (this.formstate.email.$valid) {
                fetch('/api/Account/ForgotPassword', { method: 'POST' });
                this.passwordReset = true;
                this.forgottenPassword = false;
            }
        } else if (this.formstate.$valid) {
            fetch('/api/Account/Login', { method: 'POST', body: this.model })
                .then(response => response.json() as Promise<LoginViewModel>)
                .then(data => {
                    if (data.redirect) {
                        this.$router.push(data.returnUrl);
                    } else {
                        this.model.errors = data.errors;
                    }
                });
        }
    }
}
