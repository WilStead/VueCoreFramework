import Vue from 'vue';
import Vuex from 'vuex';
Vue.use(Vuex);
import { uiState, getMenuItems, getChildItems } from './ui/uiStore';
import { Repository } from './repository';
import * as ErrorLog from '../error-msg';

/**
 * The SPA framework's Vuex Store instance.
 */
export const store = new Vuex.Store({
    state: {
        /**
         * The site author(s).
         */
        author: 'Wil Stead',

        /**
         * The copyright year(s) for the site.
         */
        year: '2017',

        /**
         * The contact email address for the site.
         */
        contact: 'wil.stead@williamstead.com',

        /**
         * A JWT bearer token.
         */
        token: localStorage.getItem('token') || '',

        /**
         * The username of the current user (if any).
         */
        username: 'user',

        /**
         * The email address of the current user (if any).
         */
        email: 'user@example.com',

        /**
         * An object containing information about the state of the UI.
         */
        uiState,

        /**
         * Info used to display a model error dialog.
         */
        error: {
            dialogShown: false,
            message: ''
        },

        /**
         * A collection of cached Repository objects, mapped by dataType. Should be retrieved with
         * the getter, not directly, since they are instantiated on demand.
         */
        repositories: {}
    },
    getters: {
        getRepository: (state, getters) => (dataType: string): Repository => {
            if (!state.repositories[dataType]) {
                state.repositories[dataType] = new Repository(dataType);
            }
            return state.repositories[dataType];
        }
    },
    mutations: {
        /**
         * Sets the current JWT bearer token.
         */
        setToken(state, token) {
            state.token = token;
        },

        /**
         * Initializes the VueRouter with data type info from the API.
         */
        addTypeRoutes(state, router) {
            let dataItemIndex = state.uiState.menuItems.findIndex(v => v.dataHook);
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