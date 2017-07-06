import Vue from 'vue';
import VueRouter from 'vue-router';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as ErrorMsg from '../../error-msg';
import { DataItem, PageData, Repository } from '../../store/repository';
import { permissionIncludesTarget, permissions } from '../../store/userStore';

/**
 * Describes the header of a Vuetify data table.
 */
interface TableHeader {
    /**
     * The text displayed in the header.
     */
    text: string;

    /**
     * The name of the model property used to get the value for the column.
     */
    value: string;

    /**
     * Optionally indicates that this column is left-aligned (right-align is the default).
     */
    align?: string;

    /**
     * Optionally indicates that this column is sortable (false by default).
     */
    sortable?: boolean;

    /**
     * Indicates that this field is a cultural object (false by default).
     */
    cultural?: boolean;
}

/**
 * A set of pagination options used by the Vuetify data table.
 */
export interface Pagination {
    /**
     * An optional property name which will be used to sort the items before calculating the page contents.
     */
    sortBy?: string;

    /**
     * Optionally indicates that the sort is descending (ascending by default).
     */
    descending?: boolean;

    /**
     * The page number requested.
     */
    page?: number;

    /**
     * The number of items per page.
     */
    rowsPerPage?: number;
}

@Component
export default class DynamicDataTable extends Vue {
    @Prop()
    allowEdit: boolean;

    @Prop()
    childProp: string;

    @Prop()
    dataType: string;

    @Prop()
    parentId: string;

    @Prop()
    parentProp: string;

    @Prop()
    parentType: string;

    @Prop()
    search: string;

    @Prop()
    tableType: string;

    canAdd = false;
    canDelete = false;
    deleteDialogShown = false;
    deleteAskingItems = [];
    deletePendingItems = [];
    deletePermissions: any = {};
    headers: Array<TableHeader> = [];
    items: Array<DataItem> = [];
    loading = true;
    internalPagination: Pagination = {};
    internalSearch = this.search || '';
    pagination: Pagination;
    paginationInitialization = 0;
    parentRepository: Repository = null;
    repository: Repository = null;
    routePagination: Pagination = null;
    searchDebounce = 0;
    selected: Array<DataItem> = [];
    totalItems = 0;
    updateTimeout = 0;

    @Watch('dataType')
    onDataTypeChange(val: string) {
        if (val) {
            this.repository = this.$store.getters.getRepository(this.dataType);
        }
    }

    @Watch('internalSearch')
    onSearchChange(val: string, oldVal: string) {
        clearTimeout(this.searchDebounce);
        this.searchDebounce = setTimeout(this.onSearch, 500);
    }

    @Watch('pagination', { deep: true })
    onPaginationChange(val: Pagination) {
        if (this.paginationInitialization < 2) {
            this.paginationInitialization++;
            if (this.pagination) {
                this.internalPagination.sortBy = this.pagination.sortBy;
                this.internalPagination.descending = this.pagination.descending;
                this.internalPagination.page = this.pagination.page;
                this.internalPagination.rowsPerPage = this.pagination.rowsPerPage;
            }
        }
    }

    @Watch('internalPagination', { deep: true })
    onInternalPaginationChange(val: Pagination) {
        if (this.paginationInitialization >= 2 &&
            (!this.pagination ||
            (val.sortBy !== this.pagination.sortBy
                || val.descending !== this.pagination.descending
                || val.page !== this.pagination.page
                || val.rowsPerPage !== this.pagination.rowsPerPage))) {
            this.$emit("onPagination", this.internalSearch, val);
        } else {
            this.paginationInitialization++;
        }
    }

    @Watch('parentType')
    onParentTypeChange(val: string) {
        if (val) {
            this.parentRepository = this.$store.getters.getRepository(this.parentType);
        }
    }

    @Watch('selected')
    onSelectedChanged(val: Array<DataItem>) {
        this.$emit('update:selected', val);
    }

    @Watch('$route')
    onRouteChange(val: VueRouter.Route) {
        this.getRouteData();
        if (this.pagination
            && (this.pagination.sortBy
                || this.pagination.descending
                || this.pagination.page
                || this.pagination.rowsPerPage)) {
            this.internalPagination.sortBy = this.pagination.sortBy;
            this.internalPagination.descending = this.pagination.descending;
            this.internalPagination.page = this.pagination.page;
            this.internalPagination.rowsPerPage = this.pagination.rowsPerPage;
        }
        this.refresh();
    }

    mounted() {
        this.getRouteData();
        if (this.pagination
            && (this.pagination.sortBy
                || this.pagination.descending
                || this.pagination.page
                || this.pagination.rowsPerPage)) {
            this.paginationInitialization--;
            this.internalPagination.sortBy = this.pagination.sortBy;
            this.internalPagination.descending = this.pagination.descending;
            this.internalPagination.page = this.pagination.page;
            this.internalPagination.rowsPerPage = this.pagination.rowsPerPage;
        }
        if (this.dataType) {
            this.repository = this.$store.getters.getRepository(this.dataType);
        }
        if (this.parentType) {
            this.parentRepository = this.$store.getters.getRepository(this.parentType);
        }
        this.refresh();
    }

    cancelDelete(id: string) {
        let index = this.deleteAskingItems.indexOf(id);
        if (index !== -1) {
            this.deleteAskingItems.splice(index, 1);
        }
    }

    getCulturalValue(value: string): string {
        let cValue = JSON.parse(value);
        let v = cValue[this.$store.state.userState.culture];
        if (!v) {
            let def = cValue.default;
            if (def) {
                v = cValue[def];
            }
        }
        return v;
    }

    getData(): Promise<PageData<DataItem>> {
        this.loading = true;
        return new Promise((resolve, reject) => {
            const { sortBy, descending, page, rowsPerPage } = this.internalPagination;
            if (this.parentRepository) {
                if (this.tableType === "multiselect") {
                    this.parentRepository.getAllChildIds(this.$route.fullPath, this.parentId, this.parentProp)
                        .then(childIds => {
                            this.repository.getPage(this.$route.fullPath, this.internalSearch, sortBy, descending, page, rowsPerPage, childIds)
                                .then(data => {
                                    this.loading = false;
                                    resolve({
                                        pageItems: data.pageItems,
                                        totalItems: data.totalItems
                                    });
                                });
                        })
                        .catch(error => {
                            this.loading = false;
                            ErrorMsg.logError("dynamic-data-table.getData", new Error(error));
                            reject("A problem occurred while loading the data.");
                        });
                } else {
                    this.parentRepository.getChildPage(this.$route.fullPath, this.parentId, this.parentProp, this.internalSearch, sortBy, descending, page, rowsPerPage)
                        .then(data => {
                            this.loading = false;
                            resolve({
                                pageItems: data.pageItems,
                                totalItems: data.totalItems
                            });
                        })
                        .catch(error => {
                            this.loading = false;
                            ErrorMsg.logError("dynamic-data-table.getData", new Error(error));
                            reject("A problem occurred while loading the data.");
                        });
                }
            } else {
                this.repository.getPage(this.$route.fullPath, this.internalSearch, sortBy, descending, page, rowsPerPage)
                    .then(data => {
                        this.loading = false;
                        resolve({
                            pageItems: data.pageItems,
                            totalItems: data.totalItems
                        });
                    })
                    .catch(error => {
                        this.loading = false;
                        ErrorMsg.logError("dynamic-data-table.getData", new Error(error));
                        reject("A problem occurred while loading the data.");
                    });
            }
        });
    }

    getRouteData() {
        this.pagination = {};
        let routePagination = this.getRoutePagination();
        if (routePagination) {
            this.pagination.sortBy = routePagination.sortBy;
            this.pagination.descending = routePagination.descending;
            this.pagination.page = routePagination.page;
            this.pagination.rowsPerPage = routePagination.rowsPerPage;
        }
    }

    getRoutePagination(): Pagination {
        if (this.$route.query.sortBy || this.$route.query.descending
            || this.$route.query.page || this.$route.query.rowsPerPage) {
            let page = Number(this.$route.query.page);
            if (isNaN(page)) {
                page = 1;
            }
            let rowsPerPage = Number(this.$route.query.rowsPerPage);
            if (isNaN(rowsPerPage)) {
                rowsPerPage = 5;
            }
            return {
                sortBy: this.$route.query.sortBy,
                descending: this.$route.query.descending === "true",
                page,
                rowsPerPage
            };
        } else {
            return undefined;
        }
    }

    onDelete() {
        this.loading = true;
        if (this.tableType === 'collection') {
            this.repository.removeRangeFromParent(this.$route.fullPath, this.childProp, this.selected.map(i => i[i.primaryKeyProperty]))
                .then(data => {
                    if (data.error) {
                        this.$emit("onError", data.error);
                    } else {
                        for (var i = 0; i < this.selected.length; i++) {
                            this.items.splice(this.items.findIndex(d =>
                                d[d.primaryKeyProperty] == this.selected[i][this.selected[i].primaryKeyProperty]),
                                1);
                        }
                        this.selected = [];
                    }
                    this.loading = false;
                })
                .catch(error => {
                    this.loading = false;
                    this.$emit("onError", "A problem occurred. The item(s) could not be removed.");
                    ErrorMsg.logError("dynamic-data-table.onDelete", new Error(error));
                });
        } else {
            this.repository.removeRange(this.$route.fullPath, this.selected.map(i => i[i.primaryKeyProperty]))
                .then(data => {
                    if (data.error) {
                        this.$emit("onError", data.error);
                    } else {
                        for (var i = 0; i < this.selected.length; i++) {
                            this.items.splice(this.items.findIndex(d =>
                                d[d.primaryKeyProperty] == this.selected[i][this.selected[i].primaryKeyProperty]),
                                1);
                        }
                        this.selected = [];
                    }
                    this.loading = false;
                })
                .catch(error => {
                    this.loading = false;
                    this.$emit("onError", "A problem occurred. The item(s) could not be removed.");
                    ErrorMsg.logError("dynamic-data-table.onDelete", new Error(error));
                });
        }
    }

    onDeleteItem(id: string) {
        this.loading = true;
        this.deletePendingItems.push(id);
        this.cancelDelete(id); // removes from asking
        if (this.tableType === 'collection') {
            this.repository.removeFromParent(this.$route.fullPath, id, this.childProp)
                .then(data => {
                    if (data.error) {
                        this.$emit("onError", data.error);
                    }
                    else {
                        this.items.splice(this.items.findIndex(d => d[d.primaryKeyProperty] == id), 1);
                        let index = this.deletePendingItems.indexOf(id);
                        if (index !== -1) {
                            this.deletePendingItems.splice(index, 1);
                        }
                    }
                    this.loading = false;
                })
                .catch(error => {
                    this.loading = false;
                    this.$emit("onError", "A problem occurred. The item could not be removed.");
                    ErrorMsg.logError("dynamic-data-table.onDeleteItem", new Error(error));
                });
        } else {
            this.repository.remove(this.$route.fullPath, id)
                .then(data => {
                    if (data.error) {
                        this.$emit("onError", data.error);
                    }
                    else {
                        this.items.splice(this.items.findIndex(d => d[d.primaryKeyProperty] == id), 1);
                        let index = this.deletePendingItems.indexOf(id);
                        if (index !== -1) {
                            this.deletePendingItems.splice(index, 1);
                        }
                    }
                    this.loading = false;
                })
                .catch(error => {
                    this.loading = false;
                    this.$emit("onError", "A problem occurred. The item could not be removed.");
                    ErrorMsg.logError("dynamic-data-table.onDeleteItem", new Error(error));
                });
        }
    }

    onDuplicate(id: string) {
        this.loading = true;
        this.repository.duplicate(this.$route.fullPath, id)
            .then(data => {
                if (data.error) {
                    this.$emit("onError", data.error);
                }
                else {
                    this.$router.push({ name: this.dataType, params: { operation: 'add', id: data.data[data.data.primaryKeyProperty] } });
                }
                this.loading = false;
            })
            .catch(error => {
                this.loading = false;
                this.$emit("onError", "A problem occurred. The item could not be copied.");
                ErrorMsg.logError("dynamic-data-table.onDuplicate", new Error(error));
            });
    }

    onNew() {
        this.loading = true;
        this.repository.add(
            this.$route.fullPath,
            this.tableType === 'collection' ? this.childProp : undefined,
            this.tableType === 'collection' ? this.parentId : undefined)
            .then(data => {
                this.loading = false;
                if (data.error) {
                    this.$emit("onError", data.error);
                } else {
                    this.$router.push({ name: this.dataType, params: { operation: 'add', id: data.data[data.data.primaryKeyProperty] } });
                }
            })
            .catch(error => {
                this.loading = false;
                this.$emit("onError", "A problem occurred. The new item could not be added.");
                ErrorMsg.logError("dynamic-data-table.onNew", new Error(error));
            });
    }

    onSearch() {
        this.searchDebounce = 0;
        this.$emit("onPagination", this.internalSearch, this.internalPagination);
        this.refresh();
    }

    onViewItem(id: string) {
        this.$router.push({ name: this.dataType, params: { operation: 'view', id } });
    }

    refresh() {
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateTable, 125);
        }
    }

    updateData() {
        if (this.repository && (this.tableType !== 'child' || this.parentRepository)) {
            this.getData()
                .then((data: PageData<DataItem>) => {
                    this.items = data.pageItems;
                    this.totalItems = data.totalItems;

                    this.deletePermissions = {};
                    if (this.allowEdit) {
                        let deleteAny = this.$store.state.userState.isAdmin; // Admins can delete anything.
                        if (!deleteAny) {
                            let permission = this.$store.getters.getPermission(this.dataType);
                            if (permission === permissions.permissionDataAll) {
                                deleteAny = true;
                            }
                        }
                        for (var i = 0; i < this.items.length; i++) {
                            if (deleteAny) {
                                this.deletePermissions[this.items[i][this.items[i].primaryKeyProperty]] = true;
                            } else {
                                let permission = this.$store.getters.getPermission(this.dataType, this.items[i][this.items[i].primaryKeyProperty]);
                                this.deletePermissions[this.items[i][this.items[i].primaryKeyProperty]] =
                                    permission === permissions.permissionDataAll;
                            }
                        }
                    }
                    this.canDelete = Object.keys(this.deletePermissions).length > 0;
                })
                .catch(error => {
                    this.$emit("onError", "A problem occurred while loading the data.");
                    ErrorMsg.logError("dynamic-data-table.updateData", new Error(error));
                });
        }
    }

    updateTable() {
        this.updateTimeout = 0;
        this.loading = true;

        let permission = this.$store.getters.getPermission(this.dataType);
        this.canAdd = permissionIncludesTarget(permission, permissions.permissionDataAdd);

        this.headers = [];
        this.repository.getFieldDefinitions(this.$route.fullPath)
            .then(defData => {
                this.headers = [];
                defData.forEach(field => {
                    if (!field.hideInTable) {
                        let h: TableHeader = {
                            text: field.label || field.placeholder,
                            value: field.model,
                            sortable: ((field.type === 'input'
                                && (field.inputType === 'text'
                                    || field.inputType === 'number'
                                    || field.inputType === 'email'
                                    || field.inputType === 'telephone'
                                    || field.inputType === 'range'
                                    || field.inputType === 'time'
                                    || field.inputType === 'date'
                                    || field.inputType === 'datetime'
                                    || field.inputType === 'datetime-local'))
                                || field.type === 'vuetifyText'
                                || field.type === 'vuetifyDateTime'
                                || field.type === 'vuetifyTimespan'),
                            cultural: field.inputType === 'cultural'
                        };
                        if (field.type === 'vuetifySelect'
                            || field.type === 'vuetifyDateTime'
                            || field.type === 'vuetifyTimespan') {
                            h.value = field.model + "Formatted";
                        }
                        if (field.isName) {
                            h.align = 'left';
                            this.headers.unshift(h);
                        } else {
                            this.headers.push(h);
                        }
                    }
                });
                this.updateData();
                this.loading = false;
            })
            .catch(error => {
                this.loading = false;
                this.$emit("onError", "A problem occurred while updating the data.");
                ErrorMsg.logError("dynamic-data-table.updateTable", error);
            });
    }
}
