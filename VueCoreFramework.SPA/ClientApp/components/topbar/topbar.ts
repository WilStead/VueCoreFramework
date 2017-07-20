import Vue from 'vue';
import VueRouter from 'vue-router';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as Store from '../../store/store';
import { authenticate, authMgr } from '../../authorization';
import * as moment from 'moment';

@Component
export default class TopbarComponent extends Vue {
    signedIn = false;
    lastUpdate: number = 0;
    updateTimeout = 0;

    get totalUnread() {
        return this.$store.state.uiState.messaging.conversations
            .map(c => c.unreadCount)
            .reduce((a, b) => { return a + b; }, 0)
            + this.$store.state.uiState.messaging.systemMessages
                .filter(m => !m.received).length;
    }

    mounted() { this.updateAuth(); }

    @Watch('$route')
    onRouteChange(val: VueRouter.Route, oldVal: VueRouter.Route) {
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateAuth, 500);
        }
    }

    getReturnUrl() {
        let returnUrl: string = this.$route.query.returnUrl;
        if (!returnUrl) {
            returnUrl = this.$route.fullPath;
        }
        return returnUrl;
    }

    onLogin() {
        this.$router.push({ path: '/login', query: { returnUrl: this.getReturnUrl() } });
    }

    onLogout() {
        this.signedIn = false;
        this.$store.dispatch(Store.logout);
        this.$router.push('/');
    }

    onToggleChat() {
        if (!this.$store.state.uiState.messaging.messagingShown) {
            this.$store.dispatch(Store.refreshGroups, this.$route.fullPath);
            this.$store.dispatch(Store.refreshConversations, this.$route.fullPath);
            this.$store.dispatch(Store.refreshChat, this.$route.fullPath);
        }
        this.$store.commit(Store.toggleMessaging);
    }

    updateAuth() {
        this.updateTimeout = 0;
        authenticate()
            .then(auth => {
                if (auth === "authorized") {
                    this.signedIn = true;
                } else {
                    this.signedIn = false;
                }
            });
    }
}