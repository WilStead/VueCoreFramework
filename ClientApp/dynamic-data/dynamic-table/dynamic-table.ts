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
    repository: Repository;

    @Prop()
    routeName: string;
    
    activity = false;
    deleteDialogShown = false;
    deleteAskingItems = [];
    deletePendingItems = [];
    errorMessage = '';
    headers: Array<TableHeader> = [];
    items: Array<any> = [];
    loading = true;
    pagination: any = {};
    search = '';
    selected: Array<any> = [];
    totalItems = 0;

    @Watch('search')
    onSearchChange(val: string, oldVal: string) {
        this.updateData();
    }

    @Watch('pagination', { immediate: true, deep: true })
    onPaginationChange(val: any, oldVal: any) {
        this.updateData();
    }

    cancelDelete(id: string) {
        let index = this.deleteAskingItems.indexOf(id);
        if (index !== -1) {
            this.deleteAskingItems.splice(index, 1);
        }
    }

    getData() {
        this.loading = true;
        return new Promise((resolve, reject) => {
            const { sortBy, descending, page, rowsPerPage } = this.pagination;
            this.repository.getPage(this.$route.fullPath, this.search, sortBy, descending, page, rowsPerPage)
                .then(data => {
                    this.loading = false;
                    resolve({
                        items: data.pageItems,
                        total: data.totalItems
                    });
                })
                .catch(error => {
                    this.errorMessage = "A problem occurred while loading the data.";
                    this.loading = false;
                    ErrorMsg.logError("dynamic-table.getData", error);
                });
        });
    }

    mounted() {
        this.activity = true;
        this.repository.getFieldDefinitions(this.$route.fullPath)
            .then(defData => {
                defData.forEach(field => {
                    if (!field.hideInTable && field.visible !== false) {
                        let h: TableHeader = {
                            text: field.label || field.placeholder,
                            value: field.model,
                            sortable: field.type === 'input'
                            && (field.inputType === 'text'
                                || field.inputType === 'number'
                                || field.inputType === 'email'
                                || field.inputType === 'telephone'
                                || field.inputType === 'range'
                                || field.inputType === 'time'
                                || field.inputType === 'date'
                                || field.inputType === 'datetime'
                                || field.inputType === 'datetime-local')
                        };
                        if (h.text === 'Name') {
                            h.left = true;
                            this.headers.unshift(h);
                        } else {
                            this.headers.push(h);
                        }
                    }
                });
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred while updating the data.";
                this.activity = false;
                ErrorMsg.logError("dynamic-form.updateForm", error);
            });
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
                ErrorMsg.logError("dynamic-table.onDelete", error);
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
                ErrorMsg.logError("dynamic-table.onDeleteItem", error);
            });
    }

    onDetail(id: string) {
        this.$router.push({ name: this.routeName, params: { operation: 'details', id } });
    }

    onEdit(id: string) {
        this.$router.push({ name: this.routeName, params: { operation: 'edit', id } });
    }

    onNew() {
        this.$router.push({ name: this.routeName, params: { operation: 'create', id: Date.now().toString() } });
    }

    updateData() {
        this.getData()
            .then((data: any) => {
                this.items = data.items;
                this.totalItems = data.total;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred while loading the data.";
                ErrorMsg.logError("dynamic-table.updateData", error);
            });
    }
}
