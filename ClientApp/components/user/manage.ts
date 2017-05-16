import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { checkResponse, ApiResponseViewModel } from '../../router';
import { FormState } from '../../vue-form';

interface ManageUserViewModel {
    email: string,
    oldPassword: string,
    newPassword: string,
    confirmPassword: string,
    errors: Array<String>
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
        fetch('/api/Manage/CancelPendingEmailChange',
            {
                headers: {
                    'Authorization': `bearer ${this.$store.state.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath));
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
                .catch(error => console.log(error));
        } else {
            this.changeSuccess = false;
        }
    }
}
