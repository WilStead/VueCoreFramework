import { checkResponse, ApiResponseViewModel } from '../router';
import * as ErrorMsg from '../error-msg';

export interface DataItem {
    id: string;
    creationTimestamp: number;
    updateTimestamp: number;
}

export interface OperationReply<T extends DataItem> {
    data: T;
    errors: Array<string>;
}

export interface PageData<T extends DataItem> {
    pageItems: Array<T>;
    totalItems: number;
}

export class Repository<T extends DataItem> {
    private data: Array<T> = [];

    dataType = '';

    constructor(initial: Array<T>) { this.data = initial.slice(); }

    add(vm: T): Promise<OperationReply<T>> {
        return fetch(`/api/Data/${this.dataType}/Add`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Content-Type': 'application/json',
                    'Authorization': `bearer ${this.$store.state.token}`
                },
                body: JSON.stringify(vm)
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<T>)
            .then(data => {
                if (data.error) {
                    return {
                        data: vm,
                        errors: [ data.error ]
                    };
                }
                return { data };
            })
            .catch(error => {
                return Promise.reject(`There was a problem with your request. ${error.Message}`);
            });
    }

    find(id: string): Promise<T> {
        return new Promise<T>((resolve, reject) => {
            resolve(this.data.find(d => d.id == id));
        });
    }

    getAll(): Promise<Array<T>> {
        return new Promise<Array<T>>((resolve, reject) => {
            resolve(this.data);
        });
    }

    getPage(search, sortBy, descending, page, rowsPerPage): Promise<PageData<T>> {
        return new Promise<PageData<T>>((resolve, reject) => {
            let pageItems = this.data.slice();
            pageItems = pageItems.filter(v => {
                for (var prop in v) {
                    if (typeof v[prop] === 'string') {
                        let s: string = <any>v[prop];
                        if (s.includes(search)) return true;
                    } else if (typeof v[prop] === 'number') {
                        let n: number = <any>v[prop];
                        if (n.toString().includes(search)) return true;
                    }
                }
                return false;
            });

            if (sortBy) {
                pageItems.sort((a, b) => {
                    const sortA = a[sortBy];
                    const sortB = b[sortBy];

                    if (descending) {
                        if (sortA < sortB) return 1;
                        if (sortA > sortB) return -1;
                        return 0;
                    } else {
                        if (sortA < sortB) return -1;
                        if (sortA > sortB) return 1;
                        return 0;
                    }
                });
            }

            if (rowsPerPage > 0) {
                pageItems = pageItems.slice((page - 1) * rowsPerPage, page * rowsPerPage);
            }
            resolve({ pageItems, totalItems: this.data.length });
        });
    }

    remove(id: string): Promise<void> {
        return new Promise<void>((resolve, reject) => {
            this.data.splice(this.data.findIndex(d => d.id == id), 1);
            resolve();
        });
    }

    removeRange(ids: Array<string>): Promise<void> {
        return new Promise<void>((resolve, reject) => {
            for (var i = 0; i < ids.length; i++) {
                this.data.splice(this.data.findIndex(d => d.id == ids[i]), 1);
            }
            resolve();
        });
    }

    update(vm: T): Promise<OperationReply<T>> {
        return new Promise<OperationReply<T>>((resolve, reject) => {
            vm.updateTimestamp = Date.now();
            let oldIndex = this.data.findIndex(d => d.id == vm.id);
            if (oldIndex == -1) reject();
            this.data.splice(oldIndex, 1, vm);
            let reply = {
                data: this.data[oldIndex],
                errors: []
            };
            resolve(reply);
        });
    }
}