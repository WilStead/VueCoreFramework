import Vue from 'vue';
import VueRouter from 'vue-router';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as ErrorMsg from '../../error-msg';
import { permissionIncludesTarget, permissions, ShareData } from '../../store/userStore';
import { ApiResponseViewModel, checkResponse } from '../../router';

@Component
export default class DashboardComponent extends Vue {
    @Prop()
    fontAwesome: boolean;

    @Prop()
    iconClass: string;

    @Prop()
    title: string;

    allPermission = false;
    groupMembers: string[] = [];
    permissionOptions = [
        { text: 'View', value: permissions.permissionDataView },
        { text: 'Edit', value: permissions.permissionDataEdit },
        { text: 'Add', value: permissions.permissionDataAdd },
        { text: 'All', value: permissions.permissionDataAll }
    ];
    routeName = '';
    selectedPermission = null;
    selectedShareGroup = null;
    selectedShareUsername = null;
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

    @Watch('shareDialog')
    onShareDialogChange(val: boolean, oldVal: boolean) {
        if (val) {
            this.updateShares();
        }
    }

    @Watch('$route', { deep: true })
    onRouteChange(val: VueRouter.Route) {
        this.getRouteData();
    }

    mounted() {
        this.getRouteData();
    }

    getRouteData() {
        this.routeName = this.$route.name.endsWith('DataTable')
            ? this.$route.name.substr(0, this.$route.name.length - 9) // remove 'DataTable'
            : this.$route.name;
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
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                } else {
                    this.updateShares();
                    this.shareSuccessMessage = 'Success';
                }
                this.shareActivity = false;
            })
            .catch(error => {
                this.shareErrorMessage = 'A problem occurred.';
                if (error && error.message && error.message.startsWith('CODE:')) {
                    this.shareErrorMessage += error.message.replace('CODE:', '');
                }
                ErrorMsg.logError('dynamic-table.onHide', error);
            });
    }

    onSelectedShareGroupChange(val: string, oldVal: string) {
        this.shareGroup = val;
    }

    onSelectedShareUsernameChange(val: string, oldVal: string) {
        this.shareUsername = val;
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
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                } else {
                    this.updateShares();
                    this.shareSuccessMessage = 'Success';
                }
                this.shareActivity = false;
            })
            .catch(error => {
                this.shareErrorMessage = 'A problem occurred.';
                if (error && error.message && error.message.startsWith('CODE:')) {
                    this.shareErrorMessage += error.message.replace('CODE:', '');
                }
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
                        'Accept': `application/json;v=${this.$store.state.apiVer}`,
                        'Accept-Language': this.$store.state.userState.culture,
                        'Authorization': `bearer ${this.$store.state.userState.token}`
                    }
                })
                .then(response => checkResponse(response, this.$route.fullPath))
                .then(response => {
                    if (!response.ok) {
                        if (response.status === 404) {
                            this.shareGroupSuggestion = '';
                        } else {
                            throw new Error(`CODE:${response.statusText}`);
                        }
                    } else {
                        this.shareGroupSuggestion = response.statusText;
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
                        'Accept': `application/json;v=${this.$store.state.apiVer}`,
                        'Accept-Language': this.$store.state.userState.culture,
                        'Authorization': `bearer ${this.$store.state.userState.token}`
                    }
                })
                .then(response => checkResponse(response, this.$route.fullPath))
                .then(response => {
                    if (!response.ok) {
                        if (response.status === 404) {
                            this.shareUsernameSuggestion = '';
                        } else {
                            throw new Error(`CODE:${response.statusText}`);
                        }
                    } else {
                        this.shareUsernameSuggestion = response.statusText;
                    }
                })
                .catch(error => {
                    ErrorMsg.logError('dynamic-table.suggestShareUsername', error);
                });
        }
    }

    updateShares() {
        fetch(`/api/Share/GetCurrentShares/${this.routeName}`,
            {
                method: 'GET',
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                }
                return response;
            })
            .then(response => response.json() as Promise<ShareData[]>)
            .then(data => {
                this.shares = [];
                for (var i = 0; i < data.length; i++) {
                    this.shares[i] = data[i];
                    this.shares[i].id = i;
                }
            })
            .catch(error => {
                ErrorMsg.logError('dynamic-table.updateShares', error);
            });
        fetch(`/api/Share/GetShareableGroupMembers`,
            {
                method: 'GET',
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                }
                return response;
            })
            .then(response => response.json() as Promise<string[]>)
            .then(data => {
                this.groupMembers = data;
            })
            .catch(error => {
                ErrorMsg.logError('dynamic-table.updateShares', error);
            });
        fetch(`/api/Share/GetShareableGroupSubset`,
            {
                method: 'GET',
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                }
                return response;
            })
            .then(response => response.json() as Promise<string[]>)
            .then(data => {
                this.shareGroups = data;
            })
            .catch(error => {
                this.shareGroups = [];
                ErrorMsg.logError('dynamic-table.updateShares', error);
            });
    }
}