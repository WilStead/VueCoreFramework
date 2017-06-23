import Vue from 'vue';
import VueRouter from 'vue-router';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as Store from '../../store/store';
import { checkAuthorization } from '../../router';
import * as moment from 'moment';

@Component
export default class TopbarComponent extends Vue {
    signedIn = false;
    lastUpdate: number = 0;
    updateTimeout = 0;

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

    logout() {
        this.signedIn = false;
        this.$store.commit(Store.setUsername, 'user');
        this.$store.commit(Store.setEmail, 'user@example.com');
        this.$store.commit(Store.setToken, '');
        localStorage.removeItem('token');
        fetch('/api/Account/Logout', { method: 'POST' });
        this.$router.push('/');
    }

    updateAuth() {
        this.updateTimeout = 0;
        checkAuthorization(undefined)
            .then(auth => {
                // Regardless of the authorization result, the check process will
                // set the cached email if the user is signed in.
                if (this.$store.state.userState.email
                    && this.$store.state.userState.email !== 'user@example.com') {
                    this.signedIn = true;
                } else {
                    this.signedIn = false;
                }
            });
    }
}