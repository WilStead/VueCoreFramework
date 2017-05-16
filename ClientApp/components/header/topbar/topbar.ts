import Vue from 'vue';
import Component from 'vue-class-component';
import { checkAuthorization } from '../../../router';

@Component
export default class TopbarComponent extends Vue {
    userActivity = "sign in";
    created() {
        checkAuthorization(undefined)
            .then(auth => {
                if (auth) {
                    this.userActivity = "sign out";
                } else {
                    this.userActivity = "sign in";
                }
            });
    }

    getReturnUrl() {
        let returnUrl: string = this.$route.query.returnUrl;
        if (!returnUrl) {
            returnUrl = this.$route.fullPath;
        }
        return returnUrl;
    }
}