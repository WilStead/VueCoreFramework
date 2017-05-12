import Vue from 'vue';
import Component from 'vue-class-component';
import { State } from 'vuex-class';

@Component
export default class NavmenuComponent extends Vue {
    @State uiState
}
