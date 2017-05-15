import Vue from 'vue';
import Component from 'vue-class-component';

@Component({
    props: {
        vertical: Boolean
    },
    components: {
        MenuItemComponent: require('./menu-item/menu-item.vue')
    }
})
export default class NavmenuComponent extends Vue { }
