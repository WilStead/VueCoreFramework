import Vue from 'vue';
import Vuex from 'vuex';
Vue.use(Vuex);
import Oidc from 'oidc-client';
import * as Api from '../api';
import { uiState, getMenuItems, getChildItems } from './uiStore';
import { userState, PermissionData, SharePermission } from './userStore';
import { authMgr, AuthorizationViewModel, checkAuthorization } from '../authorization';
import { Repository } from './repository';
import { ConversationViewModel, MessageViewModel, messaging } from './messaging';
import { Group } from '../components/group/manage';
import * as ErrorLog from '../error-msg';

/**
 * The SPA framework's Vuex Store instance.
 */
export const store = new Vuex.Store({
    state: {
        /**
         * The current API version used by the client.
         */
        apiVer: '1.0',

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
            // If no permission is cached, try getting it.
            if (!state.userState.permissions[dataType]) {
                checkAuthorization(dataType, "view", id);
            }
            // If it still doesn't exist, the user has no permission (or the parameters are invalid)
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
            // If no permission is cached, try getting it.
            if (!state.userState.permissions[dataType]) {
                checkAuthorization(dataType, "view", id);
            }
            // If it still doesn't exist, the user has no permission (or the parameters are invalid)
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
            getMenuItems(router, state.apiVer, state.userState.culture, state.uiState.menuItems[dataItemIndex])
                .then(data => {
                    if (!state.uiState.menuItems[dataItemIndex].submenu
                        || !state.uiState.menuItems[dataItemIndex].submenu.length) {
                        state.uiState.menuItems.splice(dataItemIndex, 1);
                    }
                })
                .catch(error => ErrorLog.logError("store.addTypeRoutes.getMenuItems", error));
            getChildItems(router, state.apiVer, state.userState.culture)
                .catch(error => ErrorLog.logError("store.addTypeRoutes.getChildItems", error));

            // must be added after dynamic routes to avoid matching before them.
            router.addRoutes([{ path: '*', redirect: '/error/notfound' }]);
        },

        /**
         * Hides the chat window (returns to the messaging menu).
         */
        hideChat(state) {
            state.uiState.messaging.chatShown = false;
        },

        /**
         * Sets the current user culture.
         */
        setCulture(state, culture: string) {
            state.userState.culture = culture;
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
        setJoinedGroups(state, joinedGroups: Group[]) {
            state.userState.joinedGroups = joinedGroups;
        },

        /**
         * Sets the current users managed groups.
         */
        setManagedGroups(state, managedGroups: Group[]) {
            state.userState.managedGroups = managedGroups;
        },

        /**
         * Sets the current JWT bearer token.
         */
        setToken(state, token: string) {
            state.userState.user.access_token = token;
        },

        /**
         * Sets the current OIDC user.
         */
        setUser(state, user: Oidc.User) {
            state.userState.user = user;
        },

        /**
         * Sets the current username.
         */
        setUsername(state, username: string) {
            state.userState.username = username;
        },

        /**
         * Starts the chat UI with the given user.
         */
        startChatAdminReview(state, payload) {
            state.uiState.messaging.groupChat = '';
            state.uiState.messaging.proxySender = payload.proxySender;
            state.uiState.messaging.interlocutor = payload.interlocutor;
            state.uiState.messaging.chatShown = true;
            state.uiState.messaging.messagingShown = true;
        },

        /**
         * Starts the chat UI with the given group.
         */
        startChatWithGroup(state, group: string) {
            state.uiState.messaging.proxySender = '';
            state.uiState.messaging.interlocutor = '';
            state.uiState.messaging.groupChat = group;
            state.uiState.messaging.chatShown = true;
            state.uiState.messaging.messagingShown = true;
        },

        /**
         * Shown the chat UI for system messages.
         */
        startChatWithSystem(state) {
            state.uiState.messaging.proxySender = '';
            state.uiState.messaging.groupChat = '';
            state.uiState.messaging.interlocutor = '';
            state.uiState.messaging.chatShown = true;
            state.uiState.messaging.messagingShown = true;
        },

        /**
         * Starts the chat UI with the given user.
         */
        startChatWithUser(state, username: string) {
            state.uiState.messaging.proxySender = '';
            state.uiState.messaging.groupChat = '';
            state.uiState.messaging.interlocutor = username;
            state.uiState.messaging.chatShown = true;
            state.uiState.messaging.messagingShown = true;
        },

        /**
         * Shows/hides the messaging panel.
         */
        toggleMessaging(state) {
            state.uiState.messaging.messagingShown = !state.uiState.messaging.messagingShown;
        },

        /**
         * Sets the user's conversations.
         */
        updateConversations(state, conversations: ConversationViewModel[]) {
            state.uiState.messaging.conversations = conversations;
        },

        /**
         * Sets the user's messages.
         */
        updateMessages(state, messages: MessageViewModel[]) {
            state.uiState.messaging.messages = messages;
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
        },

        /**
         * Sets the user's system messages.
         */
        updateSystemMessages(state, messages: MessageViewModel[]) {
            state.uiState.messaging.systemMessages = messages;
        }
    },
    actions: {
        /**
         * Signs the current user out.
         */
        logout({ commit, state }) {
            state.uiState.messaging.groupChat = '';
            state.uiState.messaging.proxySender = '';
            state.uiState.messaging.interlocutor = '';
            state.uiState.messaging.chatShown = false;
            state.uiState.messaging.messagingShown = false;
            state.userState.username = 'user';
            state.userState.email = 'user@example.com';
            authMgr.signoutRedirect();
        },

        /**
         * Updates the messages in the chat window.
         */
        refreshChat({ commit, state }, returnPath) {
            if (state.uiState.messaging.groupChat) {
                return messaging.getGroupMessages(returnPath, state.uiState.messaging.groupChat)
                    .then(data => {
                        commit(updateMessages, data);
                    })
                    .catch(error => {
                        commit(updateMessages, []);
                        ErrorLog.logError('store.refreshChat', error);
                    });
            } else if (state.uiState.messaging.proxySender) {
                return messaging.getProxyUserMessages(returnPath, state.uiState.messaging.proxySender, state.uiState.messaging.interlocutor)
                    .then(data => {
                        commit(updateMessages, data);
                    })
                    .catch(error => {
                        commit(updateMessages, []);
                        ErrorLog.logError('store.refreshChat', error);
                    });
            } else if (state.uiState.messaging.interlocutor) {
                return messaging.getUserMessages(returnPath, state.uiState.messaging.interlocutor)
                    .then(data => {
                        commit(updateMessages, data);
                        messaging.markConversationRead(returnPath, state.uiState.messaging.interlocutor);
                    })
                    .catch(error => {
                        commit(updateMessages, []);
                        ErrorLog.logError('store.refreshChat', error);
                    });
            } else {
                return messaging.getSystemMessages(returnPath)
                    .then(data => {
                        commit(updateMessages, data);
                        commit(updateSystemMessages, data);
                        messaging.markSystemMessagesRead(returnPath);
                    })
                    .catch(error => {
                        commit(updateMessages, []);
                        commit(updateSystemMessages, []);
                        ErrorLog.logError('store.refreshChat', error);
                    });
            }
        },

        /**
         * Updates the user's conversations.
         */
        refreshConversations({ commit }, returnPath) {
            return messaging.getConversations(returnPath)
                .then(data => {
                    commit(updateConversations, data);
                })
                .catch(error => {
                    ErrorLog.logError('store.refreshConversations', error);
                });
        },

        /**
         * Updates the user's group memberships from the API.
         */
        refreshGroups({ commit, state }, returnPath) {
            return Api.getApi('/api/Group/GetGroupMemberships/')
                .then(response => {
                    if (!response.ok) {
                        throw new Error(response.statusText);
                    }
                    return response;
                })
                .then(response => response.json() as Promise<Group[]>)
                .then(data => {
                    let managedGroups = [];
                    let joinedGroups = [];
                    for (var i = 0; i < data.length; i++) {
                        if (data[i].manager === state.userState.username
                            || data[i].name === 'Admin' && state.userState.isSiteAdmin) {
                            managedGroups.push(data[i]);
                        } else {
                            joinedGroups.push(data[i]);
                        }
                    }
                    commit(setManagedGroups, managedGroups);
                    commit(setJoinedGroups, joinedGroups);
                })
                .catch(error => {
                    ErrorLog.logError('store.refreshGroups', error);
                });
        },

        /**
         * Updates the user's system messages.
         */
        refreshSystemMessages({ commit }, returnPath) {
            return messaging.getSystemMessages(returnPath)
                .then(data => {
                    commit(updateSystemMessages, data);
                })
                .catch(error => {
                    ErrorLog.logError('store.refreshSystemMessages', error);
                });
        }
    }
});

export const addTypeRoutes = 'addTypeRoutes';
export const hideChat = 'hideChat';
export const logout = 'logout';
export const refreshChat = 'refreshChat';
export const refreshConversations = 'refreshConversations';
export const refreshGroups = 'refreshGroups';
export const refreshSystemMessages = 'refreshSystemMessages';
export const setCulture = 'setCulture';
export const setEmail = 'setEmail';
export const setIsAdmin = 'setIsAdmin';
export const setIsSiteAdmin = 'setIsSiteAdmin';
export const setJoinedGroups = 'setJoinedGroups';
export const setManagedGroups = 'setManagedGroups';
export const setToken = 'setToken';
export const setUser = 'setUser';
export const setUsername = 'setUsername';
export const startChatAdminReview = 'startChatAdminReview';
export const startChatWithGroup = 'startChatWithGroup';
export const startChatWithSystem = 'startChatWithSystem';
export const startChatWithUser = 'startChatWithUser';
export const toggleMessaging = 'toggleMessaging';
export const updateConversations = 'updateConversations';
export const updateMessages = 'updateMessages';
export const updatePermission = 'updatePermission';
export const updateSystemMessages = 'updateSystemMessages';