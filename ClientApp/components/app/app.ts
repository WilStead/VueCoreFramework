import Vue from 'vue';
import Component from 'vue-class-component';

@Component({
    components: {
        TopbarComponent: require('../topbar/topbar.vue'),
        MenuItemComponent: require('../menu-item/menu-item.vue')
    }
})
export default class AppComponent extends Vue {
    sideNav = false;
}
