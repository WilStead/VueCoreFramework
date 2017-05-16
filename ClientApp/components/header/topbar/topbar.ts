import Vue from 'vue';
import Component from 'vue-class-component';
import { checkAuthorization } from '../../../router';

@Component
export default class TopbarComponent extends Vue {
    userActivity = "sign in";
    updateAuth() {
        checkAuthorization(undefined, this.getReturnUrl())
            .then(auth => {
                if (auth) {
                    this.userActivity = "sign out";
                } else {
                    this.userActivity = "sign in";
                }
            });
    }
    created() { this.updateAuth(); }
    beforeUpdate() { this.updateAuth(); }

    getReturnUrl() {
        let returnUrl: string = this.$route.query.returnUrl;
        if (!returnUrl) {
            returnUrl = this.$route.fullPath;
        }
        return returnUrl;
    }
}