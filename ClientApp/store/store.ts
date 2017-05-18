import Vue from 'vue';
import Vuex from 'vuex';
Vue.use(Vuex);
import { MenuItem, uiState } from './ui/uiStore';
import { countryData } from '../viewmodels/country';

export const store = new Vuex.Store({
    state: {
        count: 0,
        token: localStorage.getItem('token') || '',
        email: '',
        uiState,
        countryData
    },
    mutations: {
        increment(state) {
            state.count++;
        },
        setToken(state, token) {
            state.token = token;
        },
        toggleVerticalMenu(state, onOff) {
            if (onOff === undefined) state.uiState.verticalMenuShown = !state.uiState.verticalMenuShown;
            else state.uiState.verticalMenuShown = onOff.onOff;
        }
    }
});