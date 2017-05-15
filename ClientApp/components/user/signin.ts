import Vue from 'vue';
import Component from 'vue-class-component';

interface SigninModel {
    username: string,
    password: string,
    rememberUser: boolean
}

@Component
export default class SigninComponent extends Vue {
    formstate: Object = {};
    model: SigninModel = {
        username: '',
        password: '',
        rememberUser: false
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

    onSubmit() { }
}
