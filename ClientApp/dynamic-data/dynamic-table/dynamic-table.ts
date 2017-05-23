import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as ErrorMsg from '../../components/error/error-msg';
import { FieldDefinition } from '../field-definition';
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
    repository: Repository<any>;

    @Prop()
    routeName: string;

    @Prop()
    vmDefinition: Array<FieldDefinition>;

    activity = false;
    deleteDialogShown = false;
    deleteItemDialogShown = {};
    headers: Array<TableHeader> = [];
    items: Array<any> = [];
    loading = true;
    pagination: any = {};
    search = '';
    selected: Array<any> = [];
    totalItems = 0;

    @Watch('pagination', { immediate: true, deep: true })
    onPaginationChange(val: any, oldVal: any) {
        this.getData()
            .then((data: any) => {
                this.items = data.items;
                this.totalItems = data.total;
            });
    }

    getData() {
        this.loading = true;
        return new Promise((resolve, reject) => {
            const { sortBy, descending, page, rowsPerPage } = this.pagination;
            this.repository.getAll()
                .then(data => {
                    const total = data.length;
                    let items = data.slice();

                    if (sortBy) {
                        items.sort((a, b) => {
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
                        items = items.slice((page - 1) * rowsPerPage, page * rowsPerPage);
                    }

                    this.loading = false;
                    resolve({ items, total });
                });
        });
    }

    mounted() {
        this.vmDefinition.forEach(field => {
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
    }

    onDelete() {
        this.activity = true;
        this.$store.state.countryData.removeRange(this.selected.map(i => i.id))
            .then(() => {
                for (var i = 0; i < this.selected.length; i++) {
                    this.items.splice(this.items.findIndex(d => d.id == this.selected[i].id), 1);
                }
                this.selected = [];
                this.activity = false;
            })
            .catch(error => {
                this.activity = false;
                ErrorMsg.showErrorMsgAndLog("A problem occurred. The item(s) could not be removed.", error);
            });
    }

    onDeleteItem(id: string) {
        this.activity = true;
        this.$store.state.countryData.remove(id)
            .then(() => {
                this.items.splice(this.items.findIndex(d => d.id == id), 1);
                this.activity = false;
            })
            .catch(error => {
                this.activity = false;
                ErrorMsg.showErrorMsgAndLog("A problem occurred. The item could not be removed.", error);
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
}
