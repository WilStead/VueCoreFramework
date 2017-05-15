import Vue from 'vue';
import Component from 'vue-class-component';

@Component({
    components: {
        HeaderComponent: require('../header/header.vue'),
        NavmenuComponent: require('../navmenu/navmenu.vue'),
        FooterComponent: require('../footer/footer.vue')
    }
})
export default class AppComponent extends Vue { }
