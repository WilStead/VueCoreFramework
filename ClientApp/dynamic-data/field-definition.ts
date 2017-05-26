export interface DateTimePickerOptions {
    format: string;
}

export interface FieldDefinition {
    model: string;
    type: string;
    inputType?: string;
    label?: string;
    default?: any;
    placeholder?: string;
    visible?: boolean | Function;
    hideInTable?: boolean | Function;
    disabled?: boolean | Function;
    readonly?: boolean | Function;
    required?: boolean;
    autocomplete?: boolean;
    hint?: string;
    help?: string;
    styleClasses?: string | Array<string>;
    step?: number;
    values?: Array<any> | Function;
    dateTimePickerOptions?: DateTimePickerOptions;
    rows?: number;
    min?: number;
    max?: number;
    pattern?: string;
    validator?: Function | Array<Function>;
}