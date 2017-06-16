import Vue from 'vue';
import Component from 'vue-class-component';
import { checkAuthorization } from '../../router';
import * as moment from 'moment';

@Component
export default class TopbarComponent extends Vue {
    signedIn = false;
    lastUpdate: number = 0;

    mounted() { this.updateAuth(); }
    beforeUpdate() {
        if (this.lastUpdate < moment().subtract(30, 's').valueOf()) {
            this.updateAuth();
            this.lastUpdate = moment().valueOf();
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
        this.$store.state.username = 'user';
        this.$store.state.email = 'user@example.com';
        this.$store.state.token = '';
        localStorage.removeItem('token');
        fetch('/api/Account/Logout', { method: 'POST' });
        this.$router.push('/');
    }

    updateAuth() {
        checkAuthorization(undefined)
            .then(auth => {
                // Regardless of the authorization result, the check process will
                // set the cached email if the user is signed in.
                if (this.$store.state.email && this.$store.state.email !== 'user@example.com') {
                    this.signedIn = true;
                } else {
                    this.signedIn = false;
                }
            });
    }
}