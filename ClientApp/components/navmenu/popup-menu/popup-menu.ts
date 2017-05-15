import Vue from 'vue';
import Component from 'vue-class-component';
import { MenuItem } from '../../../store/ui/uiStore';

@Component({
    props: {
        menuItems: Array,
        vertical: Boolean
    }
})
export default class PopupMenuComponent extends Vue {
    beforeCreate() {
        this.$options.components.MenuItemComponent = require('../menu-item/menu-item.vue');
    }
}
