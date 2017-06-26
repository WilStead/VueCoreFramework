import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import * as Store from '../../store/store';
import { ConversationViewModel, MessageViewModel, messaging } from '../../store/messaging';
import { checkResponse } from '../../router';
import { Group } from '../group/manage';
import * as ErrorMsg from '../../error-msg';
import VueMarkdown from 'vue-markdown';

@Component({
    components: {
        TopbarComponent: require('../topbar/topbar.vue'),
        MenuItemComponent: require('../menu-item/menu-item.vue'),
        VueMarkdown
    }
})
export default class AppComponent extends Vue {
    conversations: ConversationViewModel[] = [];
    groups: Group[] = [];
    messageText = '';
    sideNav = false;
    chatErrorMessage = '';
    chatRefreshTimeout = 0;
    conversationRefreshTimeout = 0;
    groupRefreshTimeout = 0;
    systemMessageRefreshTimeout = 0;
    systemMessages = false;

    mounted() {
        let forwardUrl = document.getElementById("forward-url").getAttribute("data-forward-url");
        if (forwardUrl) {
            this.$router.push(forwardUrl);
        }

        this.$store.commit(Store.addTypeRoutes, this.$router);
        
        if (this.groupRefreshTimeout === 0) {
            this.groupRefreshTimeout = setTimeout(this.refreshGroups, 10000);
        }

        if (this.conversationRefreshTimeout === 0) {
            this.conversationRefreshTimeout = setTimeout(this.refreshConversations, 10000);
        }

        if (this.systemMessageRefreshTimeout === 0) {
            this.systemMessageRefreshTimeout = setTimeout(this.refreshSystemMessages, 10000);
        }

        if (this.chatRefreshTimeout === 0) {
            this.chatRefreshTimeout = setTimeout(this.refreshChat, 10000);
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

    getMessageClass(message: MessageViewModel) {
        if (message.username === this.$store.state.userState.username) {
            return { 'blue--text': true };
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

    onDeleteChat(interlocutor: string) {
        messaging.markConversationDeleted(this.$route.fullPath, interlocutor)
            .then(data => {
                if (data['error']) {
                    throw new Error(data['error']);
                } else {
                    this.refreshConversations();
                }
            })
            .catch(error => {
                ErrorMsg.logError('app.onDeleteChat', error);
            });
    }

    onGroupChat(group: Group) {
        this.$store.commit(Store.startChatWithGroup, group);
        this.$store.commit(Store.refreshChat, this.$route.fullPath);
    }

    onHideChat() {
        this.$store.commit(Store.hideChat);
    }

    onSystemChat() {
        this.$store.commit(Store.startChatWithSystem);
        this.$store.commit(Store.refreshChat, this.$route.fullPath);
    }

    onUserChat(interlocutor: string) {
        this.$store.commit(Store.startChatWithUser, interlocutor);
        this.$store.commit(Store.refreshChat, this.$route.fullPath);
    }

    refreshChat() {
        this.chatRefreshTimeout = 0;
        if (this.$store.state.uiState.messaging.messagingShown
            && this.$store.state.uiState.messaging.chatShown) {
            this.$store.commit(Store.refreshChat, this.$route.fullPath);
        }
        this.chatRefreshTimeout = setTimeout(this.refreshChat, 10000);
    }

    refreshConversations() {
        this.conversationRefreshTimeout = 0;
        messaging.getConversations(this.$route.fullPath)
            .then(data => {
                if (data['error']) {
                    throw new Error(data['error']);
                } else {
                    this.conversations = data;
                }
                this.conversationRefreshTimeout = setTimeout(this.refreshConversations, 10000);
            })
            .catch(error => {
                ErrorMsg.logError('app.refreshConversations', error);
            });
    }

    refreshGroups() {
        this.groupRefreshTimeout = 0;
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
                    throw new Error(data['error']);
                } else {
                    this.groups = data;
                    this.groupRefreshTimeout = setTimeout(this.refreshGroups, 10000);
                }
            })
            .catch(error => {
                ErrorMsg.logError('app.refreshGroups', error);
            });
    }

    refreshSystemMessages() {
        this.systemMessageRefreshTimeout = 0;
        messaging.getSystemMessages(this.$route.fullPath)
            .then(data => {
                if (data['error']) {
                    throw new Error(data['error']);
                } else {
                    this.systemMessages = data.length > 0;
                    this.systemMessageRefreshTimeout = setTimeout(this.refreshSystemMessages, 10000);
                }
            })
            .catch(error => {
                ErrorMsg.logError('app.refreshSystemMessages', error);
            });
    }

    sendMessage() {
        this.chatErrorMessage = '';
        if (this.$store.state.uiState.messaging.groupChat) {
            messaging.sendMessageToGroup(this.$route.fullPath,
                this.$store.state.uiState.messaging.groupChat,
                this.messageText)
                .then(data => {
                    if (data.error) {
                        this.chatErrorMessage = data.error;
                    } else {
                        this.refreshChat();
                    }
                })
                .catch(error => {
                    ErrorMsg.logError('app.sendMessage', error);
                });
        } else {
            messaging.sendMessageToUser(this.$route.fullPath,
                this.$store.state.uiState.messaging.interlocutor,
                this.messageText)
                .then(data => {
                    if (data.error) {
                        this.chatErrorMessage = data.error;
                    } else {
                        this.refreshChat();
                    }
                })
                .catch(error => {
                    ErrorMsg.logError('app.sendMessage', error);
                });
        }
    }
}
