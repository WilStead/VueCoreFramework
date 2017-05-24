import Vue from 'vue';
import Component from 'vue-class-component';
import { checkAuthorization } from '../../router';

@Component
export default class TopbarComponent extends Vue {
    signedIn = false;
    mounted() { this.updateAuth(); }
    beforeUpdate() { this.updateAuth(); }

    getReturnUrl() {
        let returnUrl: string = this.$route.query.returnUrl;
        if (!returnUrl) {
            returnUrl = this.$route.fullPath;
        }
        return returnUrl;
    }

    logout() {
        this.$store.state.token = '';
        localStorage.removeItem('token');
        fetch('/api/Account/Logout', { method: 'POST' });
        this.$router.push('/');
    }

    updateAuth() {
        checkAuthorization(undefined, this.getReturnUrl())
            .then(auth => {
                if (auth) {
                    this.signedIn = true;
                } else {
                    this.signedIn = false;
                }
            });
    }
}