import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import * as Store from '../../store/store';
import { ConversationViewModel } from '../../store/messaging';
import { Group } from '../group/manage';

@Component({
    components: {
        TopbarComponent: require('../topbar/topbar.vue'),
        MenuItemComponent: require('../menu-item/menu-item.vue')
    }
})
export default class AppComponent extends Vue {
    conversations: ConversationViewModel[] = [];
    groups: Group[] = [];
    messageText = '';
    sideNav = false;
    conversationRefreshTimeout = 0;
    groupRefreshTimeout = 0;

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

    onGroupChat(group: Group) {

    }

    onUserChat(interlocutor: string) {

    }

    refreshConversations() {
        this.conversationRefreshTimeout = 0;

    }

    refreshGroups() {
        this.groupRefreshTimeout = 0;

    }

    sendMessage() {

    }
}
