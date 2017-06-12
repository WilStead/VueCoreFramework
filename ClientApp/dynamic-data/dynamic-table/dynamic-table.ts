import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as ErrorMsg from '../../error-msg';
import { DataItem, Repository } from '../../store/repository';

interface TableHeader {
    text: string;
    value: string;
    left?: boolean;
    sortable?: boolean;
}

@Component
export default class DynamicTableComponent extends Vue {
    @Prop()
    operation: string;

    @Prop()
    parentId: string;

    @Prop()
    parentProp: string;

    @Prop()
    parentType: string;

    @Prop()
    repositoryType: string;

    @Prop()
    routeName: string;

    activity = false;
    childItems: Array<any> = [];
    childLoading = true;
    childPagination: any = {};
    childSearch = '';
    deleteDialogShown = false;
    deleteAskingChildItems = [];
    deleteAskingItems = [];
    deletePendingChildItems = [];
    deletePendingItems = [];
    errorMessage = '';
    headers: Array<TableHeader> = [];
    items: Array<any> = [];
    loading = true;
    pagination: any = {};
    parentRepository: Repository = null;
    repository: Repository = null;
    search = '';
    selectErrorDialogMessage = '';
    selectErrorDialogShown = false;
    selected: Array<any> = [];
    selectedChildren: Array<any> = [];
    totalChildItems = 0;
    totalItems = 0;
    updateTimeout = 0;

    @Watch('childPagination', { deep: true })
    onChildPaginationChange(val: any, oldVal: any) {
        this.updateChildData();
    }

    @Watch('childSearch')
    onChildSearchChange(val: string, oldVal: string) {
        this.updateChildData();
    }

    @Watch('parentType')
    onParentTypeChanged(val: string, oldVal: string) {
        this.parentRepository = new Repository(val);
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateTable, 125);
        }
    }

    @Watch('repositoryType')
    onRepositoryTypeChanged(val: string, oldVal: string) {
        this.repository = new Repository(val);
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateTable, 125);
        }
    }

    @Watch('search')
    onSearchChange(val: string, oldVal: string) {
        this.updateData();
    }

    @Watch('pagination', { deep: true })
    onPaginationChange(val: any, oldVal: any) {
        this.updateData();
    }

    mounted() {
        this.repository = new Repository(this.repositoryType);
        if (this.parentType && this.parentId) {
            this.parentRepository = new Repository(this.parentType);
        }
        this.updateTable();
    }

    cancelDelete(id: string) {
        let index = this.deleteAskingItems.indexOf(id);
        if (index !== -1) {
            this.deleteAskingItems.splice(index, 1);
        }
    }

    cancelDeleteChild(id: string) {
        let index = this.deleteAskingChildItems.indexOf(id);
        if (index !== -1) {
            this.deleteAskingChildItems.splice(index, 1);
        }
    }

    getChildData() {
        this.childLoading = true;
        return new Promise((resolve, reject) => {
            const { sortBy, descending, page, rowsPerPage } = this.childPagination;
            this.parentRepository.getChildPage(this.$route.fullPath, this.parentId, this.parentProp, this.childSearch, sortBy, descending, page, rowsPerPage)
                .then(data => {
                    this.childLoading = false;
                    resolve({
                        items: data.pageItems,
                        total: data.totalItems
                    });
                })
                .catch(error => {
                    this.childLoading = false;
                    ErrorMsg.logError("dynamic-table.getChildData", new Error(error));
                    reject("A problem occurred while loading the data.");
                });
        });
    }

    getData() {
        this.loading = true;
        return new Promise((resolve, reject) => {
            const { sortBy, descending, page, rowsPerPage } = this.pagination;
            if (this.parentRepository) {
                if (this.operation === "multiselect") {
                    this.parentRepository.getAllChildIds(this.$route.fullPath, this.parentId, this.parentProp)
                        .then(childIds => {
                            this.repository.getPage(this.$route.fullPath, this.search, sortBy, descending, page, rowsPerPage, childIds)
                                .then(data => {
                                    this.loading = false;
                                    resolve({
                                        items: data.pageItems,
                                        total: data.totalItems
                                    });
                                })
                                .catch(error => {
                                    this.loading = false;
                                    ErrorMsg.logError("dynamic-table.getData", new Error(error));
                                    reject("A problem occurred while loading the data.");
                                });
                        })
                        .catch(error => {
                            this.loading = false;
                            ErrorMsg.logError("dynamic-table.getData", new Error(error));
                            reject("A problem occurred while loading the data.");
                        });
                } else {
                    this.parentRepository.getChildPage(this.$route.fullPath, this.parentId, this.parentProp, this.search, sortBy, descending, page, rowsPerPage)
                        .then(data => {
                            this.loading = false;
                            resolve({
                                items: data.pageItems,
                                total: data.totalItems
                            });
                        })
                        .catch(error => {
                            this.loading = false;
                            ErrorMsg.logError("dynamic-table.getData", new Error(error));
                            reject("A problem occurred while loading the data.");
                        });
                }
            } else {
                this.repository.getPage(this.$route.fullPath, this.search, sortBy, descending, page, rowsPerPage)
                    .then(data => {
                        this.loading = false;
                        resolve({
                            items: data.pageItems,
                            total: data.totalItems
                        });
                    })
                    .catch(error => {
                        this.loading = false;
                        ErrorMsg.logError("dynamic-table.getData", new Error(error));
                        reject("A problem occurred while loading the data.");
                    });
            }
        });
    }

    onAddSelect() {
        this.activity = true;
        this.parentRepository.addToParentCollection(this.$route.fullPath, this.parentId, this.parentProp, this.selected.map(c => c.id))
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                }
                else {
                    this.selected = [];
                    this.updateData();
                    this.updateChildData();
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred. The item could not be removed.";
                this.activity = false;
                ErrorMsg.logError("dynamic-table.onDeleteChildItem", new Error(error));
            });
    }

    onCancel() {
        this.activity = false;
        this.errorMessage = '';
        this.$router.go(-1);
    }

    onDelete() {
        this.activity = true;
        this.repository.removeRange(this.$route.fullPath, this.selected.map(i => i.id))
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                } else {
                    for (var i = 0; i < this.selected.length; i++) {
                        this.items.splice(this.items.findIndex(d => d.id == this.selected[i].id), 1);
                    }
                    this.selected = [];
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred. The item(s) could not be removed.";
                this.activity = false;
                ErrorMsg.logError("dynamic-table.onDelete", new Error(error));
            });
    }

    onDeleteChildItem(id: string) {
        this.activity = true;
        this.deletePendingChildItems.push(id);
        this.cancelDeleteChild(id); // removes from asking
        this.repository.remove(this.$route.fullPath, id)
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                }
                else {
                    this.items.splice(this.items.findIndex(d => d.id == id), 1);
                    let index = this.deletePendingChildItems.indexOf(id);
                    if (index !== -1) {
                        this.deletePendingChildItems.splice(index, 1);
                    }
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred. The item could not be removed.";
                this.activity = false;
                ErrorMsg.logError("dynamic-table.onDeleteChildItem", new Error(error));
            });
    }

    onDeleteItem(id: string) {
        this.activity = true;
        this.deletePendingItems.push(id);
        this.cancelDelete(id); // removes from asking
        this.repository.remove(this.$route.fullPath, id)
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                }
                else {
                    this.items.splice(this.items.findIndex(d => d.id == id), 1);
                    let index = this.deletePendingItems.indexOf(id);
                    if (index !== -1) {
                        this.deletePendingItems.splice(index, 1);
                    }
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred. The item could not be removed.";
                this.activity = false;
                ErrorMsg.logError("dynamic-table.onDeleteItem", new Error(error));
            });
    }

    onNew() {
        if (this.operation === 'collection') {
            this.$router.push({
                name: this.routeName, params: {
                    operation: 'create',
                    id: Date.now().toString(),
                    parentType: this.parentType,
                    parentId: this.parentId,
                    parentProp: this.parentProp
                }
            });
        } else {
            this.$router.push({ name: this.routeName, params: { operation: 'create', id: Date.now().toString() } });
        }
    }

    onRemoveSelect() {
        this.activity = true;
        this.parentRepository.removeFromParentCollection(this.$route.fullPath, this.parentId, this.parentProp, this.selectedChildren.map(c => c.id))
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                }
                else {
                    this.selectedChildren = [];
                    this.updateData();
                    this.updateChildData();
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred. The item could not be removed.";
                this.activity = false;
                ErrorMsg.logError("dynamic-table.onDeleteChildItem", new Error(error));
            });
    }

    onSelectItems() {
        if (!this.selected.length) {
            this.selectErrorDialogMessage = "You have not selected an item.";
            this.selectErrorDialogShown = true;
        } else if (this.selected.length > 1) {
            this.selectErrorDialogMessage = "You can only select a single item.";
            this.selectErrorDialogShown = true;
        } else if (this.parentRepository && this.parentProp) {
            this.parentRepository.find(this.$route.fullPath, this.parentId)
                .then(data => {
                    if (data.error) {
                        this.errorMessage = data.error;
                        this.activity = false;
                    } else {
                        let vm = data.data;
                        vm[this.parentProp + "Id"] = this.selected[0].id;
                        this.parentRepository.update(this.$route.fullPath, vm)
                            .then(data => {
                                if (data.error) {
                                    this.errorMessage = data.error;
                                } else {
                                    this.$router.go(-1);
                                }
                                this.activity = false;
                            })
                            .catch(error => {
                                this.errorMessage = "A problem occurred. The item could not be updated.";
                                this.activity = false;
                                ErrorMsg.logError("dynamic-table.onSelectItems", new Error(error));
                            });
                    }
                })
                .catch(error => {
                    this.errorMessage = "A problem occurred while updating the data.";
                    this.activity = false;
                    ErrorMsg.logError("dynamic-table.onSelectItems", new Error(error));
                });
        } else {
            this.errorMessage = "There was a problem saving your selection. Please try going back to the previous page before trying again.";
        }
    }

    onViewChildItem(id: string) {
        this.$router.push({ name: this.parentType.toLowerCase(), params: { operation: 'details', id } });
    }

    onViewItem(id: string) {
        this.$router.push({ name: this.routeName, params: { operation: 'details', id } });
    }

    updateChildData() {
        if (this.parentRepository) {
            this.getChildData()
                .then((data: any) => {
                    this.childItems = data.items;
                    this.totalChildItems = data.total;
                })
                .catch(error => {
                    this.errorMessage = "A problem occurred while loading the data.";
                    ErrorMsg.logError("dynamic-table.updateChildData", new Error(error));
                });
        }
    }

    updateData() {
        if (this.repository) {
            this.getData()
                .then((data: any) => {
                    this.items = data.items;
                    this.totalItems = data.total;
                })
                .catch(error => {
                    this.errorMessage = "A problem occurred while loading the data.";
                    ErrorMsg.logError("dynamic-table.updateData", new Error(error));
                });
        }
    }

    updateTable() {
        this.updateTimeout = 0;
        this.activity = true;
        this.headers = [];
        this.repository.getFieldDefinitions(this.$route.fullPath)
            .then(defData => {
                defData.forEach(field => {
                    if (!field.hideInTable && field.visible !== false) {
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
                                || field.type === 'vuetifyTimespan')
                        };
                        if (field.type === 'vuetifySelect'
                            || field.type === 'vuetifyDateTime'
                            || field.type === 'vuetifyTimespan') {
                            h.value = field.model + "Formatted";
                        }
                        if (h.text === 'Name') {
                            h.left = true;
                            this.headers.unshift(h);
                        } else {
                            this.headers.push(h);
                        }
                    }
                });
                this.updateData();
                this.updateChildData();
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred while updating the data.";
                this.activity = false;
                ErrorMsg.logError("dynamic-table.mounted", error);
            });
    }
}
