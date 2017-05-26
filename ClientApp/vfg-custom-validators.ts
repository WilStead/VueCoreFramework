import VueFormGenerator from 'vue-form-generator';

const resources = {
    fieldIsRequired: "This field is required",
    invalidFormat: "Invalid format",
    textTooSmall: "Must be at least {1} characters long",
    textTooBig: "Cannot be longer than {1} characters",
    thisNotText: "Invalid entry"
}

function msg(text: string, ...args: any[]) {
    if (text != null && args.length)
        for (let i = 0; i < args.length; i++)
            text = text.replace("{" + (i - 1) + "}", arguments[i]);

    return text;
}

export function checkEmpty(value, required, messages = resources) {
    if (value === undefined || value === null || value === "") {
        if (required)
            return [msg(messages.fieldIsRequired)];
        else
            return [];
    }
    return null;
}

interface VFG_Validator { (value, field, model, messages): any; locale: any; }

export const string_regexp = <VFG_Validator>function (value, field, model, messages = resources) {
    let res = checkEmpty(value, field.required, messages); if (res != null) return res;

    let err = [];
    if (typeof value === 'string') {
        if (field.min && value.length < field.min)
            err.push(msg(messages.textTooSmall, value.length, field.min));

        if (field.max && value.length > field.max)
            err.push(msg(messages.textTooBig, value.length, field.max));

        if (field.pattern) {
            let re = new RegExp(field.pattern);
            if (!re.test(value))
                return [msg(messages.invalidFormat)];
        }

    } else
        err.push(msg(messages.thisNotText));

    return err;
}
string_regexp.locale = customMessages => (value, field, model) => string_regexp(value, field, model, Object.assign({}, resources, customMessages));

export const requireEmail = <VFG_Validator>function (value, field, model, messages = resources) {
    if (value === undefined || value === null || value === "") {
        return ["A valid email address is required"];
    }
    let re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    if (!re.test(value))
        return ["A valid email address is required"];
}

export const requirePasswordMatch = <VFG_Validator>function (value, field, model, messages = resources) {
    if (value === undefined || value === null || value === "") {
        return ["You must confirm your new password"];
    }
    if (value !== model.newPassword) {
        return ["Your passwords must match"];
    }
}

export const requireNewPassword = <VFG_Validator>function (value, field, model, messages = resources) {
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

export const validators = {
    'string': VueFormGenerator.validators.string,
    'number': VueFormGenerator.validators.number,
    'string_regexp': string_regexp
};