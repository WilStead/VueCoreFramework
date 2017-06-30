import Vue from 'vue';
import VueRouter from 'vue-router';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as ErrorMsg from '../../error-msg';
import { DataItem, PageData, Repository } from '../../store/repository';
import { permissionIncludesTarget, permissions, ShareData } from '../../store/userStore';
import { ApiResponseViewModel, checkResponse } from '../../router';
import { Pagination } from './dynamic-data-table';

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
    left?: boolean;

    /**
     * Optionally indicates that this column is sortable (false by default).
     */
    sortable?: boolean;
}

@Component({
    components: {
        DynamicDataTable: require('./dynamic-data-table.vue')
    }
})
export default class DynamicTableComponent extends Vue {
    @Prop()
    childProp: string;

    @Prop()
    operation: string;

    @Prop()
    parentId: string;

    @Prop()
    parentProp: string;

    @Prop()
    parentType: string;

    activity = false;
    allPermission = false;
    canShare = false;
    childPagination: Pagination = {};
    errorMessage = '';
    groupMembers: string[] = [];
    pagination: Pagination = {};
    parentRepository: Repository = null;
    permissionOptions = [
        { text: 'View', value: permissions.permissionDataView },
        { text: 'Edit', value: permissions.permissionDataEdit },
        { text: 'Add', value: permissions.permissionDataAdd },
        { text: 'All', value: permissions.permissionDataAll }
    ];
    repository: Repository = null;
    routeName = '';
    selectedPermission = null;
    selectedShareGroup = null;
    selectedShareUsername = null;
    selectErrorDialogMessage = '';
    selectErrorDialogShown = false;
    selected: Array<DataItem> = [];
    selectedChildren: Array<DataItem> = [];
    shareActivity = false;
    shareDialog = false;
    shareErrorMessage = '';
    shareSuccessMessage = '';
    shareGroups: string[] = [];
    shareGroup = '';
    shareGroupSuggestion = '';
    shareGroupTimeout = 0;
    shareUsernameSuggestion = '';
    shareUsernameTimeout = 0;
    shares: ShareData[] = [];
    shareUsername = '';
    shareWithAll = false;
    updateTimeout = 0;

    @Watch('parentType')
    onParentTypeChanged(val: string, oldVal: string) {
        this.parentRepository = this.$store.getters.getRepository(val);
        this.refresh();
    }

    @Watch('shareDialog')
    onShareDialogChange(val: boolean, oldVal: boolean) {
        if (val) {
            this.updateShares();
        }
    }

    @Watch('$route', { deep: true })
    onRouteChange(val: VueRouter.Route) {
        this.getRouteData();
        this.repository = this.$store.getters.getRepository(this.routeName);
        this.refresh();
    }

    mounted() {
        this.getRouteData();
        this.repository = this.$store.getters.getRepository(this.routeName);
        if (this.parentType) {
            this.parentRepository = this.$store.getters.getRepository(this.parentType);
        }
        this.refresh();
    }

    getChildRoutePagination(): Pagination {
        if (this.$route.query.childSortBy || this.$route.query.childDescending
            || this.$route.query.childPage || this.$route.query.childRowsPerPage) {
            let page = Number(this.$route.query.childPage);
            if (isNaN(page)) {
                page = 1;
            }
            let rowsPerPage = Number(this.$route.query.childRowsPerPage);
            if (isNaN(rowsPerPage)) {
                rowsPerPage = 5;
            }
            return {
                sortBy: this.$route.query.childSortBy,
                descending: this.$route.query.childDescending === "true",
                page,
                rowsPerPage
            };
        } else {
            return undefined;
        }
    }

    getRouteData() {
        this.routeName = this.$route.name.substr(0, this.$route.name.length - 9); // remove 'DataTable'
        let routePagination = this.getRoutePagination();
        let childRoutePagination = this.getChildRoutePagination();
        if (routePagination) {
            this.pagination.sortBy = routePagination.sortBy;
            this.pagination.descending = routePagination.descending;
            this.pagination.page = routePagination.page;
            this.pagination.rowsPerPage = routePagination.rowsPerPage;
        }
        if (childRoutePagination) {
            this.childPagination.sortBy = childRoutePagination.sortBy;
            this.childPagination.descending = childRoutePagination.descending;
            this.childPagination.page = childRoutePagination.page;
            this.childPagination.rowsPerPage = childRoutePagination.rowsPerPage;
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

    onAddSelect() {
        this.activity = true;
        this.parentRepository.addChildrenToCollection(this.$route.fullPath, this.parentId, this.parentProp, this.selected.map(c => c[c.primaryKeyProperty]))
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                }
                else {
                    this.selected = [];
                    this.refresh();
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred. The item could not be removed.";
                this.activity = false;
                ErrorMsg.logError("dynamic-table.onAddSelect", new Error(error));
            });
    }

    onCancel() {
        this.activity = false;
        this.errorMessage = '';
        this.$router.go(-1);
    }

    onChildPagination(search: string, pagination: Pagination) {
        this.$router.push({
            name: this.$route.name,
            params: this.$route.params,
            query: {
                search: this.$route.query.search || '',
                sortBy: this.pagination.sortBy || '',
                descending: this.pagination.descending.toString(),
                page: this.pagination.page.toString(),
                rowsPerPage: this.pagination.rowsPerPage.toString(),
                childSearch: search || '',
                childSortBy: pagination.sortBy || '',
                childDescending: pagination.descending.toString(),
                childPage: pagination.page.toString(),
                childRowsPerPage: pagination.rowsPerPage.toString()
            }
        });
    }

    onError(error: string) {
        this.errorMessage = error;
    }

    onHide(share: ShareData) {
        this.shareActivity = true;
        this.shareErrorMessage = '';
        this.shareSuccessMessage = '';
        let action: string;
        if (share.name === 'All Users') {
            action = 'HideDataFromAll';
        }
        if (share.type === 'user') {
            action = `HideDataFromUser/${share.name}`;
        } else {
            action = `HideDataFromGroup/${share.name}`;
        }
        fetch(`/api/Share/${action}/${this.routeName}?operation=${encodeURIComponent(share.level)}`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.error) {
                    this.shareErrorMessage = data.error;
                } else {
                    this.updateShares();
                    this.shareSuccessMessage = 'Success';
                }
                this.shareActivity = false;
            })
            .catch(error => {
                this.shareErrorMessage = 'A problem occurred.';
                ErrorMsg.logError('dynamic-table.onHide', error);
            });
    }

    onPagination(search: string, pagination: Pagination) {
        let query: any = {
            search: search || '',
            sortBy: pagination.sortBy || '',
            descending: pagination.descending.toString(),
            page: pagination.page.toString(),
            rowsPerPage: pagination.rowsPerPage.toString()
        };
        if (this.parentType) {
            query.childSearch = this.$route.query.childSearch || '';
            query.childSortBy = this.childPagination.sortBy || '';
            query.childDescending = this.childPagination.descending.toString();
            query.childPage = this.childPagination.page.toString();
            query.childRowsPerPage = this.childPagination.rowsPerPage.toString();
        }
        this.$router.push({
            name: this.$route.name,
            params: this.$route.params,
            query
        });
    }

    onRemoveSelect() {
        this.activity = true;
        this.parentRepository.removeChildrenFromCollection(this.$route.fullPath, this.parentId, this.parentProp, this.selectedChildren.map(c => c[c.primaryKeyProperty]))
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                }
                else {
                    this.selectedChildren = [];
                    this.refresh();
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred. The item could not be removed.";
                this.activity = false;
                ErrorMsg.logError("dynamic-table.onDeleteChildItem", new Error(error));
            });
    }

    onSelectedShareGroupChange(val: string, oldVal: string) {
        this.shareGroup = val;
    }

    onSelectedShareUsernameChange(val: string, oldVal: string) {
        this.shareUsername = val;
    }

    onSelectItem() {
        if (!this.selected.length) {
            this.selectErrorDialogMessage = "You have not selected an item.";
            this.selectErrorDialogShown = true;
        } else if (this.selected.length > 1) {
            this.selectErrorDialogMessage = "You can only select a single item.";
            this.selectErrorDialogShown = true;
        } else if (this.childProp) {
            this.repository.replaceChild(this.$route.fullPath, this.parentId, this.selected[0][this.selected[0].primaryKeyProperty], this.childProp)
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
                    ErrorMsg.logError("dynamic-table.onSelectItem", new Error(error));
                });
        } else {
            this.errorMessage = "There was a problem saving your selection. Please try going back to the previous page before trying again.";
        }
    }

    onShare() {
        if (this.selectedPermission || this.allPermission) {
            if (this.shareWithAll) {
                this.share('ShareDataWithAll');
            }
            if (this.shareGroup) {
                this.share('ShareDataWithGroup', this.shareGroup);
            }
            if (this.shareUsername) {
                this.share('ShareDataWithUser', this.shareUsername);
            }
        }
    }

    onShareGroupChange(val: string, oldVal: string) {
        if (this.shareGroupTimeout === 0) {
            this.shareGroupTimeout = setTimeout(this.suggestShareGroup, 500);
        }
    }

    onShareUsernameChange(val: string, oldVal: string) {
        if (this.shareUsernameTimeout === 0) {
            this.shareUsernameTimeout = setTimeout(this.suggestShareUsername, 500);
        }
    }

    refresh() {
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateTable, 125);
        }
    }

    share(action: string, target?: string) {
        this.shareActivity = true;
        this.shareErrorMessage = '';
        this.shareSuccessMessage = '';
        let url = `/api/Share/${action}`;
        if (target) {
            url += `/${target}`;
        }
        url += `/${this.routeName}`;
        if (!this.allPermission) {
            url += `?operation=${encodeURIComponent(this.selectedPermission)}`;
        }
        fetch(url,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.error) {
                    this.shareErrorMessage = data.error;
                } else {
                    this.updateShares();
                    this.shareSuccessMessage = 'Success';
                }
                this.shareActivity = false;
            })
            .catch(error => {
                this.shareErrorMessage = 'A problem occurred.';
                this.shareActivity = false;
                ErrorMsg.logError('dynamic-table.share', error);
            });
    }

    suggestShareGroup() {
        this.shareGroupTimeout = 0;
        if (this.shareGroup) {
            fetch(`/api/Share/GetShareableGroupCompletion/${this.shareGroup}`,
                {
                    method: 'GET',
                    headers: {
                        'Accept': 'application/json',
                        'Authorization': `bearer ${this.$store.state.userState.token}`
                    }
                })
                .then(response => checkResponse(response, this.$route.fullPath))
                .then(response => response.json() as Promise<ApiResponseViewModel>)
                .then(data => {
                    if (data['error']) {
                        throw new Error(`There was a problem retrieving a share group suggestion: ${data['error']}`);
                    } else {
                        this.shareGroupSuggestion = data.response;
                    }
                })
                .catch(error => {
                    ErrorMsg.logError('dynamic-table.suggestShareGroup', error);
                });
        }
    }

    suggestShareUsername() {
        this.shareUsernameTimeout = 0;
        if (this.shareUsername) {
            fetch(`/api/Share/GetShareableUsernameCompletion/${this.shareUsername}`,
                {
                    method: 'GET',
                    headers: {
                        'Accept': 'application/json',
                        'Authorization': `bearer ${this.$store.state.userState.token}`
                    }
                })
                .then(response => checkResponse(response, this.$route.fullPath))
                .then(response => response.json() as Promise<ApiResponseViewModel>)
                .then(data => {
                    if (data['error']) {
                        throw new Error(`There was a problem retrieving a share group suggestion: ${data['error']}`);
                    } else {
                        this.shareUsernameSuggestion = data.response;
                    }
                })
                .catch(error => {
                    ErrorMsg.logError('dynamic-table.suggestShareUsername', error);
                });
        }
    }

    updateData() {
        if (this.repository) {
            this.$children.forEach(child => {
                if (typeof child['refresh'] === "function") {
                    let f: Function = child['refresh'];
                    f();
                }
            });
        }
    }

    updateShares() {
        fetch(`/api/Share/GetCurrentShares/${this.routeName}`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<Array<ShareData>>)
            .then(data => {
                if (data['error']) {
                    throw new Error(`There was a problem retrieving current shares: ${data['error']}`);
                } else {
                    this.shares = [];
                    for (var i = 0; i < data.length; i++) {
                        this.shares[i] = data[i];
                        this.shares[i].id = i;
                    }
                }
            })
            .catch(error => {
                ErrorMsg.logError('dynamic-table.updateShares', error);
            });
        fetch(`/api/Share/GetShareableGroupMembers`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<Array<string>>)
            .then(data => {
                if (data['error']) {
                    throw new Error(`There was a problem retrieving sharable group members: ${data['error']}`);
                } else {
                    this.groupMembers = data;
                }
            })
            .catch(error => {
                ErrorMsg.logError('dynamic-table.updateShares', error);
            });
        fetch(`/api/Share/GetShareableGroupSubset`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<Array<string>>)
            .then(data => {
                if (data['error']) {
                    this.shareGroups = [];
                    throw new Error(`There was a problem retrieving sharable groups: ${data['error']}`);
                } else {
                    this.shareGroups = data;
                }
            })
            .catch(error => {
                ErrorMsg.logError('dynamic-table.updateShares', error);
            });
    }

    updateTable() {
        this.updateTimeout = 0;

        this.canShare = this.$store.state.userState.isAdmin && this.$store.getters.getSharePermission(this.routeName);

        this.updateData();
    }
}
