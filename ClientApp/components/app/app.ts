import Vue from 'vue';
import Component from 'vue-class-component';

@Component({
    components: {
        HeaderComponent: require('../header/header.vue')
    }
})
export default class AppComponent extends Vue {
}
