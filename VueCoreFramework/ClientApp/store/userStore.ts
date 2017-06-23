export interface SharePermissionData {
    dataType?: string;
    canShare?: boolean;
    id?: string;
}

export interface SharePermission {
    canShare?: boolean;
    ids?: string[];
}

export const userState = {
    /**
     * The email address of the current user (if any).
     */
    email: 'user@example.com',

    /**
     * A collection of known share/hide permissions, as a map of dataTypes to SharePermission
     * objects.
     */
    sharePermissions: {},

    /**
     * A JWT bearer token.
     */
    token: localStorage.getItem('token') || '',

    /**
     * The username of the current user (if any).
     */
    username: 'user'
};