import { ApiResponseViewModel, checkResponse } from '../router';
import { store } from './store';

export interface MessageViewModel {
    /**
     * The content of the message. May have markdown-formatted text.
     */
    content: string;

    /**
     * Indicates that the message is from the system, rather than from a user.
     */
    isSystemMessage?: boolean;

    /**
     * Indicates that the user who sent the message is an admin.
     */
    isUserAdmin?: boolean;

    /**
     * Indicates that the user who sent the message is the group manager.
     */
    isUserManager?: boolean;

    /**
     * Indicates that the user who sent the message is the site admin.
     */
    isUserSiteAdmin?: boolean;

    /**
     * Indicates that the single recipient has read the message.
     */
    received?: boolean;

    /**
     * The name of the user who sent the message.
     */
    username?: string;

    /**
     * The date and time when the message was sent.
     */
    timestamp: string;
}

export interface ConversationViewModel {
    /**
     * The username of the other party in the conversation.
     */
    interlocutor: string;

    /**
     * The number of messages the current user has not read in the conversation.
     */
    unreadCount: number;
}

export const messaging = {
    /**
     * Called to get a list of users involved in individual conversations in which the current user
     * is a sender or recipient, with an unread message count.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @returns {ConversationViewModel[]} The list of conversations.
     */
    getConversations(returnPath: string): Promise<ConversationViewModel[]> {
        return fetch(`/api/Message/GetConversations`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ConversationViewModel[]>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    },

    /**
     * Called to get the messages exchanged within the given group.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} group The name of the group whose conversation will be retrieved.
     * @returns {MessageViewModel[]} The ordered list of messages.
     */
    getGroupMessages(returnPath: string, group: string): Promise<MessageViewModel[]> {
        return fetch(`/api/Message/GetGroupMessages/${group}`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<MessageViewModel[]>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    },

    /**
     * Called to get a list of users involved in individual conversations in which the given user
     * is a sender or recipient. For use by admins to review chat logs.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} proxy The name of the user whose conversation will be retrieved.
     * @returns {ConversationViewModel[]} The list of conversations.
     */
    getProxyConversations(returnPath: string, proxy: string): Promise<ConversationViewModel[]> {
        return fetch(`/api/Message/GetProxyConversations`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ConversationViewModel[]>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    },

    /**
     * Called to get the messages between a proxy user and the given user. For use by admins to review chat logs.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} proxy The name of the user whose conversation with the other user will be retrieved.
     * @param {string} username The name of the user whose conversation with the proxy user will be retrieved.
     * @returns {MessageViewModel[]} The ordered list of messages.
     */
    getProxyUserMessages(returnPath: string, proxy: string, username: string): Promise<MessageViewModel[]> {
        return fetch(`/api/Message/GetProxyUserMessages/${proxy}/${username}`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<MessageViewModel[]>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    },

    /**
     * Called to get the system messages sent to the current user which have not been marked
     * deleted by the current user.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @returns {MessageViewModel[]} The ordered list of messages.
     */
    getSystemMessages(returnPath: string): Promise<MessageViewModel[]> {
        return fetch('/api/Message/GetSystemMessages',
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<MessageViewModel[]>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    },

    /**
     * Called to get the messages between the current user and the given user which have not been
     * marked deleted by the current user.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} username The name of the user whose conversation with the current user will be retrieved.
     * @returns {MessageViewModel[]} The ordered list of messages.
     */
    getUserMessages(returnPath: string, username: string): Promise<MessageViewModel[]> {
        return fetch(`/api/Message/GetUserMessages/${username}`,
            {
                method: 'GET',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<MessageViewModel[]>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    },

    /**
     * Called to mark a conversation with a given user deleted.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} username The name of the user whose conversation with the current user will be marked deleted.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    markConversationDeleted(returnPath: string, username: string): Promise<ApiResponseViewModel> {
        return fetch(`/api/Message/MarkConversationDeleted/${username}`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    },

    /**
     * Called to mark a conversation with a given user read, from the perspective of the current user.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} username The name of the user whose conversation with the current user will be marked read.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    markConversationRead(returnPath: string, username: string): Promise<ApiResponseViewModel> {
        return fetch(`/api/Message/MarkConversationRead/${username}`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    },

    /**
     * Called to mark all system messages sent to the current user read.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    markSystemMessagesRead(returnPath: string): Promise<ApiResponseViewModel> {
        return fetch('/api/Message/MarkSystemMessagesRead',
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    },

    /**
     * Called to send a message to the given group.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} group The name of the group to which the message will be sent.
     * @param {string} message The message to send.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    sendMessageToGroup(returnPath: string, group: string, message: string): Promise<ApiResponseViewModel> {
        return fetch(`/api/Message/SendMessageToGroup/${group}?message=${encodeURIComponent(message)}`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    },

    /**
     * Called to send a message to the given user.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} username The name of the user to whom the message will be sent.
     * @param {string} message The message to send.
     * @returns {ApiResponseViewModel} A response object containing any error which occurred.
     */
    sendMessageToUser(returnPath: string, username: string, message: string): Promise<ApiResponseViewModel> {
        return fetch(`/api/Message/SendMessageToUser/${username}?message=${encodeURIComponent(message)}`,
            {
                method: 'POST',
                headers: {
                    'Accept': 'application/json',
                    'Authorization': `bearer ${store.state.userState.token}`
                }
            })
            .then(response => checkResponse(response, returnPath))
            .then(response => response.json() as Promise<ApiResponseViewModel>)
            .catch(error => {
                throw new Error(`There was a problem with your request. ${error}`);
            });
    }
};