import Vue from 'vue';
import Vuex from 'vuex';
Vue.use(Vuex);
import { MenuItem, uiState, getMenuItems } from './ui/uiStore';

export const store = new Vuex.Store({
    state: {
        count: 0,
        author: 'Wil Stead',
        year: '2017',
        contact: 'wil.stead@williamstead.com',
        token: localStorage.getItem('token') || '',
        email: '',
        uiState,
        error: {
            dialogShown: false,
            message: ''
        }
    },
    mutations: {
        increment(state) {
            state.count++;
        },
        setToken(state, token) {
            state.token = token;
        },
        addMenuItems(state, router) {
            let dataItemIndex = state.uiState.menuItems.findIndex(v => v.text === "Data");
            getMenuItems(router)
                .then(data => {
                    if (data.length) {
                        state.uiState.menuItems[dataItemIndex].submenu = data;
                    } else {
                        state.uiState.menuItems.splice(dataItemIndex, 1);
                    }
                });

            // must be added after dynamic routes to avoid matching before them.
            router.addRoutes([{ path: '*', redirect: '/error/notfound' }]);
        }
    }
});