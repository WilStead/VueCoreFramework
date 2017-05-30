import { router, checkResponse, ApiResponseViewModel } from '../router';
import { store } from './store';
import { FieldDefinition } from './field-definition';
import { validators } from '../vfg-custom-validators';
import * as ErrorMsg from '../error-msg';

export interface DataItem {
    id: string;
    creationTimestamp: number;
    updateTimestamp: number;
}

export interface OperationReply<T extends DataItem> {
    data: T;
    error: string;
}

export interface PageData<T extends DataItem> {
    pageItems: Array<T>;
    totalItems: number;
}

export interface Repository {
    add(returnPath: string, vm: any): Promise<OperationReply<any>>;
    find(returnPath: string, id: string): Promise<OperationReply<any>>;
    getAll(returnPath: string): Promise<Array<any>>;
    getFieldDefinitions(returnPath: string): Promise<Array<FieldDefinition>>;
    getPage(returnPath: string, search: string, sortBy: string, descending: boolean, page: number, rowsPerPage: number): Promise<PageData<any>>;
    remove(returnPath: string, id: string): Promise<OperationReply<any>>;
    removeRange(returnPath: string, ids: Array<string>): Promise<OperationReply<any>>;
    update(returnPath: string, vm: any): Promise<OperationReply<any>>;
}

interface ApiNumericResponseViewModel {
    response: number;
}

export class DataRepository<T extends DataItem> implements Repository {
    dataType = '';

    constructor(dataType: string) { this.dataType = dataType; }

    add(returnPath: string, vm: T): Promise<OperationReply<T>> {
        return fetch(`/api/Data/${this.dataType}/Add`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                },
                body: JSON.stringify(vm)
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<any>)
            .then(data => {
                if (data.error) {
                    return {
                        data: vm,
                        error: data.error
                    };
                }
                return { data };
            })
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error.Message}`);
            });
    }

    find(returnPath: string, id: string): Promise<OperationReply<T>> {
        if (id === undefined || id === null || id === '')
        {
            return Promise.reject("The item id was missing from your request.");
        }
        return fetch(`/api/Data/${this.dataType}/Find/${id}`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<any>)
            .then(data => {
                if (data.error) {
                    return {
                        data: undefined,
                        error: data.error
                    };
                }
                return { data };
            })
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error.Message}`);
            });
    }

    getAll(returnPath: string): Promise<Array<T>> {
        return fetch(`/api/Data/${this.dataType}/GetAll`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<Array<T>>)
            .then(data => data)
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error.Message}`);
            });
    }

    getFieldDefinitions(returnPath: string): Promise<Array<FieldDefinition>> {
        return fetch(`/api/Data/${this.dataType}/GetFieldDefinitions`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<any>)
            .then(data => {
                if (data.error) {
                    return Promise.reject(`There was a problem with your request. ${data.error}`);
                } else {
                    let defs = data;
                    for (var i = 0; i < defs.length; i++) {
                        if (defs[i].validator && validators[defs[i].validator]) {
                            defs[i].validator = validators[defs[i].validator];
                        }
                    }
                }
            })
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error.Message}`);
            });
    }

    getPage(returnPath: string, search: string, sortBy: string, descending: boolean, page: number, rowsPerPage: number): Promise<PageData<T>> {
        var url = `/api/Data/${this.dataType}/GetPage`;
        if (search || sortBy || descending || page || rowsPerPage) {
            url += '?';
        }
        if (search) {
            url += `search=${encodeURIComponent(search)}`;
        }
        if (sortBy) {
            if (search) {
                url += '&';
            }
            url += `sortBy=${encodeURIComponent(sortBy)}`;
        }
        if (descending) {
            if (search || sortBy) {
                url += '&';
            }
            url += `descending=${descending}`;
        }
        if (page) {
            if (search || sortBy || descending) {
                url += '&';
            }
            url += `page=${page}`;
        }
        if (rowsPerPage) {
            if (search || sortBy || descending || page) {
                url += '&';
            }
            url += `rowsPerPage=${rowsPerPage}`;
        }
        return fetch(`/api/Data/${this.dataType}/GetTotal`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ApiNumericResponseViewModel>)
            .then(response => {
                return fetch(url,
                    {
                        method: 'GET',
                        headers: {
                            'Accept': 'application/json',
                            'Authorization': `bearer ${store.state.token}`
                        }
                    })
                    .then(response => checkResponse(response, returnPath))
                    .then(response => response.json() as Promise<Array<T>>)
                    .then(data => {
                        return {
                            pageItems: data,
                            totalItems: response.response
                        };
                    })
                    .catch(error => {
                        return Promise.reject(`There was a problem with your request. ${error.Message}`);
                    });
            })
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error.Message}`);
            });
    }

    remove(returnPath: string, id: string): Promise<OperationReply<T>> {
        if (id === undefined || id === null || id === '') {
            return Promise.reject("The item id was missing from your request.");
        }
        return fetch(`/api/Data/${this.dataType}/Remove/${id}`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.response) {
                    return {
                        data: undefined,
                        error: data.response
                    };
                }
                return { data: undefined, error: undefined };
            })
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error.Message}`);
            });
    }

    removeRange(returnPath: string, ids: Array<string>): Promise<OperationReply<T>> {
        if (ids === undefined || ids === null || !ids.length) {
            return Promise.reject("The item ids were missing from your request.");
        }
        return fetch(`/api/Data/${this.dataType}/Remove`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                },
                body: JSON.stringify(ids)
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.response) {
                    return {
                        data: undefined,
                        error: data.response
                    };
                }
                return { data: undefined, error: undefined };
            })
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error.Message}`);
            });
    }

    update(returnPath: string, vm: T): Promise<OperationReply<T>> {
        return fetch(`/api/Data/${this.dataType}/Update`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                },
                body: JSON.stringify(vm)
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<any>)
            .then(data => {
                if (data.error) {
                    return {
                        data: vm,
                        error: data.error
                    };
                }
                return { data };
            })
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error.Message}`);
            });
    }
}