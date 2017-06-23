import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import * as Store from '../../store/store';

@Component({
    components: {
        TopbarComponent: require('../topbar/topbar.vue'),
        MenuItemComponent: require('../menu-item/menu-item.vue')
    }
})
export default class AppComponent extends Vue {
    sideNav = false;

    mounted() {
        let forwardUrl = document.getElementById("forward-url").getAttribute("data-forward-url");
        if (forwardUrl) {
            this.$router.push(forwardUrl);
        }

        this.$store.commit(Store.addTypeRoutes, this.$router);
    }
}
