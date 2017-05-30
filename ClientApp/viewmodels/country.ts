import { DataRepository } from '../store/repository';

export interface Country {
    id: string;
    creationTimestamp: number;
    updateTimestamp: number;
    name: string;
    epiIndex: number;
}

export const countryData = new DataRepository<Country>('Country');