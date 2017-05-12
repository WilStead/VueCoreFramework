import Vue from 'vue';
import Component from 'vue-class-component';
import { Mutation } from 'vuex-class';

@Component({
    components: {
        TopbarComponent: require('./topbar/topbar.vue'),
        NavmenuComponent: require('../navmenu/navmenu.vue')
    }
})
export default class HeaderComponent extends Vue {
    @Mutation toggleVerticalMenu;
}
