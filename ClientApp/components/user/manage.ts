import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { checkAuthorization, ApiResponseViewModel } from '../../router';
import { FormState } from '../../vue-form';

interface ManageUserViewModel {
    email: string,
    oldPassword: string,
    newPassword: string,
    confirmPassword: string,
    errors: Object
}

interface ManageUserFormState extends FormState {
    email?: any,
    oldPassword?: any,
    newPassword?: any,
    confirmPassword?: any
}

@Component
export default class ManageUserComponent extends Vue {
    formstate: ManageUserFormState = {};

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
    successMessage = "Success!";

    pendingEmailChange = false;
    created() {
        fetch('/api/Account/HasPendingEmailChange')
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.response === "yes") {
                    this.pendingEmailChange = true;
                }
            });
    }
    cancelEmailChange() {
        fetch('/api/Manage/CancelPendingEmailChange');
        this.changeSuccess = true;
        this.successMessage = "Okay, your request to update your email has been canceled. Please confirm your original email account by clicking on the link that was just sent.";
    }

    onSubmit() {
        let url: string;
        let proceed = true;
        if (this.changingEmail) {
            proceed = this.formstate.email.$valid;
            url = '/api/Manage/ChangeEmail';
        } else {
            if (this.formstate.newPassword.$invalid ||
                this.formstate.confirmPassword.$invalid) {
                proceed = false;
            } else if (this.changingPassword) {
                proceed = this.formstate.oldPassword.$valid;
                url = '/api/Manage/ChangePassword';
            } else {
                url = 'api/Manage/SetPassword';
            }
        }
        if (proceed) {
            fetch(url, { method: 'POST', body: this.model })
                .then(response => response.json() as Promise<ManageUserViewModel>)
                .then(data => {
                    if (Object.keys(data.errors).length === 0) {
                        this.changeSuccess = true;
                        this.successMessage = "Success!";
                        this.cancelChange();
                    } else {
                        this.changeSuccess = false;
                        this.model.errors = data.errors;
                    }
                });
        } else {
            this.changeSuccess = false;
        }
    }
}
