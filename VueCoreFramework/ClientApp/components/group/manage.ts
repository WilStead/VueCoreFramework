import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as Store from '../../store/store';
import { checkResponse, ApiResponseViewModel } from '../../router';
import * as ErrorMsg from '../../error-msg';

interface Group {
    name: string;
    manager: string;
    members: string[];
}

@Component
export default class ManageGroupComponent extends Vue {
    activity = false;
    createErrorMessage = '';
    createGroupDialog = false;
    errorMessage = '';
    joinedGroups: Group[] = [];
    leaveGroupDialog = false;
    managedGroups: Group[] = [];
    newGroupName = '';

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

    onCreateGroup() {
        if (this.newGroupName) {
            let re = /^[\w.@-]+&/;
            if (re.test(this.newGroupName)) {
                this.activity = true;
                this.errorMessage = '';
                this.createErrorMessage = '';
                fetch(`/api/Authorization/StartNewGroup/${this.newGroupName}`,
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

    onLeaveGroup(group: Group) {
        this.activity = true;
        this.errorMessage = '';
        this.leaveGroupDialog = false;
        fetch(`/api/Authorization/LeaveGroup/${group.name}`,
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

    onRemoveGroupMember(group: Group, member: string) {
        this.activity = true;
        this.errorMessage = '';
        fetch(`/api/Authorization/RemoveUserFromGroup/${member.username}/${group.name}`,
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

    refreshGroups() {
        this.activity = true;
        this.errorMessage = '';
        fetch('/api/Authorization/GetGroupMemberships/',
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
                        if (data[i].manager === this.$store.state.userState.username) {
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