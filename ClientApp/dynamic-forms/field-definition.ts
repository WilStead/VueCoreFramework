export interface ItemOptions {
    value: string;
    name: string;
}

export interface FieldDefinition {
    model: string;
    type: string;
    inputType?: string;
    label?: string;
    isId?: boolean;
    default?: any;
    placeholder?: string;
    visible?: boolean | Function;
    disabled?: boolean | Function;
    readonly?: boolean | Function;
    required?: boolean;
    autocomplete?: boolean;
    hint?: string;
    help?: string;
    styleClasses?: string | Array<string>;
    values?: Array<any> | Function;
    checklistOptions?: ItemOptions;
    radiosOptions?: ItemOptions;
    checked?: boolean;
    rows?: number;
    buttonText?: string;
    onSubmit?: Function;
    validateBeforeSubmit?: boolean;
    min?: number;
    max?: number;
    pattern?: string;
    validator?: Function | Array<Function>;
}