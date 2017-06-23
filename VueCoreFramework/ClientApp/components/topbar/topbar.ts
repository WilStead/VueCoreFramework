import Vue from 'vue';
import VueRouter from 'vue-router';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as Store from '../../store/store';
import { authenticate } from '../../router';
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
        this.$store.commit(Store.logout);
        this.$router.push('/');
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