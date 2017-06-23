import Vue from 'vue';
import Vuex from 'vuex';
Vue.use(Vuex);
import { uiState, getMenuItems, getChildItems } from './uiStore';
import { userState, PermissionData, SharePermission } from './userStore';
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
        /**
         * Gets the stored permission for the given data, if any exists.
         */
        getPermission: (state, getters) => (dataType: string, id?: string): string => {
            if (!state.userState.permissions[dataType]) {
                return undefined;
            }
            if (id) {
                if (!state.userState.permissions[dataType].ids
                    || !state.userState.permissions[dataType].ids[id]) {
                    return undefined;
                } else {
                    return state.userState.permissions[dataType].ids[id].permission;
                }
            } else {
                return state.userState.permissions[dataType].permission;
            }
        },

        /**
         * Retrieves a repository for the given data type, using a cached version if possible.
         */
        getRepository: (state, getters) => (dataType: string): Repository => {
            if (!state.repositories[dataType]) {
                state.repositories[dataType] = new Repository(dataType);
            }
            return state.repositories[dataType];
        },

        /**
         * Gets the stored share permission for the given data, if any exists.
         */
        getSharePermission: (state, getters) => (dataType: string, id?: string): string => {
            if (!state.userState.permissions[dataType]) {
                return undefined;
            }
            if (id) {
                if (!state.userState.permissions[dataType].ids
                    || !state.userState.permissions[dataType].ids[id]) {
                    return undefined;
                } else {
                    return state.userState.permissions[dataType].ids[id].canShare;
                }
            } else {
                return state.userState.permissions[dataType].canShare;
            }
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

        logout(state) {
            state.userState.username = 'user';
            state.userState.email = 'user@example.com';
            state.userState.token = '';
            localStorage.removeItem('token');
            fetch('/api/Account/Logout', { method: 'POST' });
        },

        /**
         * Sets the current user email.
         */
        setEmail(state, email: string) {
            state.userState.email = email;
        },

        /**
         * Sets whether the current user is an administrator.
         */
        setIsAdmin(state, isAdmin: boolean) {
            state.userState.isAdmin = isAdmin;
        },

        /**
         * Sets whether the current user is the site administrator.
         */
        setIsSiteAdmin(state, isSiteAdmin: boolean) {
            state.userState.isSiteAdmin = isSiteAdmin;
        },

        /**
         * Sets the current users managed groups.
         */
        setManagedGroups(state, managedGroups: string[]) {
            state.userState.managedGroups = managedGroups;
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
         * Adds permission information to the store.
         */
        updatePermission(state, permission: PermissionData) {
            if (permission.dataType) {
                if (!state.userState.permissions[permission.dataType]) {
                    state.userState.permissions[permission.dataType] = {};
                }
                if (permission.id) {
                    if (!state.userState.permissions[permission.dataType].ids) {
                        state.userState.permissions[permission.dataType].ids = {};
                    }
                    state.userState.permissions[permission.dataType].ids[permission.id] = <SharePermission>{
                        canShare: permission.canShare,
                        permission: permission.permission
                    };
                } else {
                    state.userState.permissions[permission.dataType].canShare = permission.canShare;
                    state.userState.permissions[permission.dataType].permission = permission.permission;
                }
            }
        }
    }
});

export const addTypeRoutes = 'addTypeRoutes';
export const logout = 'logout';
export const setEmail = 'setEmail';
export const setIsAdmin = 'setIsAdmin';
export const setIsSiteAdmin = 'setIsSiteAdmin';
export const setManagedGroups = 'setManagedGroups';
export const setToken = 'setToken';
export const setUsername = 'setUsername';
export const updatePermission = 'updatePermission';