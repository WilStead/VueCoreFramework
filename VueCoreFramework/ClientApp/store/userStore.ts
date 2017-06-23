export interface PermissionData {
    dataType?: string;
    id?: string;
    canShare?: string;
    permission?: string;
}

export interface SharePermission {
    canShare?: string;
    permission?: string;
}

export interface TypeSharePermission {
    canShare?: string;
    permission?: string;
    ids?: any; // a map of ids to SharePermissions
}

export interface ShareData {
    id?: number;
    level: string;
    name: string;
    shortLevel: string;
    type: string;
}

export const permissions = {
    permissionDataAll: "permission/data/all",
    permissionDataAdd: "permission/data/add",
    permissionDataEdit: "permission/data/edit",
    permissionDataView: "permission/data/view"
};

export const permissionIncludesTarget = function (permission: string, targetPermission: string) {
    if (permission === permissions.permissionDataAll) {
        return true;
    }
    else if (permission === permissions.permissionDataAdd) {
        return targetPermission !== permissions.permissionDataAll;
    }
    else if (permission === permissions.permissionDataEdit) {
        return targetPermission === permissions.permissionDataEdit
            || targetPermission === permissions.permissionDataView;
    }
    else {
        return targetPermission === permissions.permissionDataView;
    }
};

export const userState = {
    /**
     * The email address of the current user (if any).
     */
    email: 'user@example.com',

    /**
     * Indicates that the current user is an administrator.
     */
    isAdmin: false,

    /**
     * Indicates that the current user is the site administrator.
     */
    isSiteAdmin: false,

    /**
     * Lists the groups the current user manages.
     */
    managedGroups: [] as string[],

    /**
     * A collection of known permissions, as a map of dataTypes to TypeSharePermission
     * objects.
     */
    permissions: {},

    /**
     * A JWT bearer token.
     */
    token: localStorage.getItem('token') || '',

    /**
     * The username of the current user (if any).
     */
    username: 'user'
};