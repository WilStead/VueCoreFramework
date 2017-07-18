import Oidc from 'oidc-client';
import { Group } from '../components/group/manage';

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
     * Indicates the current culture selected by the user.
     */
    culture: 'en-US',

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
     * The groups to which the current user belongs, but does not manage.
     */
    joinedGroups: [] as Group[],

    /**
     * The groups the current user manages.
     */
    managedGroups: [] as Group[],

    /**
     * A collection of known permissions, as a map of dataTypes to TypeSharePermission
     * objects.
     */
    permissions: {},

    /**
     * The current OIDC user (if any).
     */
    user: null as Oidc.User,

    /**
     * The username of the current user (if any).
     */
    username: 'user'
};