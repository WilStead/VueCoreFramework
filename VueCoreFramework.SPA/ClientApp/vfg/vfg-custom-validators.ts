import * as moment from 'moment';

const resources = {
    durationTooShort: "Must be at least {0}",
    durationTooLong: "Cannot be more than {0}",
    fieldIsRequired: "This field is required",
    invalidFormat: "Invalid format",
    textTooSmall: "Must be at least {1} characters long",
    textTooBig: "Cannot be longer than {1} characters",
    thisNotText: "Invalid entry"
}

function msg(text: string, ...args: any[]) {
    if (text != null && args.length)
        for (let i = 0; i < args.length; i++)
            text = text.replace("{" + i + "}", args[i]);

    return text;
}

function checkEmpty(value, required, messages = resources) {
    if (value === undefined || value === null || value === "") {
        if (required)
            return [msg(messages.fieldIsRequired)];
        else
            return [];
    }
    return null;
}

interface VFG_Validator { (value, field, model, messages): any; locale: Function; }

/**
 * A vue-form-generator validator for TimeSpan fields.
 */
export const timespan = <VFG_Validator>function (value, field, model, messages = resources) {
    let res = checkEmpty(value, field.required, messages); if (res != null) return res;

    let err = [];
    if (typeof value === 'string') {
        let d = moment.duration(value);
        if (d.toISOString() === 'P0D' && value !== '00:00:00') {
            return [msg(messages.invalidFormat)];
        }
        let dMin = moment.duration(field.min);
        if ((dMin.toISOString() !== 'P0D' || field.min === '00:00:00')
            && moment.duration(d.asMilliseconds()).subtract(dMin).asMilliseconds() < 0) {
            err.push(msg(messages.durationTooShort, dMin.humanize()));
        }
        let dMax = moment.duration(field.max);
        if ((dMax.toISOString() !== 'P0D' || field.max === '00:00:00')
            && moment.duration(dMax.asMilliseconds()).subtract(d).asMilliseconds() < 0) {
            err.push(msg(messages.durationTooLong, dMax.humanize()));
        }
    } else if (value !== undefined) {
        err.push(msg(messages.invalidFormat));
    }

    return err;
}
timespan.locale = customMessages => (value, field, model) => timespan(value, field, model, Object.assign({}, resources, customMessages));

/**
 * A vue-form-generator validator for text fields with a regex pattern.
 */
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
            if (!re.test(value)) {
                err.push(msg(messages.invalidFormat));
            }
        }

    } else {
        err.push(msg(messages.thisNotText));
    }

    return err;
}
string_regexp.locale = customMessages => (value, field, model) => string_regexp(value, field, model, Object.assign({}, resources, customMessages));

/**
 * A vue-form-generator validator for text fields which represent a required email address.
 */
export const requireEmail = <VFG_Validator>function (value, field, model, messages = resources) {
    if (value === undefined || value === null || value === "") {
        return ["A valid email address is required"];
    }
    let re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
    if (!re.test(value))
        return ["A valid email address is required"];
}

/**
 * A vue-form-generator validator for text fields which represent a 'confirm password' field.
 * Requires a field called 'newPassword' in the model to match against.
 */
export const requirePasswordMatch = <VFG_Validator>function (value, field, model, messages = resources) {
    if (value === undefined || value === null || value === "") {
        return ["You must confirm your new password"];
    }
    if (value !== model.newPassword) {
        return ["Your passwords must match"];
    }
}

/**
 * A vue-form-generator validator for text fields which represent a required password, with the
 * following requirements: a lower-case and upper-case letter, a number, and a special character.
 */
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

/**
 * A vue-form-generator validator for text fields which represent a required email address.
 */
export const requireUsername = <VFG_Validator>function (value, field, model, messages = resources) {
    if (value === undefined || value === null || value === "") {
        return ["A username is required"];
    }
    let re = /^[\w.@-]+$/;
    if (!re.test(value))
        return ["Usernames can contain only letters, numbers, underscores, hyphens, and periods"];
}

/**
 * An object which maps keys to validator objects.
 */
export const validators = {
    'string': 'string',
    'number': 'number',
    'string_regexp': string_regexp,
    'date': 'date',
    'array': 'array',
    'timespan': timespan
};