import Vue from 'vue';
import Vuex from 'vuex';
Vue.use(Vuex);
import { uiState, getMenuItems, getChildItems } from './uiStore';
import { userState, SharePermissionData } from './userStore';
import { Repository } from './repository';
import * as ErrorLog from '../error-msg';

/**
 * The SPA framework's Vuex Store instance.
 */
export const store = new Vuex.Store({
    state: {
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
        repositories: {},

        /**
         * An object containing information about the state of the UI.
         */
        uiState,

        /**
         * An object containing information about the state of the current user.
         */
        userState
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
        },

        /**
         * Sets the current user email.
         */
        setEmail(state, email: string) {
            state.userState.email = email;
        },

        /**
         * Sets the current JWT bearer token.
         */
        setToken(state, token: string) {
            state.userState.token = token;
        },

        /**
         * Sets the current username.
         */
        setUsername(state, username: string) {
            state.userState.username = username;
        },

        /**
         * Adds share/hide information to the store.
         */
        updateSharePermission(state, permission: SharePermissionData) {
            if (permission.dataType) {
                if (state.userState.sharePermissions[permission.dataType] === undefined) {
                    state.userState.sharePermissions[permission.dataType] = {};
                }
                if (permission.canShare) {
                    state.userState.sharePermissions[permission.dataType].canShare = true;
                } else if (permission.id) {
                    if (state.userState.sharePermissions[permission.dataType].ids === undefined) {
                        state.userState.sharePermissions[permission.dataType].ids = [];
                    }
                    if (state.userState.sharePermissions[permission.dataType].ids.indexOf(permission.id) == -1) {
                        state.userState.sharePermissions[permission.dataType].ids.push(permission.id);
                    }
                }
            }
        }
    }
});

export const addTypeRoutes = 'addTypeRoutes';
export const setEmail = 'setEmail';
export const setToken = 'setToken';
export const setUsername = 'setUsername';
export const updateSharePermission = 'updateSharePermission';