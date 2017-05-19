import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import { MenuItem } from '../../store/ui/uiStore';

@Component
export default class MenuItemComponent extends Vue {
    @Prop()
    menuItem: MenuItem;

    active = false;
    toggle() { this.active = !this.active; }
}
