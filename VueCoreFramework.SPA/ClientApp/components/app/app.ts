import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import * as Store from '../../store/store';
import { ConversationViewModel, MessageViewModel, messaging } from '../../store/messaging';
import { defaultCulture } from '../../globalization/globalization';
import { checkResponse, ApiResponseViewModel } from '../../router';
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
        TopbarComponent: require('../topbar/topbar.vue'),
        MenuItemComponent: require('../menu-item/menu-item.vue'),
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

    mounted() {
        let forwardUrl = document.getElementById("forward-url").getAttribute("data-forward-url");
        if (forwardUrl) {
            this.$router.push(forwardUrl);
        }

        this.$store.commit(Store.setCulture, defaultCulture);
        this.$store.commit(Store.addTypeRoutes, this.$router);

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

    onAdminChatProxy(interlocutor: string) {
        this.$store.commit(Store.startChatAdminReview, { proxySender: this.foundUser.username, interlocutor });
        this.$store.dispatch(Store.refreshChat, this.$route.fullPath)
            .then(() => {
                let chat = document.getElementById('chat-row');
                chat.scrollTop = chat.scrollHeight;
            });
    }

    onDeleteChat(interlocutor: string) {
        messaging.markConversationDeleted(this.$route.fullPath, interlocutor)
            .then(response => {
                this.refreshConversations();
            })
            .catch(error => {
                ErrorMsg.logError('app.onDeleteChat', error);
            });
    }

    onGroupChat(group: Group) {
        this.$store.commit(Store.startChatWithGroup, group.name);
        this.$store.dispatch(Store.refreshChat, this.$route.fullPath)
            .then(() => {
                let chat = document.getElementById('chat-row');
                chat.scrollTop = chat.scrollHeight;
            });
    }

    onHideChat() {
        this.$store.commit(Store.hideChat);
    }

    onLockAccount() {
        fetch(`/api/Manage/LockAccount/${this.foundUser.username}`,
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
                    if (response.statusText) {
                        ErrorMsg.showErrorMsg(response.statusText);
                    } else {
                        ErrorMsg.showErrorMsg("A problem occurred.");
                    }
                    throw new Error("CODE");
                } else {
                    this.foundUser.isLocked = true;
                }
            })
            .catch(error => {
                if (error !== "CODE") {
                    ErrorMsg.showErrorMsgAndLog('app.onLockAccount', "A problem occurred. The account was not locked.", error);
                }
            });
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

    onSystemChat() {
        this.$store.commit(Store.startChatWithSystem);
        this.$store.dispatch(Store.refreshChat, this.$route.fullPath)
            .then(() => {
                let chat = document.getElementById('chat-row');
                chat.scrollTop = chat.scrollHeight;
            });
    }

    onUnlockAccount() {
        fetch(`/api/Manage/UnlockAccount/${this.foundUser.username}`,
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
                    if (response.statusText) {
                        ErrorMsg.showErrorMsg(response.statusText);
                    } else {
                        ErrorMsg.showErrorMsg("A problem occurred.");
                    }
                    throw new Error("CODE");
                } else {
                    this.foundUser.isLocked = false;
                }
            })
            .catch(error => {
                if (error !== "CODE") {
                    ErrorMsg.showErrorMsgAndLog('app.onUnlockAccount', "A problem occurred. The account was not unlocked.", error);
                }
            });
    }

    onUserChat(interlocutor: string) {
        this.$store.commit(Store.startChatWithUser, interlocutor);
        this.$store.dispatch(Store.refreshChat, this.$route.fullPath)
            .then(() => {
                let chat = document.getElementById('chat-row');
                chat.scrollTop = chat.scrollHeight;
            });
    }

    onUsernameSearch() {
        this.chatErrorMessage = '';
        this.foundUser = null;
        if (this.searchUsername) {
            fetch(`/api/Account/VerifyUser/${this.searchUsername}`,
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
                        if (response.statusText) {
                            this.chatErrorMessage = response.statusText;
                        } else if (response.status !== 404) {
                            this.chatErrorMessage = "A problem occurred.";
                        }
                        throw new Error("CODE");
                    }
                    return response;
                })
                .then(response => response.json() as Promise<UserViewModel>)
                .then(data => {
                    this.foundUser = data;
                    messaging.getProxyConversations(this.$route.fullPath, this.foundUser.username)
                        .then(data => {
                            if (data['error']) {
                                throw new Error(data['error']);
                            } else {
                                this.foundUserConversations = data;
                            }
                        });
                })
                .catch(error => {
                    if (error !== "CODE") {
                        ErrorMsg.logError('app.onUsernameSearch', error);
                    }
                });
        }
    }

    refreshChat() {
        this.chatRefreshTimeout = 0;
        if (this.$store.state.uiState.messaging.messagingShown
            && this.$store.state.uiState.messaging.chatShown) {
            this.$store.dispatch(Store.refreshChat, this.$route.fullPath)
                .then(() => {
                    this.chatRefreshTimeout = setTimeout(this.refreshChat, 10000);
                })
                .catch(() => {
                    this.chatRefreshTimeout = setTimeout(this.refreshChat, 10000);
                });
        } else {
            this.chatRefreshTimeout = setTimeout(this.refreshChat, 10000);
        }
    }

    refreshConversations() {
        this.conversationRefreshTimeout = 0;
        if (this.$store.state.userState.user) {
            this.$store.dispatch(Store.refreshConversations, this.$route.fullPath)
                .then(() => {
                    this.conversationRefreshTimeout = setTimeout(this.refreshConversations, 10000);
                })
                .catch(() => {
                    this.conversationRefreshTimeout = setTimeout(this.refreshConversations, 10000);
                });
        } else {
            this.conversationRefreshTimeout = setTimeout(this.refreshConversations, 10000);
        }
    }

    refreshGroups() {
        this.groupRefreshTimeout = 0;
        if (this.$store.state.userState.user) {
            this.$store.dispatch(Store.refreshGroups, this.$route.fullPath)
                .then(() => {
                    this.groupRefreshTimeout = setTimeout(this.refreshGroups, 10000);
                })
                .catch(() => {
                    this.groupRefreshTimeout = setTimeout(this.refreshGroups, 10000);
                });
        } else {
            this.groupRefreshTimeout = setTimeout(this.refreshGroups, 10000);
        }
    }

    refreshSystemMessages() {
        this.systemMessageRefreshTimeout = 0;
        if (this.$store.state.userState.user) {
            this.$store.dispatch(Store.refreshSystemMessages, this.$route.fullPath)
                .then(() => {
                    this.systemMessageRefreshTimeout = setTimeout(this.refreshSystemMessages, 10000);
                })
                .catch(() => {
                    this.systemMessageRefreshTimeout = setTimeout(this.refreshSystemMessages, 10000);
                });
        } else {
            this.systemMessageRefreshTimeout = setTimeout(this.refreshSystemMessages, 10000);
        }
    }

    sendMessage() {
        this.chatErrorMessage = '';
        if (this.$store.state.uiState.messaging.groupChat) {
            messaging.sendMessageToGroup(this.$route.fullPath,
                this.$store.state.uiState.messaging.groupChat,
                this.messageText)
                .then(data => {
                    this.refreshChat();
                    this.messageText = '';
                    let chat = document.getElementById('chat-row');
                    chat.scrollTop = chat.scrollHeight;
                })
                .catch(error => {
                    if (error && error.message && error.message.startsWith('CODE')) {
                        this.chatErrorMessage = error.message.replace('CODE:', '');
                    } else {
                        ErrorMsg.logError('app.sendMessage', error);
                    }
                });
        } else {
            messaging.sendMessageToUser(this.$route.fullPath,
                this.$store.state.uiState.messaging.interlocutor,
                this.messageText)
                .then(data => {
                    this.refreshChat();
                    this.messageText = '';
                    let chat = document.getElementById('chat-row');
                    chat.scrollTop = chat.scrollHeight;
                })
                .catch(error => {
                    if (error && error.message && error.message.startsWith('CODE')) {
                        this.chatErrorMessage = error.message.replace('CODE:', '');
                    } else {
                        ErrorMsg.logError('app.sendMessage', error);
                    }
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
                })
                .catch(error => {
                    ErrorMsg.logError('app.suggestSearchUsername', error);
                });
        }
    }
}
