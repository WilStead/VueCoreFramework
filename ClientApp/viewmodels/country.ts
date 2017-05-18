import { Repository } from '../store/repository';
import { FieldDefinition } from '../dynamic-forms/field-definition';

export interface Country {
    id: string;
    creationTimestamp: number;
    name: string;
    epiIndex: number;
}

export const countryFieldDefinitions: Array<FieldDefinition> = [
    {
        model: 'id',
        type: 'input',
        inputType: 'text',
        isId: true,
        readonly: true,
        visible: false
    },
    {
        model: 'name',
        label: 'Name',
        type: 'string',
        required: true,
        min: 3,
        max: 25
    },
    {
        model: 'epiIndex',
        label: 'EPI Index',
        type: 'number',
        required: true,
        min: 0,
        max: 100
    }
];

export class CountryData extends Repository<Country> {
    constructor() {
        super([
            { id: "1", creationTimestamp: Date.now(), name: "Switzerland", epiIndex: 87.67 },
            { id: "2", creationTimestamp: Date.now(), name: "Luxembourg", epiIndex: 83.29 },
            { id: "3", creationTimestamp: Date.now(), name: "Australia", epiIndex: 82.4 },
            { id: "4", creationTimestamp: Date.now(), name: "Singapore", epiIndex: 81.78 },
            { id: "5", creationTimestamp: Date.now(), name: "Czech Republic", epiIndex: 81.47 },
            { id: "6", creationTimestamp: Date.now(), name: "Germany", epiIndex: 80.47 },
            { id: "7", creationTimestamp: Date.now(), name: "Spain", epiIndex: 79.09 },
            { id: "8", creationTimestamp: Date.now(), name: "Austria", epiIndex: 78.32 },
            { id: "9", creationTimestamp: Date.now(), name: "Sweden", epiIndex: 78.09 },
            { id: "10", creationTimestamp: Date.now(), name: "Norway", epiIndex: 78.04 }
        ]);
    }
}

export const countryData = new CountryData();