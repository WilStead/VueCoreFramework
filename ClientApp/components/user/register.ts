import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import * as ErrorMsg from '../../error-msg';
import VueFormGenerator from 'vue-form-generator';
import { Schema, VFGOptions } from '../../vfg/vfg';
import * as VFG_Custom from '../../vfg/vfg-custom-validators';

/**
 * A ViewModel used to transfer information during user account registration tasks.
 */
interface RegisterViewModel {
    /**
     * The email address of the user account.
     */
    email: string;

    /**
     * The password for the user account.
     */
    password: string;

    /**
     * The password for the user account, repeated.
     */
    confirmPassword: string;

    /**
     * An optional URL to which the user will be redirected.
     */
    returnUrl: string;

    /**
     * Indicates that the user is to be redirected to another page.
     */
    redirect: boolean;

    /**
     * A list of errors generated during the operation.
     */
    errors: Array<string>;
}

@Component
export default class RegisterComponent extends Vue {
    @Prop()
    returnUrl: string

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

    schema: Schema = {
        fields: [
            {
                type: 'vuetifyText',
                inputType: 'email',
                model: 'email',
                placeholder: 'Email',
                autocomplete: true,
                required: true,
                validator: [
                    VueFormGenerator.validators.email.locale({
                        fieldIsRequired: "A valid email address is required",
                        invalidEmail: "A valid email address is required"
                    })
                ]
            },
            {
                type: 'vuetifyText',
                inputType: 'password',
                model: 'password',
                placeholder: 'Password',
                min: 6,
                max: 24,
                pattern: /^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#\$%\^&\*_])/,
                required: true,
                validator: [
                    VFG_Custom.string_regexp.locale({
                        fieldIsRequired: "A password is required",
                        invalidFormat: "Passwords must contain a lower-case letter, upper-case letter, number, and special character like !@#$%^&*_"
                    })
                ]
            },
            {
                type: 'vuetifyText',
                inputType: 'password',
                model: 'confirmPassword',
                placeholder: 'Confirm Password',
                validator: this.requirePasswordMatch
            }
        ]
    };

    formOptions: VFGOptions = {
        validateAfterChanged: true
    };

    isValid = false;
    onValidated(isValid: boolean, errors: Array<any>) {
        this.isValid = isValid;
    }

    requirePasswordMatch(value, field, model) {
        if (value === undefined || value === null || value === "") {
            return ["You must confirm your password"];
        }
        if (value !== model.password) {
            return ["Your passwords must match"];
        }
    }

    submitting = false;
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
            .catch(error => ErrorMsg.showErrorMsgAndLog("register.onSubmit", "A problem occurred. Your account acould not be registered.", new Error(error)));
    }
}
