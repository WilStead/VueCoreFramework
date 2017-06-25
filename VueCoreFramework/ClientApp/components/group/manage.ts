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
    joinedGroups: Group[] = [];
    leaveGroup: Group = null;
    leaveGroupDialog = false;
    managedGroups: Group[] = [];
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
        // TODO: open chat system targeted at the user
    }

    onCreateGroup() {
        if (this.newGroupName) {
            let re = /^[\w.@-]+&/;
            if (re.test(this.newGroupName)) {
                this.activity = true;
                this.errorMessage = '';
                this.createErrorMessage = '';
                this.successMessage = '';
                fetch(`/api/Group/StartNewGroup/${this.newGroupName}`,
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
                            this.createErrorMessage = data.error;
                        } else {
                            this.refreshGroups();
                        }
                        this.createGroupDialog = false;
                        this.activity = false;
                    })
                    .catch(error => {
                        this.createErrorMessage = 'A problem occurred.';
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
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                } else {
                    this.refreshGroups();
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = 'A problem occurred.';
                this.activity = false;
                ErrorMsg.logError('group/manage.onDeleteGroup', error);
            });
    }

    onDeleteGroupConfirm(group: Group) {
        this.deleteGroup = group;
        this.deleteGroupDialog = true;
    }

    onGroupSearch() {
        this.activity = true;
        this.errorMessage = '';
        this.successMessage = '';
        this.leaveGroupDialog = false;
        fetch(`/api/Group/GetGroup/${this.searchGroup}`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<Group>)
            .then(data => {
                if (data['error']) {
                    this.errorMessage = data['error'];
                } else if (data['response']) {
                    this.errorMessage = data['response'];
                } else {
                    this.foundGroup = data;
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = 'A problem occurred.';
                this.activity = false;
                ErrorMsg.logError('group/manage.onGroupSearch', error);
            });
    }

    onInvite() {
        this.inviteDialog = false;
        fetch(`/api/Group/InviteUserToGroup/${this.searchUsername}/${this.inviteGroup}`,
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
                    this.errorMessage = data.error;
                } else {
                    this.successMessage = 'Invitation sent!';
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = 'A problem occurred.';
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
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                } else {
                    this.refreshGroups();
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = 'A problem occurred.';
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
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                } else {
                    this.refreshGroups();
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = 'A problem occurred.';
                this.activity = false;
                ErrorMsg.logError('group/manage.onRemoveGroupMember', error);
            });
    }

    onSearchGroupChange(val: string, oldVal: string) {
        if (this.searchGroupTimeout === 0) {
            this.searchGroupTimeout = setTimeout(this.suggestSearchGroup, 500);
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
                        'Accept': 'application/json',
                        'Authorization': `bearer ${this.$store.state.userState.token}`
                    }
                })
                .then(response => checkResponse(response, this.$route.fullPath))
                .then(response => response.json() as Promise<ApiResponseViewModel>)
                .then(data => {
                    if (data.error) {
                        this.errorMessage = data.error;
                    } else {
                        this.$store.state.userState.isSiteAdmin = false;
                        this.refreshGroups();
                    }
                    this.activity = false;
                })
                .catch(error => {
                    this.errorMessage = 'A problem occurred.';
                    this.activity = false;
                    ErrorMsg.logError('group/manage.onXferGroup', error);
                });
        } else {
            fetch(`/api/Group/TransferManagerToUser/${this.newManager}/${this.xferGroup.name}`,
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
                        this.errorMessage = data.error;
                    } else {
                        this.refreshGroups();
                    }
                    this.activity = false;
                })
                .catch(error => {
                    this.errorMessage = 'A problem occurred.';
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
        this.activity = true;
        this.errorMessage = '';
        fetch('/api/Group/GetGroupMemberships/',
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${this.$store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, this.$route.fullPath))
            .then(response => response.json() as Promise<Group[]>)
            .then(data => {
                if (data['error']) {
                    this.errorMessage = data['error'];
                } else {
                    this.managedGroups = [];
                    this.joinedGroups = [];
                    for (var i = 0; i < data.length; i++) {
                        if (data[i].manager === this.$store.state.userState.username
                            || data[i].name === 'Admin' && this.$store.state.userState.isSiteAdmin) {
                            this.managedGroups.push(data[i]);
                        } else {
                            this.joinedGroups.push(data[i]);
                        }
                    }
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = 'A problem occurred.';
                this.activity = false;
                ErrorMsg.logError('group/manage.refreshGroups', error);
            });
    }

    suggestSearchGroup() {
        this.searchGroupTimeout = 0;
        if (this.searchGroup) {
            fetch(`/api/Share/GetShareableGroupCompletion/${this.searchGroup}`,
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
                        throw new Error(`There was a problem retrieving a group suggestion: ${data['error']}`);
                    } else {
                        this.searchGroupSuggestion = data.response;
                    }
                })
                .catch(error => {
                    ErrorMsg.logError('group/manage.suggestSearchGroup', error);
                });
        }
    }

    suggestSearchUsername() {
        this.searchGroupTimeout = 0;
        if (this.searchGroup) {
            fetch(`/api/Share/GetShareableUsernameCompletion/${this.searchUsername}`,
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
                        throw new Error(`There was a problem retrieving a username suggestion: ${data['error']}`);
                    } else {
                        this.searchUsernameSuggestion = data.response;
                    }
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
            let re = /^[\w.@-]+&/;
            if (!re.test(this.newGroupName)) {
                return "Group names can contain only letters, numbers, underscores, hyphens, and periods";
            }
        }
        return true;
    }
}