import Vue from 'vue';
import Vuex from 'vuex';
Vue.use(Vuex);
import { MenuItem, uiState } from './ui/uiStore';
import { CountryData } from '../viewmodels/country';

export const store = new Vuex.Store({
    state: {
        count: 0,
        token: localStorage.getItem('token') || '',
        email: '',
        uiState,
        countryData: new CountryData()
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