import Vue from 'vue';
import Vuex from 'vuex';
Vue.use(Vuex);
import { uiState, getMenuItems, getChildItems } from './ui/uiStore';
import * as ErrorLog from '../error-msg';

/**
 * The SPA framework's Vuex Store instance.
 */
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
        addTypeRoutes(state, router) {
            let dataItemIndex = state.uiState.menuItems.findIndex(v => v.text === "Data");
            getMenuItems(router, state.uiState.menuItems[dataItemIndex])
                .then(data => {
                    if (!state.uiState.menuItems[dataItemIndex].submenu
                        || !state.uiState.menuItems[dataItemIndex].submenu.length) {
                        state.uiState.menuItems.splice(dataItemIndex, 1);
                    }
                })
                .catch(error => ErrorLog.logError("store.addTypeRoutes.getMenuItems", error));
            getChildItems(router)
                .catch(error => ErrorLog.logError("store.addTypeRoutes.getChildItems", error));

            // must be added after dynamic routes to avoid matching before them.
            router.addRoutes([{ path: '*', redirect: '/error/notfound' }]);
        }
    }
});