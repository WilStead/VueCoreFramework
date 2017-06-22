import { router, checkResponse, ApiResponseViewModel } from '../router';
import { store } from './store';
import { FieldDefinition } from '../vfg/vfg';
import { validators } from '../vfg/vfg-custom-validators';
import * as ErrorMsg from '../error-msg';

/**
 * A ViewModel used to receive a numeric response from an API call.
 */
interface ApiNumericResponseViewModel {
    response: number;
}

/**
 * Represents a database object which can be displayed automatically by the SPA framework.
 */
export interface DataItem {
    /**
     * The unique ID (primary key) of the item.
     */
    id: string;

    /**
     * The date/time when the item was created.
     */
    creationTimestamp: number;

    /**
     * The date/time when the item was last updated.
     */
    updateTimestamp: number;
}

/**
 * A ViewModel used to receive a generic response from an API call.
 */
export interface OperationReply<T> {
    /**
     * The data received from the API.
     */
    data: T;

    /**
     * Any error message received from the API.
     */
    error: string;
}

/**
 * A ViewModel used to receive a page of items from an API call.
 */
export interface PageData<T> {
    /**
     * The array of items received from the API.
     */
    pageItems: Array<T>;

    /**
     * The total number of items in the type received from the API.
     */
    totalItems: number;
}

/**
 * Encapsulates data maniupulation calls to the API for a particular data type.
 */
export class Repository {
    dataType = '';

    fieldDefinitions: FieldDefinition[] = null;

    /**
     * Initializes a new instance of Repository.
     * @param {string} dataType The name of the data type managed by this Repository.
     */
    constructor(dataType: string) { this.dataType = dataType; }

    /**
     * Called to create a new instance of the data type and add it to the database.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} childProp An optional navigation property which will be set on the new object.
     * @param {string} parentId The primary key of the entity which will be set on the childProp property.
     * @returns {OperationReply<DataItem>} A response object containing any error which occurred, or the newly added item.
     */
    add(returnPath: string, childProp: string, parentId: string): Promise<OperationReply<DataItem>> {
        let url = `/api/Data/${this.dataType}/Add`;
        if (childProp && parentId) {
            url += `/${childProp}/${parentId}`;
        }
        return fetch(url,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<OperationReply<DataItem>>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to add an assortment of child entities to a parent entity under the given navigation
     * property.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} id The primary key of the parent entity.
     * @param {string} childProp The navigation property to which the children will be added.
     * @param {Array<string>} ids The primary keys of the child entities which will be added.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    addChildrenToCollection(returnPath: string, id: string, childProp: string, ids: Array<string>): Promise<ApiResponseViewModel> {
        return fetch(`/api/Data/${this.dataType}/AddChildrenToCollection/${id}/${childProp}`,
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
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to find an entity with the given primary key value, or an empty ViewModel (not null).
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} id The primary key of the entity to be found.
     * @returns {OperationReply<DataItem>} A response object containing any error which occurred, or the item.
     */
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
            .then(response => response.json() as Promise<OperationReply<DataItem>>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to retrieve ViewModels representing all the entities in the database of the
     * repository's type.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @returns {Array<DataItem>} All the items.
     */
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
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to retrieve all the primary keys of the entities in a given relationship.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} id The primary key of the parent entity.
     * @param {string} childProp The navigation property of the relationship on the parent entity.
     * @returns {Array<string>} The primary keys of all the children.
     */
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
            .then(response => response.json() as Promise<Array<string> | ApiResponseViewModel>)
            .then(data => {
                if (data['error']) {
                    throw new Error(`There was a problem with your request. ${data['error']}`);
                }
                return data;
            })
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to retrieve the primary key of a child entity in the given relationship.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} id The primary key of the parent entity.
     * @param {string} childProp The navigation property of the relationship on the parent entity.
     * @returns {ApiResponseViewModel} A response object containing the primary key of the child entity.
     */
    getChildId(returnPath: string, id: string, childProp: string): Promise<ApiResponseViewModel> {
        return fetch(`/api/Data/${this.dataType}/GetChildId/${id}/${childProp}`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to retrieve a page of child entities in a given relationship.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} id The primary key of the parent entity.
     * @param {string} childProp The navigation property of the relationship on the parent entity.
     * @param {string} search An optional search term which will filter the results. Any string or
     * numeric property with matching text will be included.
     * @param {string} sortBy An optional property name which will be used to sort the items before
     * calculating the page contents.
     * @param {boolean} descending Indicates whether the sort is descending; if false, the sort is ascending.
     * @param {number} page The page number requested.
     * @param {number} rowsPerPage The number of items per page.
     * @returns {PageData<DataItem>} The PageData for the page of children retrieved.
     */
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
                        throw new Error(`There was a problem with your request. ${error}`);
                    });
            })
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to retrieve a list of FieldDefinitions for the repository's data type.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @returns {Array<FieldDefinition>} The FieldDefinitions for the properties of the repository's data type.
     */
    getFieldDefinitions(returnPath: string): Promise<Array<FieldDefinition>> {
        if (this.fieldDefinitions === null) {
            return fetch(`/api/Data/${this.dataType}/GetFieldDefinitions`,
                {
                    method: 'GET',
                    headers: {
                        'Accept': 'application/json',
                        'Authorization': `bearer ${store.state.token}`
                    }
                })
                .then(response => checkResponse(response, returnPath))
                .then(response => response.json() as Promise<Array<FieldDefinition>>)
                .then(data => {
                    if (data['error']) {
                        throw new Error(`There was a problem with your request. ${data['error']}`);
                    } else {
                        let defs = data;
                        // Translate validator keys to default validator names or actual functions.
                        for (var i = 0; i < defs['length']; i++) {
                            if (defs[i].validator && validators[defs[i].validator as string]) {
                                defs[i].validator = validators[defs[i].validator as string];
                            }
                        }
                        this.fieldDefinitions = defs;
                        return this.fieldDefinitions;
                    }
                })
                .catch(error => {
                    throw new Error(`There was a problem with your request. ${error}`);
                });
        } else {
            return new Promise<Array<FieldDefinition>>((resolve, reject) => { resolve(this.fieldDefinitions); });
        }
    }

    /**
     * Called to retrieve the set of entities with the given paging parameters.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} search An optional search term which will filter the results. Any string or
     * numeric property with matching text will be included.
     * @param {string} sortBy An optional property name which will be used to sort the items before
     * calculating the page contents.
     * @param {boolean} descending Indicates whether the sort is descending; if false, the sort is ascending.
     * @param {number} page The page number requested.
     * @param {number} rowsPerPage The number of items per page.
     * @param {Array<string>} except The primary keys of items which should be excluded from the
     * results before caluclating the page contents.
     * @returns {PageData<DataItem>} The PageData for the page of items retrieved.
     */
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
                        throw new Error(`There was a problem with your request. ${error}`);
                    });
            })
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to remove an entity from the database.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} id The primary key of the entity to remove.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    remove(returnPath: string, id: string): Promise<ApiResponseViewModel> {
        if (id === undefined || id === null || id === '') {
            throw new Error("The item id was missing from your request.");
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
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to remove an assortment of child entities from a parent entity under the given
     * navigation property.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} id The primary key of the parent entity.
     * @param {string} childProp The navigation property from which the children will be removed.
     * @param {Array<string>} childIds The primary keys of the child entities which will be removed.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    removeChildrenFromCollection(returnPath: string, id: string, childProp: string, childIds: Array<string>): Promise<ApiResponseViewModel> {
        return fetch(`/api/Data/${this.dataType}/RemoveChildrenFromCollection/${id}/${childProp}`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                },
                body: JSON.stringify(childIds)
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to terminate a relationship bewteen two entities. If the child entity is made an
     * orphan by the removal and is not a MenuClass object, it is then removed from the database
     * entirely.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} id The primary key of the child entity whose relationship is being severed.
     * @param {string} childProp The navigation property of the relationship being severed.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    removeFromParent(returnPath: string, id: string, childProp: string): Promise<ApiResponseViewModel> {
        return fetch(`/api/Data/${this.dataType}/RemoveFromParent/${id}/${childProp}`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to remove a collection of entities from the database.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {Array<string>} ids The primary keys of the entities to remove.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    removeRange(returnPath: string, ids: Array<string>): Promise<ApiResponseViewModel> {
        if (ids === undefined || ids === null || !ids.length) {
            throw new Error("The item ids were missing from your request.");
        }
        return fetch(`/api/Data/${this.dataType}/RemoveRange`,
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
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to terminate a relationship for multiple entities. If any child entity is made an
     * orphan by the removal and is not a MenuClass object, it is then removed from the database
     * entirely.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} childProp The navigation property of the relationship being severed.
     * @param {Array<string>} ids The primary keys of child entities whose relationships are being severed.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    removeRangeFromParent(returnPath: string, childProp: string, ids: Array<string>): Promise<ApiResponseViewModel> {
        return fetch(`/api/Data/${this.dataType}/RemoveRangeFromParent/${childProp}`,
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
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to create a relationship between two entities, replacing another entity which was
     * previously in that relationship with another one. If the replaced entity is made an orphan
     * by the removal and is not a MenuClass object, it is then removed from the database entirely.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} parentId The primary key of the parent entity in the relationship.
     * @param {string} newChildId The primary key of the new child entity entering into the relationship.
     * @param {string} childProp The navigation property of the relationship on the child entity.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    replaceChild(returnPath: string, parentId: string, newChildId: string, childProp: string): Promise<ApiResponseViewModel> {
        return fetch(`/api/Data/${this.dataType}/ReplaceChild/${parentId}/${newChildId}/${childProp}`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to create a relationship between two entities, replacing another entity which was
     * previously in that relationship with a new entity. If the replaced entity is made an orphan
     * by the removal and is not a MenuClass object, it is then removed from the database entirely.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} parentId The primary key of the parent entity in the relationship.
     * @param {string} childProp The navigation property of the relationship on the child entity.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    replaceChildWithNew(returnPath: string, parentId: string, childProp: string): Promise<OperationReply<DataItem>> {
        return fetch(`/api/Data/${this.dataType}/ReplaceChildWithNew/${parentId}/${childProp}`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${store.state.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<OperationReply<DataItem>>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }

    /**
     * Called to update an entity in the database.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {DataItem} vm The item to update.
     * @returns {OperationReply<DataItem>} A response object containing any error which occurred, or the updated item.
     */
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
            .then(response => response.json() as Promise<OperationReply<DataItem>>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }
}