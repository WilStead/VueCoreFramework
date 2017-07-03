/**
 * Describes button definitions used by vue-form-generator.
 */
interface ButtonDefinition {
    /**
     * A string containing CSS classes to be applied to the button.
     */
    classes: string;

    /**
     * The label for the button.
     */
    label: string;

    /**
     * A function to be used as the button's onclick event handler. Accepts model, field, and event parameters.
     */
    onclick: Function;
}

/**
 * Describes field definitions used by vue-form-generator.
 */
export interface FieldDefinition {
    /**
     * Indicates that this text field can be auto-completed.
     */
    autocomplete?: boolean;

    /**
     * An array of buttons for the field.
     */
    buttons?: Array<any>

    /**
     * An optional default value for the field.
     */
    default?: any;

    /**
     * Indicates whether the field is disabled.
     */
    disabled?: boolean | Function;

    /**
     * Optionally designates a group to which the field belongs.
     */
    groupName?: string;

    /**
     * Optional help text displayed with the field.
     */
    help?: string;

    /**
     * Indicates that the field is hidden in data tables (but may still be visible in forms).
     */
    hideInTable?: boolean | Function;

    /**
     * Optional hint text displayed with the field.
     */
    hint?: string;

    /**
     * The name of a Material Icon displayed with certain fields.
     */
    icon?: string;

    /**
     * The input type of certain fields which can display more than one type of data.
     * For navigation properties, the type name.
     */
    inputType?: string;

    /**
     * For navigation properties, indicates the name of the inverse navigation property.
     */
    inverseType?: string;

    /**
     * Indicates that the field is a Name property.
     */
    isName?: boolean;

    /**
     * Optional label text displayed with the field.
     */
    label?: string;

    /**
     * An optional maximum value for the field.
     */
    max?: number;

    /**
     * An optional minimum value for the field.
     */
    min?: number;

    /**
     * The name of the model property represented by the field.
     */
    model: string;

    /**
     * For navigation properties, indicates the type of navigation property.
     */
    navigationType?: string;

    /**
     * For navigation properties, indicates the name of the parent data type.
     */
    parentType?: string;

    /**
     * An optional regex pattern used to validate the field.
     */
    pattern?: string | RegExp;

    /**
     * Optional placeholder text displayed with the field.
     */
    placeholder?: string;

    /**
     * Optional text displayed before a text field.
     */
    prefix?: string;

    /**
     * Indicates that the text field is read-only.
     */
    readonly?: boolean | Function;

    /**
     * Indicates that the field is required.
     */
    required?: boolean;

    /**
     * An optional value indicating the number of rows in a textarea field.
     */
    rows?: number;

    /**
     * An optional step value for numeric text fields, and TimeSpan fields.
     */
    step?: number;

    /**
     * A string or Array of strings containing CSS classes to decorate the field.
     */
    styleClasses?: string | Array<string>;

    /**
     * Optional text displayed after a text field.
     */
    suffix?: string;

    /**
     * Indicates the type of field. Required.
     */
    type: string;

    /**
     * An optional name of a validator used to validate the field, or a validator function.
     */
    validator?: string | string[] | Function | Array<Function>;

    /**
     * The Array of options for a select or multiselect field, or a function which returns such an Array.
     */
    values?: Array<any> | Function;

    /**
     * Indicates whether the field is visible on data tables and forms, or a function that returns such an indicator.
     */
    visible?: boolean | Function;
}

/**
 * Describes a group of fields used in a vue-form-generator schema.
 */
export interface SchemaGroup {
    /**
     * The title of the group.
     */
    legend: string;

    /**
     * The Array of FieldDefinitions contained in the group.
     */
    fields: Array<FieldDefinition>;
}

/**
 * Describes a vue-form-generator schema.
 */
export interface Schema {
    /**
     * An optional Array of SchemaGroups contained in the schema.
     */
    groups?: Array<SchemaGroup>;

    /**
     * The Array of FieldDefinitions contained in the schema. Optional if any groups are present.
     */
    fields?: Array<FieldDefinition>;
}

/**
 * Describes the set of options used to configure vue-form-generator in a Component.
 */
export interface VFGOptions {
    /**
     * Execute validation after model loaded.
     */
    validateAfterLoad?: boolean;

    /**
     * Execute validation after every change.
     */
    validateAfterChanged?: boolean;

    /**
     * Prefix to add to every field's id. Will be prepended to ids explicity set in the schema, as
     * well as auto-generated ones.
     */
    fieldIdPrefix?: string;
}