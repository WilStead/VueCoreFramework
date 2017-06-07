export interface FieldDefinition {
    autocomplete?: boolean;
    buttons?: Array<any>
    default?: any;
    disabled?: boolean | Function;
    groupName?: string;
    help?: string;
    hideInTable?: boolean | Function;
    hint?: string;
    icon?: string;
    inputType?: string;
    label?: string;
    max?: number;
    min?: number;
    model: string;
    pattern?: string;
    placeholder?: string;
    prefix?: string;
    readonly?: boolean | Function;
    required?: boolean;
    rows?: number;
    step?: number;
    styleClasses?: string | Array<string>;
    suffix?: string;
    type: string;
    validator?: Function | Array<Function>;
    values?: Array<any> | Function;
    visible?: boolean | Function;
}