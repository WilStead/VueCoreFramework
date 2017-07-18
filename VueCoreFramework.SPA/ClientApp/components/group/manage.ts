import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as Store from '../../store/store';
import { checkResponse, ApiResponseViewModel } from '../../router';
import * as ErrorMsg from '../../error-msg';

export interface Group {
    name: string;
    manager: string;
    members: string[];
}

@Component
export default class ManageGroupComponent extends Vue {
    activity = false;
    createErrorMessage = '';
    createGroupDialog = false;
    deleteFoundGroupDialog = false;
    deleteGroup: Group = null;
    deleteGroupDialog = false;
    errorMessage = '';
    foundGroup: Group = null;
    inviteDialog = false;
    inviteGroup: Group = null;
    leaveGroup: Group = null;
    leaveGroupDialog = false;
    newGroupName = '';
    newManager = '';
    searchGroup = '';
    searchGroupSuggestion = '';
    searchGroupTimeout = 0;
    searchUsername = '';
    searchUsernameSuggestion = '';
    searchUsernameTimeout = 0;
    successMessage = '';
    xferGroup: Group = null;
    xferGroupDialog = false;

    describeMembers(group: Group) {
        let memberNames = [] as string[];
        for (var i = 0; i < group.members.length; i++) {
            if (group.members[i] !== this.$store.state.userState.username
                && group.members[i] !== group.manager) {
                memberNames.push(group.members[i]);
            }
        }
        let desc = '';
        if (group.manager !== this.$store.state.userState.username) {
            desc += `Manager: ${group.manager}`;
        }
        if (memberNames.length > 0) {
            if (desc.length > 0) {
                desc += ", Other ";
            }
            desc += "Members: "
            desc += memberNames.join(", ");
        }
        return desc;
    }

    onContactGroupMember(member: string) {
        this.$store.commit(Store.startChatWithUser, member);
        this.$store.dispatch(Store.refreshChat, this.$route.fullPath);
    }

    onCreateGroup() {
        if (this.newGroupName) {
            let re = /^[\w.@-]+$/;
            if (re.test(this.newGroupName)) {
                this.activity = true;
                this.errorMessage = '';
                this.createErrorMessage = '';
                this.successMessage = '';
                fetch(`/api/Group/StartNewGroup/${this.newGroupName}`,
                    {
                        method: 'POST',
                        headers: {
                            'Accept': `application/json;v=${this.$store.state.apiVer}`,
                            'Accept-Language': this.$store.state.userState.culture,
                            'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                        }
                    })
                    .then(response => checkResponse(response, this.$route.fullPath))
                    .then(response => {
                        if (!response.ok) {
                            throw new Error(`CODE:${response.statusText}`);
                        } else {
                            this.refreshGroups();
                        }
                        this.createGroupDialog = false;
                        this.activity = false;
                    })
                    .catch(error => {
                        this.createErrorMessage = 'A problem occurred.';
                        if (error && error.message && error.message.startsWith("CODE:")) {
                            this.errorMessage += error.message.replace('CODE:', '');
                        }
                        this.activity = false;
                        ErrorMsg.logError('group/manage.onCreateGroup', error);
                    });
            }
        }
    }

    onDeleteGroup() {
        this.activity = true;
        this.errorMessage = '';
        this.successMessage = '';
        this.deleteGroupDialog = false;
        fetch(`/api/Group/RemoveGroup/${this.deleteGroup.name}`,
            {
                method: 'POST',
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                } else {
                    this.refreshGroups();
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = 'A problem occurred.';
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.errorMessage += error.message.replace('CODE:', '');
                }
                this.activity = false;
                ErrorMsg.logError('group/manage.onDeleteGroup', error);
            });
    }

    onDeleteGroupConfirm(group: Group) {
        this.deleteGroup = group;
        this.deleteGroupDialog = true;
    }

    onGroupChat(group: Group) {
        this.$store.commit(Store.startChatWithGroup, group.name);
        this.$store.dispatch(Store.refreshChat, this.$route.fullPath);
    }

    onGroupSearch() {
        this.activity = true;
        this.errorMessage = '';
        this.successMessage = '';
        fetch(`/api/Group/GetGroup/${this.searchGroup}`,
            {
                method: 'POST',
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    if (response.status === 404) {
                        throw new Error('404');
                    } else {
                        throw new Error(`CODE:${response.statusText}`);
                    }
                }
                return response;
            })
            .then(response => response.json() as Promise<Group>)
            .then(data => {
                this.foundGroup = data;
                this.activity = false;
            })
            .catch(error => {
                if (error && error.message && error.message === '404') {
                    this.errorMessage = 'No results.';
                } else {
                    this.errorMessage = 'A problem occurred. ';
                    if (error && error.message && error.message.startsWith("CODE:")) {
                        this.errorMessage += error.message.replace('CODE:', '');
                    }
                }
                this.activity = false;
                ErrorMsg.logError('group/manage.onGroupSearch', error);
            });
    }

    onInvite() {
        this.inviteDialog = false;
        fetch(`/api/Group/InviteUserToGroup/${this.searchUsername}/${this.inviteGroup.name}`,
            {
                method: 'POST',
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                } else {
                    this.successMessage = 'Invitation sent!';
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = 'A problem occurred.';
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.errorMessage += error.message.replace('CODE:', '');
                }
                this.activity = false;
                ErrorMsg.logError('group/manage.onInvite', error);
            });
    }

    onInviteConfirm(group: Group) {
        this.inviteGroup = group;
        this.inviteDialog = true;
    }

    onLeaveGroup() {
        this.activity = true;
        this.errorMessage = '';
        this.successMessage = '';
        this.leaveGroupDialog = false;
        fetch(`/api/Group/LeaveGroup/${this.leaveGroup.name}`,
            {
                method: 'POST',
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                } else {
                    this.refreshGroups();
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = 'A problem occurred.';
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.errorMessage += error.message.replace('CODE:', '');
                }
                this.activity = false;
                ErrorMsg.logError('group/manage.onLeaveGroup', error);
            });
    }

    onLeaveGroupConfirm(group: Group) {
        this.leaveGroup = group;
        this.leaveGroupDialog = true;
    }

    onRemoveGroupMember(group: Group, member: string) {
        this.activity = true;
        this.errorMessage = '';
        this.successMessage = '';
        fetch(`/api/Group/RemoveUserFromGroup/${member}/${group.name}`,
            {
                method: 'POST',
                headers: {
                    'Accept': `application/json;v=${this.$store.state.apiVer}`,
                    'Accept-Language': this.$store.state.userState.culture,
                    'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => {
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                } else {
                    this.refreshGroups();
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = 'A problem occurred.';
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.errorMessage += error.message.replace('CODE:', '');
                }
                this.activity = false;
                ErrorMsg.logError('group/manage.onRemoveGroupMember', error);
            });
    }

    onSearchGroupChange(val: string, oldVal: string) {
        if (this.searchGroupTimeout === 0) {
            this.searchGroupTimeout = setTimeout(this.suggestSearchGroup, 500);
        }
    }

    onSearchGroupKeypress(event: KeyboardEvent) {
        if (event.key === "Enter") {
            this.onGroupSearch();
        }
    }

    onSearchUsernameChange(val: string, oldVal: string) {
        if (this.searchUsernameTimeout === 0) {
            this.searchUsernameTimeout = setTimeout(this.suggestSearchUsername, 500);
        }
    }

    onXferGroup() {
        this.activity = true;
        this.errorMessage = '';
        this.successMessage = '';
        this.xferGroupDialog = false;
        if (this.xferGroup.name === 'Admin') {
            fetch(`/api/Group/TransferSiteAdminToUser/${this.newManager}`,
                {
                    method: 'POST',
                    headers: {
                        'Accept': `application/json;v=${this.$store.state.apiVer}`,
                        'Accept-Language': this.$store.state.userState.culture,
                        'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                    }
                })
                .then(response => checkResponse(response, this.$route.fullPath))
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`CODE:${response.statusText}`);
                    } else {
                        this.$store.state.userState.isSiteAdmin = false;
                        this.refreshGroups();
                    }
                    this.activity = false;
                })
                .catch(error => {
                    this.errorMessage = 'A problem occurred.';
                    if (error && error.message && error.message.startsWith("CODE:")) {
                        this.errorMessage += error.message.replace('CODE:', '');
                    }
                    this.activity = false;
                    ErrorMsg.logError('group/manage.onXferGroup', error);
                });
        } else {
            fetch(`/api/Group/TransferManagerToUser/${this.newManager}/${this.xferGroup.name}`,
                {
                    method: 'POST',
                    headers: {
                        'Accept': `application/json;v=${this.$store.state.apiVer}`,
                        'Accept-Language': this.$store.state.userState.culture,
                        'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                    }
                })
                .then(response => checkResponse(response, this.$route.fullPath))
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`CODE:${response.statusText}`);
                    } else {
                        this.refreshGroups();
                    }
                    this.activity = false;
                })
                .catch(error => {
                    this.errorMessage = 'A problem occurred.';
                    if (error && error.message && error.message.startsWith("CODE:")) {
                        this.errorMessage += error.message.replace('CODE:', '');
                    }
                    this.activity = false;
                    ErrorMsg.logError('group/manage.onXferGroup', error);
                });
        }
    }

    onXferGroupConfirm(group: Group) {
        this.xferGroup = group;
        this.xferGroupDialog = true;
    }

    refreshGroups() {
        this.$store.dispatch(Store.refreshGroups, this.$route.fullPath);
    }

    suggestSearchGroup() {
        this.searchGroupTimeout = 0;
        if (this.searchGroup) {
            fetch(`/api/Share/GetShareableGroupCompletion/${this.searchGroup}`,
                {
                    method: 'GET',
                    headers: {
                        'Accept': `application/json;v=${this.$store.state.apiVer}`,
                        'Accept-Language': this.$store.state.userState.culture,
                        'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                    }
                })
                .then(response => checkResponse(response, this.$route.fullPath))
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`CODE:${response.statusText}`);
                    } else {
                        this.searchGroupSuggestion = response.statusText;
                    }
                    this.activity = false;
                })
                .catch(error => {
                    ErrorMsg.logError('group/manage.suggestSearchGroup', error);
                });
        }
    }

    suggestSearchUsername() {
        this.searchUsernameTimeout = 0;
        if (this.searchUsername) {
            fetch(`/api/Share/GetShareableUsernameCompletion/${this.searchUsername}`,
                {
                    method: 'GET',
                    headers: {
                        'Accept': `application/json;v=${this.$store.state.apiVer}`,
                        'Accept-Language': this.$store.state.userState.culture,
                        'Authorization': `bearer ${this.$store.state.userState.user.access_token}`
                    }
                })
                .then(response => checkResponse(response, this.$route.fullPath))
                .then(response => {
                    if (!response.ok) {
                        throw new Error(`CODE:${response.statusText}`);
                    } else {
                        this.searchUsernameSuggestion = response.statusText;
                    }
                    this.activity = false;
                })
                .catch(error => {
                    ErrorMsg.logError('group/manage.suggestSearchUsername', error);
                });
        }
    }

    validateGroupName() {
        if (!this.newGroupName) {
            return "A name is required";
        } else {
            let re = /^[\w.@-]+$/;
            if (!re.test(this.newGroupName)) {
                return "Group names can contain only letters, numbers, underscores, hyphens, and periods";
            }
        }
        return true;
    }
}