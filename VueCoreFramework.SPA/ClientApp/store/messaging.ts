import * as Api from '../api';

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
    async getConversations(returnPath: string): Promise<ConversationViewModel[]> {
        let response = await Api.getApi(`api/Message/GetConversations`, returnPath);
        if (!response.ok) {
            throw new Error(`CODE:${response.statusText}`);
        }
        return response.json() as Promise<ConversationViewModel[]>;
    },

    /**
     * Called to get the messages exchanged within the given group.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} group The name of the group whose conversation will be retrieved.
     * @returns {MessageViewModel[]} The ordered list of messages.
     */
    async getGroupMessages(returnPath: string, group: string): Promise<MessageViewModel[]> {
        let response = await Api.getApi(`api/Message/GetGroupMessages/${group}`, returnPath);
        if (!response.ok) {
            throw new Error(`CODE:${response.statusText}`);
        }
        return response.json() as Promise<MessageViewModel[]>;
    },

    /**
     * Called to get a list of users involved in individual conversations in which the given user
     * is a sender or recipient. For use by admins to review chat logs.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} proxy The name of the user whose conversation will be retrieved.
     * @returns {ConversationViewModel[]} The list of conversations.
     */
    async getProxyConversations(returnPath: string, proxy: string): Promise<ConversationViewModel[]> {
        let response = await Api.getApi(`api/Message/GetProxyConversations/${proxy}`, returnPath);
        if (!response.ok) {
            throw new Error(`CODE:${response.statusText}`);
        }
        return response.json() as Promise<ConversationViewModel[]>;
    },

    /**
     * Called to get the messages between a proxy user and the given user. For use by admins to review chat logs.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} proxy The name of the user whose conversation with the other user will be retrieved.
     * @param {string} username The name of the user whose conversation with the proxy user will be retrieved.
     * @returns {MessageViewModel[]} The ordered list of messages.
     */
    async getProxyUserMessages(returnPath: string, proxy: string, username: string): Promise<MessageViewModel[]> {
        let response = await Api.getApi(`api/Message/GetProxyUserMessages/${proxy}/${username}`, returnPath);
        if (!response.ok) {
            throw new Error(`CODE:${response.statusText}`);
        }
        return response.json() as Promise<MessageViewModel[]>;
    },

    /**
     * Called to get the system messages sent to the current user which have not been marked
     * deleted by the current user.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @returns {MessageViewModel[]} The ordered list of messages.
     */
    async getSystemMessages(returnPath: string): Promise<MessageViewModel[]> {
        let response = await Api.getApi('api/Message/GetSystemMessages', returnPath);
        if (!response.ok) {
            throw new Error(`CODE:${response.statusText}`);
        }
        return response.json() as Promise<MessageViewModel[]>;
    },

    /**
     * Called to get the messages between the current user and the given user which have not been
     * marked deleted by the current user.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} username The name of the user whose conversation with the current user will be retrieved.
     * @returns {MessageViewModel[]} The ordered list of messages.
     */
    async getUserMessages(returnPath: string, username: string): Promise<MessageViewModel[]> {
        let response = await Api.getApi(`api/Message/GetUserMessages/${username}`, returnPath);
        if (!response.ok) {
            throw new Error(`CODE:${response.statusText}`);
        }
        return response.json() as Promise<MessageViewModel[]>;
    },

    /**
     * Called to mark a conversation with a given user deleted.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} username The name of the user whose conversation with the current user will be marked deleted.
     * @returns {Response} A response object containing any error which occurred.
     */
    async markConversationDeleted(returnPath: string, username: string) {
        let response = await Api.postApi(`api/Message/MarkConversationDeleted/${username}`, returnPath);
        if (!response.ok) {
            throw new Error(`CODE:${response.statusText}`);
        }
    },

    /**
     * Called to mark a conversation with a given user read, from the perspective of the current user.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} username The name of the user whose conversation with the current user will be marked read.
     * @returns {Response} A response object containing any error which occurred.
     */
    async markConversationRead(returnPath: string, username: string) {
        let response = await Api.postApi(`api/Message/MarkConversationRead/${username}`, returnPath);
        if (!response.ok) {
            throw new Error(`CODE:${response.statusText}`);
        }
    },

    /**
     * Called to mark all system messages sent to the current user read.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @returns {Response} A response object containing any error which occurred.
     */
    async markSystemMessagesRead(returnPath: string) {
        let response = await Api.postApi('api/Message/MarkSystemMessagesRead', returnPath);
        if (!response.ok) {
            throw new Error(`CODE:${response.statusText}`);
        }
    },

    /**
     * Called to send a message to the given group.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} group The name of the group to which the message will be sent.
     * @param {string} message The message to send.
     * @returns {Response} A response object containing any error which occurred.
     */
    async sendMessageToGroup(returnPath: string, group: string, message: string) {
        let response = await Api.postApi(`api/Message/SendMessageToGroup/${group}?message=${encodeURIComponent(message)}`, returnPath);
        if (!response.ok) {
            throw new Error(`CODE:${response.statusText}`);
        }
    },

    /**
     * Called to send a message to the given user.
     * @param {string} returnPath The URL to return to if a login redirect occurs during the operation.
     * @param {string} username The name of the user to whom the message will be sent.
     * @param {string} message The message to send.
     * @returns {Response} A response object containing any error which occurred.
     */
    async sendMessageToUser(returnPath: string, username: string, message: string) {
        let response = await Api.postApi(`api/Message/SendMessageToUser/${username}?message=${encodeURIComponent(message)}`, returnPath);
        if (!response.ok) {
            throw new Error(`CODE:${response.statusText}`);
        }
    }
};