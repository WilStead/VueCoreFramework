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
     * The username of the user account.
     */
    username: string;

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

    formOptions: VFGOptions = {
        validateAfterChanged: true
    };
    isValid = false;
    model: RegisterViewModel = {
        username: '',
        email: '',
        password: '',
        confirmPassword: '',
        errors: []
    };
    schema: Schema = {
        fields: [
            {
                type: 'vuetifyText',
                inputType: 'string',
                model: 'username',
                placeholder: 'Username',
                autocomplete: true,
                required: true,
                min: 6,
                max: 24,
                validator: VFG_Custom.requireUsername
            },
            {
                type: 'vuetifyText',
                inputType: 'email',
                model: 'email',
                placeholder: 'Email',
                autocomplete: true,
                required: true,
                validator: VFG_Custom.requireEmail
            },
            {
                type: 'vuetifyText',
                inputType: 'password',
                model: 'password',
                placeholder: 'Password',
                min: 6,
                max: 24,
                required: true,
                validator: VFG_Custom.requireNewPassword
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
    success = false;
    submitting = false;

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

    onSubmit() {
        this.success = false;
        if (!this.isValid) return;
        this.model.errors = [];
        fetch('/api/Account/Register',
            {
                method: 'POST',
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Content-Type': `application/json;v=${this.$store.state.apiVer}`
                },
                body: JSON.stringify(this.model)
            })
            .then(response => response.json() as Promise<RegisterViewModel>)
            .then(data => {
                if (data.errors) {
                    this.model.errors = data.errors;
                } else {
                    this.success = true;
                }
            })
            .catch(error => {
                this.model.errors = ["A problem occurred. Your account acould not be registered."];
                ErrorMsg.logError("register.onSubmit", new Error(error));
            });
    }
}
