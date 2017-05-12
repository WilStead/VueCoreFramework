import Vue from 'vue';
import Component from 'vue-class-component';

@Component({
    components: {
        TopbarComponent: require('./topbar/topbar.vue'),
        MenuComponent: require('../navmenu/navmenu.vue')
    }
})
export default class HeaderComponent extends Vue {
}
