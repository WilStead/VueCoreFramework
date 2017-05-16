import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { FormState } from '../../vue-form';

interface ManageUserViewModel {
    email: string,
    oldPassword: string,
    newPassword: string,
    confirmPassword: string,
    errors: Object
}

@Component
export default class LoginComponent extends Vue {
    formstate: FormState = {};

    model: ManageUserViewModel = {
        email: '',
        oldPassword: '',
        newPassword: '',
        confirmPassword: '',
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

    emailModelErrorValidator(value) {
        return !this.getModelError('Email');
    }
    oldPasswordModelErrorValidator(value) {
        return !this.getModelError('OldPassword');
    }
    newPasswordModelErrorValidator(value) {
        return !this.getModelError('NewPassword');
    }
    getModelError(prop: string) {
        return this.model.errors[prop];
    }

    passwordMatch(value) {
        return value === this.model.newPassword;
    }
    
    changingEmail = false;
    changingPassword = false;
    settingPassword = false;
    changeEmail() {
        this.changingEmail = true;
        this.changeSuccess = false;
    }
    changePassword() {
        this.changingPassword = true;
        this.changeSuccess = false;
    }
    setPassword() {
        this.settingPassword = true;
        this.changeSuccess = false;
    }

    cancelChange() {
        this.changingEmail = false;
        this.changingPassword = false;
        this.settingPassword = false;
    }

    changeSuccess = false;

    onSubmit() {
        let url: string;
        if (this.changingEmail) {
            url = '/api/Manage/ChangeEmail';
        } else if (this.changingPassword) {
            url = '/api/Manage/ChangePassword';
        } else {
            url = 'api/Manage/SetPassword';
        }
        fetch(url, { method: 'POST', body: this.model })
            .then(response => response.json() as Promise<ManageUserViewModel>)
            .then(data => {
                if (Object.keys(data.errors).length === 0) {
                    this.changeSuccess = true;
                    this.cancelChange();
                } else {
                    this.model.errors = data.errors;
                }
            });
    }
}
