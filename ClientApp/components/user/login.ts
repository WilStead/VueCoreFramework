import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { FormState } from '../../vue-form';

interface LoginViewModel {
    email: string,
    password: string,
    rememberUser: boolean,
    returnUrl: string,
    redirect: boolean,
    errors: Object
}

@Component
export default class LoginComponent extends Vue {
    formstate: FormState = {};

    @Prop()
    query: any

    model: LoginViewModel = {
        email: '',
        password: '',
        rememberUser: false,
        returnUrl: this.query ? this.query.returnUrl || '' : '',
        redirect: false,
        errors: {}
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

    modelErrorValidator(value) {
        return !this.getModelError('*');
    }
    emailModelErrorValidator(value) {
        return !this.getModelError('Email');
    }
    passwordModelErrorValidator(value) {
        return !this.getModelError('Password');
    }
    getModelError(prop: string) {
        return this.model.errors[prop];
    }

    submitting = false;

    passwordReset = false;
    forgottenPassword = false;
    forgotPassword(val: boolean) {
        this.forgottenPassword = val;
    }

    onSubmit() {
        if (this.forgottenPassword) {
            fetch('/api/Account/ForgotPassword', { method: 'POST' });
            this.passwordReset = true;
            this.forgottenPassword = false;
        } else {
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
