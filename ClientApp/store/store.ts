import Vue from 'vue';
import Vuex from 'vuex';
Vue.use(Vuex);
import { MenuItem, uiState } from './ui/uiStore';

export const store = new Vuex.Store({
    state: {
        count: 0,
        uiState
    },
    mutations: {
        increment(state) {
            state.count++;
        },
        toggleVerticalMenu(state, onOff) {
            if (onOff === undefined) state.uiState.verticalMenuShown = !state.uiState.verticalMenuShown;
            else state.uiState.verticalMenuShown = onOff.onOff;
        }
    }
});