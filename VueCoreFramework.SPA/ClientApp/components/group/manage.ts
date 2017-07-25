import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as Api from '../../api';
import * as Store from '../../store/store';
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

    async onCreateGroup() {
        if (!this.newGroupName) {
            return;
        }

        let re = /^[\w.@-]+$/;
        if (!re.test(this.newGroupName)) {
            return;
        }

        this.activity = true;
        this.errorMessage = '';
        this.createErrorMessage = '';
        this.successMessage = '';
        try {
            let response = await Api.postApi(`api/Group/StartNewGroup/${this.newGroupName}`, this.$route.fullPath);
            if (!response.ok) {
                throw new Error(`CODE:${response.statusText}`);
            } else {
                this.refreshGroups();
            }
            this.createGroupDialog = false;
        } catch (error) {
            ErrorMsg.logError('group/manage.onCreateGroup', error);
            this.createErrorMessage = 'A problem occurred.';
            if (error && error.message && error.message.startsWith("CODE:")) {
                this.errorMessage += error.message.replace('CODE:', '');
            }
        }
        this.activity = false;
    }

    async onDeleteGroup() {
        this.activity = true;
        this.errorMessage = '';
        this.successMessage = '';
        this.deleteGroupDialog = false;
        try {
            let response = await Api.postApi(`api/Group/RemoveGroup/${this.deleteGroup.name}`, this.$route.fullPath);
            if (!response.ok) {
                throw new Error(`CODE:${response.statusText}`);
            } else {
                this.refreshGroups();
            }
        } catch (error) {
            ErrorMsg.logError('group/manage.onDeleteGroup', error);
            this.errorMessage = 'A problem occurred.';
            if (error && error.message && error.message.startsWith("CODE:")) {
                this.errorMessage += error.message.replace('CODE:', '');
            }
        }
        this.activity = false;
    }

    onDeleteGroupConfirm(group: Group) {
        this.deleteGroup = group;
        this.deleteGroupDialog = true;
    }

    onGroupChat(group: Group) {
        this.$store.commit(Store.startChatWithGroup, group.name);
        this.$store.dispatch(Store.refreshChat, this.$route.fullPath);
    }

    async onGroupSearch() {
        this.activity = true;
        this.errorMessage = '';
        this.successMessage = '';
        try {
            let response = await Api.postApi(`api/Group/GetGroup/${this.searchGroup}`, this.$route.fullPath);
            if (!response.ok) {
                if (response.status === 404) {
                    throw new Error('404');
                } else {
                    throw new Error(`CODE:${response.statusText}`);
                }
            }
            this.foundGroup = await response.json() as Group;
        } catch (error) {
            ErrorMsg.logError('group/manage.onGroupSearch', error);
            if (error && error.message && error.message === '404') {
                this.errorMessage = 'No results.';
            } else {
                this.errorMessage = 'A problem occurred. ';
                if (error && error.message && error.message.startsWith("CODE:")) {
                    this.errorMessage += error.message.replace('CODE:', '');
                }
            }
        }
        this.activity = false;
    }

    async onInvite() {
        this.inviteDialog = false;
        try {
            let response = await Api.postApi(`api/Group/InviteUserToGroup/${this.searchUsername}/${this.inviteGroup.name}`, this.$route.fullPath);
            if (!response.ok) {
                throw new Error(`CODE:${response.statusText}`);
            } else {
                this.successMessage = 'Invitation sent!';
            }
        } catch (error) {
            ErrorMsg.logError('group/manage.onInvite', error);
            this.errorMessage = 'A problem occurred.';
            if (error && error.message && error.message.startsWith("CODE:")) {
                this.errorMessage += error.message.replace('CODE:', '');
            }
        }
        this.activity = false;
    }

    onInviteConfirm(group: Group) {
        this.inviteGroup = group;
        this.inviteDialog = true;
    }

    async onLeaveGroup() {
        this.activity = true;
        this.errorMessage = '';
        this.successMessage = '';
        this.leaveGroupDialog = false;
        try {
            let response = await Api.postApi(`api/Group/LeaveGroup/${this.leaveGroup.name}`, this.$route.fullPath);
            if (!response.ok) {
                throw new Error(`CODE:${response.statusText}`);
            } else {
                this.refreshGroups();
            }
        } catch (error) {
            ErrorMsg.logError('group/manage.onLeaveGroup', error);
            this.errorMessage = 'A problem occurred.';
            if (error && error.message && error.message.startsWith("CODE:")) {
                this.errorMessage += error.message.replace('CODE:', '');
            }
        }
        this.activity = false;
    }

    onLeaveGroupConfirm(group: Group) {
        this.leaveGroup = group;
        this.leaveGroupDialog = true;
    }

    async onRemoveGroupMember(group: Group, member: string) {
        this.activity = true;
        this.errorMessage = '';
        this.successMessage = '';
        try {
            let response = await Api.postApi(`api/Group/RemoveUserFromGroup/${member}/${group.name}`, this.$route.fullPath);
            if (!response.ok) {
                throw new Error(`CODE:${response.statusText}`);
            } else {
                this.refreshGroups();
            }
        } catch (error) {
            ErrorMsg.logError('group/manage.onRemoveGroupMember', error);
            this.errorMessage = 'A problem occurred.';
            if (error && error.message && error.message.startsWith("CODE:")) {
                this.errorMessage += error.message.replace('CODE:', '');
            }
        }
        this.activity = false;
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

    async onXferGroup() {
        this.activity = true;
        this.errorMessage = '';
        this.successMessage = '';
        this.xferGroupDialog = false;
        try {
            if (this.xferGroup.name === 'Admin') {
                let response = await Api.postApi(`api/Group/TransferSiteAdminToUser/${this.newManager}`, this.$route.fullPath);
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                } else {
                    this.$store.state.userState.isSiteAdmin = false;
                }
            } else {
                let response = await Api.postApi(`api/Group/TransferManagerToUser/${this.newManager}/${this.xferGroup.name}`, this.$route.fullPath);
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                }
            }
            this.refreshGroups();
        } catch (error) {
            ErrorMsg.logError('group/manage.onXferGroup', error);
            this.errorMessage = 'A problem occurred.';
            if (error && error.message && error.message.startsWith("CODE:")) {
                this.errorMessage += error.message.replace('CODE:', '');
            }
        }
        this.activity = false;
    }

    onXferGroupConfirm(group: Group) {
        this.xferGroup = group;
        this.xferGroupDialog = true;
    }

    refreshGroups() {
        this.$store.dispatch(Store.refreshGroups, this.$route.fullPath);
    }

    async suggestSearchGroup() {
        this.searchGroupTimeout = 0;
        if (this.searchGroup) {
            try {
                let response = await Api.getApi(`api/Share/GetShareableGroupCompletion/${this.searchGroup}`, this.$route.fullPath);
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                } else {
                    this.searchGroupSuggestion = response.statusText;
                }
            } catch (error) {
                ErrorMsg.logError('group/manage.suggestSearchGroup', error);
            }
        }
    }

    async suggestSearchUsername() {
        this.searchUsernameTimeout = 0;
        if (this.searchUsername) {
            try {
                let response = await Api.getApi(`api/Share/GetShareableUsernameCompletion/${this.searchUsername}`, this.$route.fullPath);
                if (!response.ok) {
                    throw new Error(`CODE:${response.statusText}`);
                } else {
                    this.searchUsernameSuggestion = response.statusText;
                }
            } catch (error) {
                ErrorMsg.logError('group/manage.suggestSearchUsername', error);
            }
            this.activity = false;
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