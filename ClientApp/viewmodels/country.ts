import { Repository } from '../store/repository';
import VueFormGenerator from 'vue-form-generator';
import { FieldDefinition } from '../dynamic-data/field-definition';

export interface Country {
    id: string;
    creationTimestamp: number;
    updateTimestamp: number;
    name: string;
    epiIndex: number;
}

export const countryFieldDefinitions: Array<FieldDefinition> = [
    {
        model: 'id',
        type: 'input',
        inputType: 'text',
        readonly: true,
        visible: false
    },
    {
        model: 'name',
        placeholder: 'Name',
        type: 'input',
        inputType: 'text',
        required: true,
        min: 3,
        max: 25,
        validator: VueFormGenerator.validators.string
    },
    {
        model: 'epiIndex',
        label: 'EPI Index',
        type: 'input',
        inputType: 'number',
        required: true,
        min: 0,
        max: 100,
        validator: VueFormGenerator.validators.number
    }
];

export class CountryData extends Repository<Country> {
    constructor() {
        super([
            { id: "1", creationTimestamp: Date.now(), updateTimestamp: 0, name: "Switzerland", epiIndex: 87.67 },
            { id: "2", creationTimestamp: Date.now(), updateTimestamp: 0, name: "Luxembourg", epiIndex: 83.29 },
            { id: "3", creationTimestamp: Date.now(), updateTimestamp: 0, name: "Australia", epiIndex: 82.4 },
            { id: "4", creationTimestamp: Date.now(), updateTimestamp: 0, name: "Singapore", epiIndex: 81.78 },
            { id: "5", creationTimestamp: Date.now(), updateTimestamp: 0, name: "Czech Republic", epiIndex: 81.47 },
            { id: "6", creationTimestamp: Date.now(), updateTimestamp: 0, name: "Germany", epiIndex: 80.47 },
            { id: "7", creationTimestamp: Date.now(), updateTimestamp: 0, name: "Spain", epiIndex: 79.09 },
            { id: "8", creationTimestamp: Date.now(), updateTimestamp: 0, name: "Austria", epiIndex: 78.32 },
            { id: "9", creationTimestamp: Date.now(), updateTimestamp: 0, name: "Sweden", epiIndex: 78.09 },
            { id: "10", creationTimestamp: Date.now(), updateTimestamp: 0, name: "Norway", epiIndex: 78.04 }
        ]);
    }
}

export const countryData = new CountryData();