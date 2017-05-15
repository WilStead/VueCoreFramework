import Vue from 'vue';
import Component from 'vue-class-component';
import { MenuItem } from '../../../store/ui/uiStore';

@Component({
    props: {
        menuItem: Object,
        vertical: Boolean,
        inPopup: Boolean
    },
    components: {
        PopupMenuComponent: require('../popup-menu/popup-menu.vue')
    }
})
export default class MenuItemComponent extends Vue {
    get chevronDirection() {
        if (this.$props.inPopup && !this.$props.vertical) {
            return 'glyphicon-chevron-right';
        } else if (this.$props.vertical && !(this.mouseInPopup || this.mouseInItem || this.manualShow)) {
            return 'glyphicon-chevron-right';
        } else {
            return 'glyphicon-chevron-down';
        }
    }
    
    popupLeft: string = '0';
    popupTop: string = '3.4rem';

    mouseInPopup: boolean = false;
    onPopupMouseEnter(event) {
        if (!this.$props.vertical) this.mouseInPopup = true;
    }
    onPopupMouseLeave(event) {
        this.mouseInPopup = false;
    }

    mouseInItem: boolean = false;
    onItemMouseEnter(event) {
        if (!this.$props.vertical) {
            this.mouseInItem = true;
            this.popupLeft = this.$props.inPopup ? '16rem' : '0';
            this.popupTop = this.$props.inPopup ? '0' : '3.4rem';
        }
    }
    onItemMouseLeave(event) {
        this.mouseInItem = false;
    }

    manualShow: boolean = false;
    toggle() {
        this.manualShow = !this.manualShow;
    }
}
