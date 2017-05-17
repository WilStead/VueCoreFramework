import { Repository } from '../store/repository';

export interface Country {
    id: string;
    name: string;
    epiIndex: number;
}

export class CountryData extends Repository<Country> {
    constructor() {
        super([
            { id: "1", name: "Switzerland", epiIndex: 87.67 },
            { id: "2", name: "Luxembourg", epiIndex: 83.29 },
            { id: "3", name: "Australia", epiIndex: 82.4 },
            { id: "4", name: "Singapore", epiIndex: 81.78 },
            { id: "5", name: "Czech Republic", epiIndex: 81.47 },
            { id: "6", name: "Germany", epiIndex: 80.47 },
            { id: "7", name: "Spain", epiIndex: 79.09 },
            { id: "8", name: "Austria", epiIndex: 78.32 },
            { id: "9", name: "Sweden", epiIndex: 78.09 },
            { id: "10", name: "Norway", epiIndex: 78.04 }
        ]);
    }
}