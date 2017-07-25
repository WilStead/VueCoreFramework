import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { authMgr, configureOidc } from '../../authorization';
import * as Api from '../../api';
import * as Store from '../../store/store';
import { ConversationViewModel, MessageViewModel, messaging } from '../../store/messaging';
import { defaultCulture } from '../../globalization/globalization';
import { Group } from '../group/manage';
import * as ErrorMsg from '../../error-msg';
import VueMarkdown from 'vue-markdown';
import * as moment from 'moment';

/**
 * Used to transfer information about a user.
 */
interface UserViewModel {
    /**
     * The user's email address
     */
    email: string;

    /**
     * Indicates whether the user's account has been locked by an admin.
     */
    isLocked: boolean;

    /**
     * The username of the user.
     */
    username: string;
}

@Component({
    components: {
        TopbarComponent: require('../topbar/topbar.vue').default,
        MenuItemComponent: require('../menu-item/menu-item.vue').default,
        VueMarkdown
    }
})
export default class AppComponent extends Vue {
    foundUser: UserViewModel = null;
    foundUserConversations: ConversationViewModel[] = [];
    messageText = '';
    sideNav = false;
    chatErrorMessage = '';
    chatRefreshTimeout = 0;
    conversationRefreshTimeout = 0;
    groupRefreshTimeout = 0;
    searchUsername = '';
    searchUsernameSuggestion = '';
    searchUsernameTimeout = 0;
    systemMessageRefreshTimeout = 0;

    get groups() {
        return this.$store.state.userState.managedGroups.concat(this.$store.state.userState.joinedGroups);
    }

    created() {
        configureOidc();

        this.$store.commit(Store.setCulture, defaultCulture);
        this.$store.commit(Store.addTypeRoutes, this.$router);
    }

    mounted() {
        let forwardUrl = document.getElementById("forward-url").getAttribute("data-forward-url");
        if (forwardUrl) {
            this.$router.push(forwardUrl);
        }

        if (this.groupRefreshTimeout === 0) {
            this.groupRefreshTimeout = setTimeout(this.refreshGroups, 100);
        }

        if (this.systemMessageRefreshTimeout === 0) {
            this.systemMessageRefreshTimeout = setTimeout(this.refreshSystemMessages, 200);
        }

        if (this.conversationRefreshTimeout === 0) {
            this.conversationRefreshTimeout = setTimeout(this.refreshConversations, 300);
        }

        if (this.chatRefreshTimeout === 0) {
            this.chatRefreshTimeout = setTimeout(this.refreshChat, 400);
        }
    }

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

    formatTimestamp(timestamp: string) {
        return moment(timestamp).format('M/D LTS');
    }

    getMessageClass(message: MessageViewModel) {
        if (message.username === this.$store.state.userState.username) {
            return { 'blue--text': true };
        } else if (message.username === this.$store.state.uiState.messaging.proxySender) {
            return { 'blue--text': true, 'text--lighten-2': true };
        } else if (message.isUserSiteAdmin) {
            return { 'yellow--text': true, 'text--accent-4': true };
        } else if (message.isUserAdmin) {
            return { 'deep-orange--text': true, 'text--accent-3': true };
        } else if (this.$store.state.uiState.messaging.interlocutor) {
            return { 'light-green--text': true, 'text--accent-4': true };
        } else {
            switch (message.username.toLowerCase().charAt(0)) {
                case 'a': return { 'light-green--text': true, 'text--lighten-2': true };
                case 'b': return { 'lime--text': true, 'text--darken-4': true };
                case 'c': return { 'brown--text': true };
                case 'd': return { 'blue-grey--text': true };
                case 'e': return { 'red--text': true, 'text--accent-2': true };
                case 'f': return { 'pink--text': true, 'text--lighten-1': true };
                case 'g': return { 'purple--text': true, 'text--lighten-2': true };
                case 'h': return { 'purple--text': true, 'text--darken-3': true };
                case 'i': return { 'purple--text': true, 'text--darken-4': true };
                case 'j': return { 'deep-purple--text': true, 'text--darken-4': true };
                case 'k': return { 'deep-purple--text': true, 'text--accent-3': true };
                case 'l': return { 'indigo--text': true, 'text--lighten-1': true };
                case 'm': return { 'cyan--text': true };
                case 'n': return { 'cyan--text': true, 'text--darken-4': true };
                case 'o': return { 'teal--text': true };
                case 'p': return { 'teal--text': true, 'text--darken-4': true };
                case 'q': return { 'brown--text': true, 'text--lighten-1': true };
                case 'r': return { 'brown--text': true, 'text--lighten-2': true };
                case 's': return { 'blue-grey--text': true, 'text--darken-4': true };
                case 't': return { 'grey--text': true, 'text--darken-4': true };
                case 'u': return { 'lime--text': true, 'text--darken-3': true };
                case 'v': return { 'pink--text': true, 'text--darken-2': true };
                case 'w': return { 'purple--text': true, 'text--lighten-1': true };
                case 'x': return { 'deep-purple--text': true, 'text--darken-3': true };
                case 'y': return { 'indigo--text': true, 'text--darken-2': true };
                case 'z': return { 'brown--text': true, 'text--darken-4': true };
                default: return { 'purple--text': true, 'text--accent-1': true };
            }
        }
    }

    async onAdminChatProxy(interlocutor: string) {
        this.$store.commit(Store.startChatAdminReview, { proxySender: this.foundUser.username, interlocutor });
        await this.$store.dispatch(Store.refreshChat, this.$route.fullPath);
        let chat = document.getElementById('chat-row');
        chat.scrollTop = chat.scrollHeight;
    }

    async onDeleteChat(interlocutor: string) {
        try {
            await messaging.markConversationDeleted(this.$route.fullPath, interlocutor);
            this.refreshConversations();
        } catch (error) {
            ErrorMsg.logError('app.onDeleteChat', error);
        }
    }

    async onGroupChat(group: Group) {
        this.$store.commit(Store.startChatWithGroup, group.name);
        await this.$store.dispatch(Store.refreshChat, this.$route.fullPath);
        let chat = document.getElementById('chat-row');
        chat.scrollTop = chat.scrollHeight;
    }

    onHideChat() {
        this.$store.commit(Store.hideChat);
    }

    async onLockAccount() {
        try {
            let response = await Api.postAuth(`Manage/LockAccount/${this.foundUser.username}`, this.$route.fullPath);
            if (!response.ok) {
                if (response.statusText) {
                    ErrorMsg.showErrorMsg(response.statusText);
                } else {
                    ErrorMsg.showErrorMsg("A problem occurred.");
                }
                throw new Error("CODE");
            } else {
                this.foundUser.isLocked = true;
            }
        } catch (error) {
            if (error !== "CODE") {
                ErrorMsg.showErrorMsgAndLog('app.onLockAccount', "A problem occurred. The account was not locked.", error);
            }
        }
    }

    onMessageTextKeypress(event: KeyboardEvent) {
        if (event.key === "Enter") {
            this.sendMessage();
        }
    }

    onSearchUsernameChange(val: string, oldVal: string) {
        if (this.searchUsernameTimeout === 0) {
            this.searchUsernameTimeout = setTimeout(this.suggestSearchUsername, 500);
        }
    }

    onSearchUsernameKeypress(event: KeyboardEvent) {
        if (event.key === "Enter") {
            this.onUsernameSearch();
        }
    }

    async onSystemChat() {
        this.$store.commit(Store.startChatWithSystem);
        await this.$store.dispatch(Store.refreshChat, this.$route.fullPath);
        let chat = document.getElementById('chat-row');
        chat.scrollTop = chat.scrollHeight;
    }

    async onUnlockAccount() {
        try {
            let response = await Api.postAuth(`Manage/UnlockAccount/${this.foundUser.username}`, this.$route.fullPath);
            if (!response.ok) {
                if (response.statusText) {
                    ErrorMsg.showErrorMsg(response.statusText);
                } else {
                    ErrorMsg.showErrorMsg("A problem occurred.");
                }
                throw new Error("CODE");
            } else {
                this.foundUser.isLocked = false;
            }
        } catch (error) {
            if (error !== "CODE") {
                ErrorMsg.showErrorMsgAndLog('app.onUnlockAccount', "A problem occurred. The account was not unlocked.", error);
            }
        }
    }

    async onUserChat(interlocutor: string) {
        this.$store.commit(Store.startChatWithUser, interlocutor);
        await this.$store.dispatch(Store.refreshChat, this.$route.fullPath);
        let chat = document.getElementById('chat-row');
        chat.scrollTop = chat.scrollHeight;
    }

    async onUsernameSearch() {
        this.chatErrorMessage = '';
        this.foundUser = null;
        if (!this.searchUsername) {
            return;
        }
        try {
            let response = await Api.postAuth(`Account/VerifyUser/${this.searchUsername}`, this.$route.fullPath);
            if (!response.ok) {
                if (response.statusText) {
                    this.chatErrorMessage = response.statusText;
                } else if (response.status !== 404) {
                    this.chatErrorMessage = "A problem occurred.";
                }
                throw new Error("CODE");
            }
            this.foundUser = await response.json() as UserViewModel;
            this.foundUserConversations = await messaging.getProxyConversations(this.$route.fullPath, this.foundUser.username);
        } catch (error) {
            if (error !== "CODE") {
                ErrorMsg.logError('app.onUsernameSearch', error);
            }
        }
    }

    async refreshChat() {
        this.chatRefreshTimeout = 0;
        if (this.$store.state.uiState.messaging.messagingShown
            && this.$store.state.uiState.messaging.chatShown) {
            try {
                await this.$store.dispatch(Store.refreshChat, this.$route.fullPath);
            } catch (error) { }
        }
        this.chatRefreshTimeout = setTimeout(this.refreshChat, 10000);
    }

    async refreshConversations() {
        this.conversationRefreshTimeout = 0;
        if (this.$store.state.userState.user) {
            try {
                await this.$store.dispatch(Store.refreshConversations, this.$route.fullPath);
            } catch (error) { }
        }
        this.conversationRefreshTimeout = setTimeout(this.refreshConversations, 10000);
    }

    async refreshGroups() {
        this.groupRefreshTimeout = 0;
        if (this.$store.state.userState.user) {
            try {
                await this.$store.dispatch(Store.refreshGroups, this.$route.fullPath);
            } catch (error) { }
        }
        this.groupRefreshTimeout = setTimeout(this.refreshGroups, 10000);
    }

    async refreshSystemMessages() {
        this.systemMessageRefreshTimeout = 0;
        if (this.$store.state.userState.user) {
            try {
                await this.$store.dispatch(Store.refreshSystemMessages, this.$route.fullPath);
            } catch (error) { }
        }
        this.systemMessageRefreshTimeout = setTimeout(this.refreshSystemMessages, 10000);
    }

    async sendMessage() {
        this.chatErrorMessage = '';
        try {
            if (this.$store.state.uiState.messaging.groupChat) {
                await messaging.sendMessageToGroup(this.$route.fullPath,
                    this.$store.state.uiState.messaging.groupChat,
                    this.messageText);
                this.messageText = '';
                await this.refreshChat();
                let chat = document.getElementById('chat-row');
                chat.scrollTop = chat.scrollHeight;
            } else {
                await messaging.sendMessageToUser(this.$route.fullPath,
                    this.$store.state.uiState.messaging.interlocutor,
                    this.messageText);
                this.messageText = '';
                await this.refreshChat();
                let chat = document.getElementById('chat-row');
                chat.scrollTop = chat.scrollHeight;
            }
        } catch (error) {
            if (error && error.message && error.message.startsWith('CODE')) {
                this.chatErrorMessage = error.message.replace('CODE:', '');
            } else {
                ErrorMsg.logError('app.sendMessage', error);
            }
        }
    }

    async suggestSearchUsername() {
        this.searchUsernameTimeout = 0;
        if (!this.searchUsername) {
            return;
        }
        try {
            let response = await Api.getApi(`api/Share/GetShareableUsernameCompletion/${this.searchUsername}`, this.$route.fullPath);
            if (!response.ok) {
                throw new Error(`CODE:${response.statusText}`);
            } else {
                this.searchUsernameSuggestion = response.statusText;
            }
        } catch (error) {
            ErrorMsg.logError('app.suggestSearchUsername', error);
        }
    }
}
