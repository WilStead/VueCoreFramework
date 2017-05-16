import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { FormState } from '../../vue-form';

interface RegisterViewModel {
    email: string,
    password: string,
    confirmPassword: string,
    returnUrl: string,
    redirect: boolean,
    errors: Array<string>
}

@Component
export default class RegisterComponent extends Vue {
    formstate: FormState = {};

    @Prop()
    returnUrl: any

    model: RegisterViewModel = {
        email: '',
        password: '',
        confirmPassword: '',
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

    passwordMatch(value) {
        return value === this.model.password;
    }

    onSubmit() {
        if (this.formstate.$valid) {
            fetch('/api/Account/Register',
                {
                    method: 'POST',
                    headers: {
                        'Accept': 'application/json',
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(this.model)
                })
                .then(response => response.json() as Promise<RegisterViewModel>)
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
