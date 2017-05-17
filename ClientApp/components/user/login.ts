import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { FormState } from '../../vue-form';

interface LoginViewModel {
    email: string,
    password: string,
    rememberUser: boolean,
    returnUrl: string,
    redirect: boolean,
    token: string
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
        token: '',
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
            this.model.errors = [];
            if (this.formstate.email.$valid) {
                fetch('/api/Account/ForgotPassword',
                    {
                        method: 'POST',
                        headers: {
                            'Authorization': `bearer ${this.$store.state.token}`
                        }
                    });
                this.passwordReset = true;
                this.forgottenPassword = false;
            }
        } else if (this.formstate.$valid) {
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
                    console.log(error);
                    this.submitting = false;
                });
        }
    }
}
