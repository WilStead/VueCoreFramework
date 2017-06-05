import { router, checkResponse, ApiResponseViewModel } from '../router';
import { store } from './store';
import { FieldDefinition } from './field-definition';
import { validators } from '../vfg/vfg-custom-validators';
import * as ErrorMsg from '../error-msg';

interface ApiNumericResponseViewModel {
    response: number;
}

export interface DataItem {
    id: string;
    creationTimestamp: number;
    updateTimestamp: number;
}

export interface OperationReply<DataItem> {
    data: DataItem;
    error: string;
}

export interface PageData<DataItem> {
    pageItems: Array<DataItem>;
    totalItems: number;
}

export class Repository {
    dataType = '';

    constructor(dataType: string) { this.dataType = dataType; }

    add(returnPath: string, vm: DataItem): Promise<OperationReply<DataItem>> {
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
                return Promise.reject(`There was a problem with your request. ${error}`);
            });
    }

    addToParentCollection(returnPath: string, id: string, childProp: string, ids: Array<string>): Promise<OperationReply<DataItem>> {
        return fetch(`/api/Data/${this.dataType}/AddToParentCollection/${id}/${childProp}`,
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
                return Promise.reject(`There was a problem with your request. ${error}`);
            });
    }

    find(returnPath: string, id: string): Promise<OperationReply<DataItem>> {
        if (id === undefined || id === null || id === '') {
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
                return Promise.reject(`There was a problem with your request. ${error}`);
            });
    }

    getAll(returnPath: string): Promise<Array<DataItem>> {
        return fetch(`/api/Data/${this.dataType}/GetAll`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<Array<DataItem>>)
            .then(data => data)
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error}`);
            });
    }

    getAllChildIds(returnPath: string, id: string, childProp: string): Promise<Array<string>> {
        return fetch(`/api/Data/${this.dataType}/GetAllChildIds/${id}/${childProp}`,
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
                }
                return data;
            })
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error}`);
            });
    }

    getChildPage(returnPath: string, id: string, childProp: string, search: string, sortBy: string, descending: boolean, page: number, rowsPerPage: number): Promise<PageData<DataItem>> {
        var url = `/api/Data/${this.dataType}/GetChildPage/${id}/${childProp}`;
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
        return fetch(`/api/Data/${this.dataType}/GetChildTotal/${id}/${childProp}`,
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
                    .then(response => response.json() as Promise<Array<DataItem>>)
                    .then(data => {
                        return {
                            pageItems: data,
                            totalItems: response.response
                        };
                    })
                    .catch(error => {
                        return Promise.reject(`There was a problem with your request. ${error}`);
                    });
            })
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error}`);
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
                    return defs;
                }
            })
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error}`);
            });
    }

    getPage(returnPath: string, search: string, sortBy: string, descending: boolean, page: number, rowsPerPage: number, except: Array<string> = []): Promise<PageData<DataItem>> {
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
                        method: 'POST',
                        headers: {
                            'Accept': 'application/json',
                            'Content-Type': 'application/json',
                            'Authorization': `bearer ${store.state.token}`
                        },
                        body: JSON.stringify(except)
                    })
                    .then(response => checkResponse(response, returnPath))
                    .then(response => response.json() as Promise<Array<DataItem>>)
                    .then(data => {
                        return {
                            pageItems: data,
                            totalItems: response.response - (except ? except.length : 0)
                        };
                    })
                    .catch(error => {
                        return Promise.reject(`There was a problem with your request. ${error}`);
                    });
            })
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error}`);
            });
    }

    remove(returnPath: string, id: string): Promise<OperationReply<DataItem>> {
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
                return Promise.reject(`There was a problem with your request. ${error}`);
            });
    }

    removeFromParentCollection(returnPath: string, id: string, childProp: string, ids: Array<string>): Promise<OperationReply<DataItem>> {
        return fetch(`/api/Data/${this.dataType}/RemoveFromParentCollection/${id}/${childProp}`,
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
                return Promise.reject(`There was a problem with your request. ${error}`);
            });
    }

    removeRange(returnPath: string, ids: Array<string>): Promise<OperationReply<DataItem>> {
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
                return Promise.reject(`There was a problem with your request. ${error}`);
            });
    }

    update(returnPath: string, vm: DataItem): Promise<OperationReply<DataItem>> {
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
                return Promise.reject(`There was a problem with your request. ${error}`);
            });
    }
}