import Vue from 'vue';
import Vuex from 'vuex';
Vue.use(Vuex);
import { MenuItem, uiState } from './ui/uiStore';
import { countryData } from '../viewmodels/country';

export const store = new Vuex.Store({
    state: {
        count: 0,
        author: 'Wil Stead',
        year: '2017',
        contact: 'wil.stead@williamstead.com',
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
        }
    }
});