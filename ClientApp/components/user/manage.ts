﻿import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { checkResponse, ApiResponseViewModel } from '../../router';
import VueFormGenerator from 'vue-form-generator';
import * as ErrorMsg from '../error/error-msg';

interface ManageUserViewModel {
    email: string,
    oldPassword: string,
    newPassword: string,
    confirmPassword: string,
    errors: Array<String>
}

@Component
export default class ManageUserComponent extends Vue {
    hasPassword = false;
    pendingEmailChange = false;
    created() {
        fetch('/api/Account/HasPassword',
            {
                headers: {
                    'Authorization': `bearer ${this.$store.state.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.response === "yes") {
                    this.hasPassword = true;
                }
            });
        fetch('/api/Account/HasPendingEmailChange',
            {
                headers: {
                    'Authorization': `bearer ${this.$store.state.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.response === "yes") {
                    this.pendingEmailChange = true;
                }
            });
    }

    components = {
        'vue-form-generator': VueFormGenerator.component
    };

    model: ManageUserViewModel = {
        email: '',
        oldPassword: '',
        newPassword: '',
        confirmPassword: '',
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
                validator: this.requireEmail,
                visible: () => this.changingEmail
            },
            {
                type: 'input',
                inputType: 'password',
                model: 'oldPassword',
                placeholder: 'Old Password',
                autocomplete: true,
                validator: this.requirePassword,
                visible: () => this.changingPassword || this.settingPassword
            },
            {
                type: 'input',
                inputType: 'password',
                model: 'newPassword',
                placeholder: 'New Password',
                min: 6,
                max: 24,
                validator: this.requireNewPassword,
                visible: () => this.changingPassword || this.settingPassword
            },
            {
                type: 'input',
                inputType: 'password',
                model: 'confirmPassword',
                placeholder: 'Confirm Password',
                validator: this.requirePasswordMatch,
                visible: () => this.changingPassword || this.settingPassword
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

    requireEmail(value) {
        if (!this.changingEmail) return null;
        if (value === undefined || value === null || value === "") {
            return ["A valid email address is required"];
        }
        let re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        if (!re.test(value))
            return ["A valid email address is required"];
    }

    requirePassword(value) {
        if (!this.changingPassword && !this.settingPassword) return null;
        if (value === undefined || value === null || value === "") {
            return ["A password is required"];
        }
    }

    requirePasswordMatch(value, field, model) {
        if (!this.changingPassword && !this.settingPassword) return null;
        if (value === undefined || value === null || value === "") {
            return ["You must confirm your new password"];
        }
        if (value !== model.newPassword) {
            return ["Your passwords must match"];
        }
    }

    requireNewPassword(value, field) {
        if (!this.changingPassword && !this.settingPassword) return null;
        if (value === undefined || value === null || value === "") {
            return ["A password is required"];
        }
        if (value.length < field.min || value.length > field.max) {
            return [`Passwords must be between ${field.min} and ${field.max} characters`];
        }
        let re = /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])/;
        if (!re.test(value))
            return ["Passwords must contain at least one of each of the following: lower-case letter, upper-case letter, number, and special character like !@#$%^&*"];
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

    cancelEmailChange() {
        fetch('/api/Manage/CancelPendingEmailChange',
            {
                headers: {
                    'Authorization': `bearer ${this.$store.state.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(() => {
                this.changeSuccess = true;
                this.successMessage = "Okay, your request to update your email has been canceled. Please confirm your original email account by clicking on the link that was just sent.";
            })
            .catch(error => ErrorMsg.showErrorMsgAndLog("A problem occurred. Your request was not received.", error));
    }

    onSubmit() {
        if (!this.isValid) return;
        let url: string;
        if (this.changingEmail) {
            url = '/api/Manage/ChangeEmail';
        } else if (this.changingPassword) {
            url = '/api/Manage/ChangePassword';
        } else {
            url = 'api/Manage/SetPassword';
        }
        fetch(url,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${this.$store.state.token}`
                },
                body: JSON.stringify(this.model)
            })
            .then(response => checkResponse(response, this.$route.fullPath))
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
            })
            .catch(error => {
                ErrorMsg.showErrorMsgAndLog("A problem occurred. Your request was not received.", error);
                this.changeSuccess = false;
            });
    }
}
