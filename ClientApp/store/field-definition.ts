export interface DateTimePickerOptions {
    format: string;
}

export interface FieldDefinition {
    autocomplete?: boolean;
    buttons?: Array<any>
    dateTimePickerOptions?: DateTimePickerOptions;
    default?: any;
    disabled?: boolean | Function;
    groupName?: string;
    help?: string;
    hideInTable?: boolean | Function;
    hint?: string;
    inputType?: string;
    label?: string;
    max?: number;
    min?: number;
    model: string;
    pattern?: string;
    placeholder?: string;
    readonly?: boolean | Function;
    required?: boolean;
    rows?: number;
    step?: number;
    styleClasses?: string | Array<string>;
    type: string;
    validator?: Function | Array<Function>;
    values?: Array<any> | Function;
    visible?: boolean | Function;
}