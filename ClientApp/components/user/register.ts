import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import VueFormGenerator from 'vue-form-generator';
import * as ErrorMsg from '../error/error-msg';

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
    @Prop()
    returnUrl: any

    components = {
        'vue-form-generator': VueFormGenerator.component
    };

    model: RegisterViewModel = {
        email: '',
        password: '',
        confirmPassword: '',
        returnUrl: this.returnUrl || '',
        redirect: false,
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
                validator: [
                    VueFormGenerator.validators.email.locale({
                        fieldIsRequired: "a valid email address is required",
                        invalidEmail: "a valid email address is required"
                    })
                ]
            },
            {
                type: 'input',
                inputType: 'password',
                model: 'password',
                placeholder: 'Password',
                min: 6,
                max: 24,
                pattern: /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*])/,
                required: true,
                validator: [
                    VueFormGenerator.validators.string.locale({
                        fieldIsRequired: "a password is required",
                        textTooSmall: "passwords must be at least {1} characters long",
                        textTooBig: "passwords cannot be longer than {1} characters",
                        thisNotText: "invalid characters in password"
                    }),
                    VueFormGenerator.validators.regexp.locale({
                        fieldIsRequired: "a password is required",
                        invalidFormat: "passwords must contain at least one of each of the following: lower-case letter, upper-case letter, number, and special character like !@#$%^&*"
                    })
                ]
            },
            {
                type: 'input',
                inputType: 'password',
                model: 'confirmPassword',
                placeholder: 'Confirm Password',
                validator: this.requirePasswordMatch
            },
            {
                type: 'submit',
                buttonText: 'Register',
                validateBeforeSubmit: true,
                onSubmit: this.onSubmit
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

    requirePasswordMatch(value, field, model) {
        if (value === undefined || value === null || value === "") {
            return ["you must confirm your password"];
        }
        if (value !== model.newPassword) {
            return ["your passwords must match"];
        }
    }

    onSubmit() {
        if (!this.isValid) return;
        this.model.errors = [];
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
            })
            .catch(error => ErrorMsg.showErrorMsgAndLog("A problem occurred. Your account acould not be registered.", error));
    }
}
