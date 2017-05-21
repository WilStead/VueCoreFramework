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