import Vue from 'vue';
import Component from 'vue-class-component';

interface RegistrationModel {
    username: String,
    email: String,
    password: String,
    passwordConfirm: String
}

@Component
export default class RegisterComponent extends Vue {
    formstate: Object = {};
    model: RegistrationModel = {
        username: '',
        email: '',
        password: '',
        passwordConfirm: ''
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
    }
}
