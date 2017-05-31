import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { checkResponse, ApiResponseViewModel } from '../../../router';
import VueFormGenerator from 'vue-form-generator';
import * as VFG_Custom from '../../../vfg/vfg-custom-validators';
import * as ErrorMsg from '../../../error-msg';

interface ResetPasswordViewModel {
    email: string,
    newPassword: string,
    confirmPassword: string,
    code: string,
    errors: Array<String>
}

@Component
export default class ResetComponent extends Vue {
    @Prop()
    code: string;

    components = {
        'vue-form-generator': VueFormGenerator.component
    };

    model: ResetPasswordViewModel = {
        email: '',
        newPassword: '',
        confirmPassword: '',
        code: this.code,
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
                required: true,
                validator: VFG_Custom.requireEmail
            },
            {
                type: 'input',
                inputType: 'password',
                model: 'newPassword',
                placeholder: 'New Password',
                min: 6,
                max: 24,
                required: true,
                validator: VFG_Custom.requireNewPassword
            },
            {
                type: 'input',
                inputType: 'password',
                model: 'confirmPassword',
                placeholder: 'Confirm Password',
                required: true,
                validator: VFG_Custom.requirePasswordMatch
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

    changeSuccess = false;
    submitting = false;

    onSubmit() {
        if (!this.isValid) return;
        this.submitting = true;
        fetch('api/Account/ResetPassword',
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(this.model)
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ResetPasswordViewModel>)
            .then(data => {
                this.submitting = false;
                if (Object.keys(data.errors).length === 0) {
                    this.changeSuccess = true;
                } else {
                    this.model.errors = data.errors;
                }
            })
            .catch(error => {
                this.submitting = false;
                ErrorMsg.showErrorMsgAndLog("reset.onSubmit", "A problem occurred. Your request was not received.", error);
            });
    }
}
