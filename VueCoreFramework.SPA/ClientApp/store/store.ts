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
        async addTypeRoutes(state, router) {
            let dataItemIndex = state.uiState.menuItems.findIndex(v => v.dataHook);
            if (dataItemIndex !== -1) {
                try {
                    await getMenuItems(router, state.apiVer, state.userState.culture, state.uiState.menuItems[dataItemIndex]);
                    await getChildItems(router, state.apiVer, state.userState.culture);
                } catch (error) {
                    ErrorLog.logError("store.addTypeRoutes", error);
                }
            }
            // Must be added after dynamic routes to avoid matching before them.
            router.addRoutes([{ path: '*', redirect: '/error/notfound' }]);
        },

        /**
         * Hides the chat window (returns to the messaging menu).
         */
        hideChat(state) {
            state.uiState.messaging.chatShown = false;
        },

        /**
         * Signs the current user out.
         */
        logout(state) {
            state.uiState.messaging.groupChat = '';
            state.uiState.messaging.proxySender = '';
            state.uiState.messaging.interlocutor = '';
            state.uiState.messaging.chatShown = false;
            state.uiState.messaging.messagingShown = false;
            state.userState.user = null;
            state.userState.username = 'user';
        },

        /**
         * Sets the current user culture.
         */
        setCulture(state, culture: string) {
            state.userState.culture = culture;
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
         * Sets the current OIDC user.
         */
        setUser(state, user: Oidc.User) {
            state.userState.user = user;
            state.userState.username = user == null ? 'user' : user.profile.name;
            state.userState.isAdmin = user == null ? false : user.profile.role.includes("Admin");
            state.userState.isSiteAdmin = user == null ? false : user.profile.role.includes("SiteAdmin");
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
         * Updates the messages in the chat window.
         */
        async refreshChat({ commit, state }, returnPath) {
            try {
                let data: MessageViewModel[];
                if (state.uiState.messaging.groupChat) {
                    data = await messaging.getGroupMessages(returnPath, state.uiState.messaging.groupChat);
                } else if (state.uiState.messaging.proxySender) {
                    data = await messaging.getProxyUserMessages(returnPath, state.uiState.messaging.proxySender, state.uiState.messaging.interlocutor);
                } else if (state.uiState.messaging.interlocutor) {
                    data = await messaging.getUserMessages(returnPath, state.uiState.messaging.interlocutor);
                    messaging.markConversationRead(returnPath, state.uiState.messaging.interlocutor);
                } else {
                    data = await messaging.getSystemMessages(returnPath);
                    commit(updateSystemMessages, data);
                    messaging.markSystemMessagesRead(returnPath);
                }
                commit(updateMessages, data);
            } catch (error) {
                ErrorLog.logError('store.refreshChat', error);
            }
        },

        /**
         * Updates the user's conversations.
         */
        async refreshConversations({ commit }, returnPath) {
            try {
                let data = await messaging.getConversations(returnPath);
                commit(updateConversations, data);
            } catch (error) {
                ErrorLog.logError('store.refreshConversations', error);
            }
        },

        /**
         * Updates the user's group memberships from the API.
         */
        async refreshGroups({ commit, state }, returnPath) {
            try {
                let response = await Api.getApi('api/Group/GetGroupMemberships/');
                if (!response.ok) {
                    throw new Error(response.statusText);
                }
                let data = await response.json() as Group[];
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
            } catch (error) {
                ErrorLog.logError('store.refreshGroups', error);
            }
        },

        /**
         * Updates the user's system messages.
         */
        async refreshSystemMessages({ commit }, returnPath) {
            try {
                let data = await messaging.getSystemMessages(returnPath);
                commit(updateSystemMessages, data);
            } catch (error) {
                ErrorLog.logError('store.refreshSystemMessages', error);
            }
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
export const setJoinedGroups = 'setJoinedGroups';
export const setManagedGroups = 'setManagedGroups';
export const setUser = 'setUser';
export const startChatAdminReview = 'startChatAdminReview';
export const startChatWithGroup = 'startChatWithGroup';
export const startChatWithSystem = 'startChatWithSystem';
export const startChatWithUser = 'startChatWithUser';
export const toggleMessaging = 'toggleMessaging';
export const updateConversations = 'updateConversations';
export const updateMessages = 'updateMessages';
export const updatePermission = 'updatePermission';
export const updateSystemMessages = 'updateSystemMessages';