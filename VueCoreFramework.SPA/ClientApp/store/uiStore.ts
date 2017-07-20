import VueRouter from 'vue-router';
import * as Api from '../api';
import * as ErrorMsg from '../error-msg';
import { Repository } from './repository';
import { router } from '../router';
import { ConversationViewModel, MessageViewModel } from '../store/messaging';

/**
 * Describes an item in the SPA framework's main menu.
 */
export interface MenuItem {
    /**
     * The text displayed on the menu item.
     */
    text: string;

    /**
     * The name of a Material Icon used to decorate the menu item.
     */
    iconClass: string;

    /**
     * Indicates that the iconClass is a FontAwesome icon, rather than a Material Icon.
     */
    fontAwesome?: boolean;

    /**
     * Indicates that this menu item is where all auto-generated menu items will be nested. Only
     * the first such item found will be used, if more than one is set. At least one *must* be
     * present, or the framework will be unable to scaffold data type items dynamically.
     */
    dataHook?: boolean;

    /**
     * Indicates that this menu item is a divider. Overrides all other properties.
     */
    divider?: boolean;

    /**
     * Indicates that this menu item is a heading. Overrides all other properties except text.
     */
    header?: boolean;

    /**
     * An optional route name which will be navigated to when the menu item is selected.
     */
    route?: string;

    /**
     * An optional collection of sub-items contained within this menu item.
     */
    submenu?: Array<MenuItem>;

    /**
     * A key used to track items in a list.
     */
    key?: number;
}

/**
 * An object containing information about the state of the UI.
 */
export const uiState = {
    /**
     * The site author(s).
     */
    author: 'Wil Stead',

    /**
     * The contact email address for the site.
     */
    contact: 'wil.stead@williamstead.com',

    /**
     * The copyright year(s) for the site.
     */
    year: '2017',

    /**
     * The MenuItems displayed in the SPA framework's main menu.
     */
    menuItems: <MenuItem[]>[
        {
            text: 'Home',
            iconClass: 'home',
            route: '/',
            key: 100
        },
        {
            divider: true,
            key: 200
        },
        {
            text: 'Data',
            header: true,
            key: 300
        },
        {
            text: 'Data',
            iconClass: 'view_list',
            dataHook: true,
            key: 400
        },
        {
            divider: true,
            key: 500
        },
        {
            text: 'Groups',
            header: true,
            key: 600
        },
        {
            text: 'Groups',
            iconClass: 'group',
            route: '/group/manage',
            key: 700
        }
    ],

    messaging: {
        /**
         * Controls whether the messaging sidebar is shown.
         */
        messagingShown: false,


        /**
         * Controls whether the chat window is shown (rather than the menu).
         */
        chatShown: false,

        /**
         * The individual conversations in which this user is currently involved (does not include
         * group chats).
         */
        conversations: [] as ConversationViewModel[],


        /**
         * Indicates the name of the group the user is currently chatting with.
         */
        groupChat: '',


        /**
         * Indicates the name of the user the current user is currently chatting with.
         */
        interlocutor: '',


        /**
         * The messages of the current conversation.
         */
        messages: [] as MessageViewModel[],

        /**
         * When an admin is viewing a conversation, this is the sender.
         */
        proxySender: '',

        /**
         * The system messages the current user has received.
         */
        systemMessages: [] as MessageViewModel[]
    }
};

function addMenuItem(menu: MenuItem, router: VueRouter, data: any, dataClass: string, category: string, iconClass: string) {
    if (!iconClass) {
        iconClass = 'view_list';
    }
    let index = category.indexOf('/');
    if (index === 0 && category.length > 1) {
        index = category.substring(1).indexOf('/'); // skip first if it's not the only character
    }
    let currentCategory = index <= 0 ? category : category.substring(0, index);
    category = index <= 0 ? '' : category.substring(index + 1);
    if (currentCategory && currentCategory != '/') {
        let menuItem = null;
        if (menu.submenu && menu.submenu.length) {
            for (var i = 0; i < menu.submenu.length; i++) {
                if (menu.submenu[i].text === currentCategory) {
                    menuItem = menu.submenu[i];
                    break;
                }
            }
        }
        if (!menuItem) {
            if (!menu.submenu) {
                menu.submenu = [];
            }
            menuItem = {
                text: currentCategory,
                fontAwesome: data[dataClass].fontAwesome,
                iconClass,
                submenu: [],
                key: Date.now()
            };
            menu.submenu.push(menuItem);
        }
        addMenuItem(menuItem, router, data, dataClass, category, iconClass);
    } else {
        let baseRoute = `/data/${dataClass.toLowerCase()}`;
        let tableRoute = baseRoute + '/table';

        router.addRoutes([{
            path: baseRoute,
            meta: { requiresAuthorize: true },
            component: require('../components/data/dashboard.vue').default,
            props: {
                title: dataClass,
                iconClass,
                fontAwesome: data[dataClass].fontAwesome
            },
            children: [
                {
                    name: dataClass + "DataTable",
                    path: 'table',
                    components: {
                        content: data[dataClass].dashboardTableContent
                            ? require(`../components/data/${data[dataClass].dashboardTableContent}.vue`).default
                            : require('../components/data/empty.vue').default,
                        data: require('../dynamic-data/dynamic-table/dynamic-table.vue').default
                    }
                },
                {
                    name: dataClass,
                    path: ':operation/:id',
                    components: {
                        content: data[dataClass].dashboardFormContent
                            ? require(`../components/data/${data[dataClass].dashboardFormContent}.vue`).default
                            : require('../components/data/empty.vue').default,
                        data: require('../dynamic-data/dynamic-form/dynamic-form.vue').default
                    },
                    props: { content: true, data: true }
                }
            ]
        }]);

        let menuItem: MenuItem = null;
        if (menu.submenu && menu.submenu.length) {
            for (var i = 0; i < menu.submenu.length; i++) {
                if (menu.submenu[i].text === dataClass) {
                    menuItem = menu.submenu[i];
                    break;
                }
            }
        }
        if (!menuItem) {
            if (!menu.submenu) {
                menu.submenu = [];
            }
            menuItem = {
                text: dataClass,
                iconClass,
                fontAwesome: data[dataClass].fontAwesome,
                route: tableRoute,
                key: Date.now()
            };
            menu.submenu.push(menuItem);
        } else {
            menuItem.iconClass = iconClass;
            menuItem.fontAwesome = data[dataClass].fontAwesome;
            menuItem.route = tableRoute;
        }
    }
}

/**
 * Retrieves the child data types (non-MenuClass types) from the API and generates routes for each.
 * @param {VueRouter} router The SPA framework's VueRouter instance.
 * @param {string} apiVer The current API version.
 */
export function getChildItems(router: VueRouter, apiVer: string, culture: string): Promise<void> {
    return Api.callApi('api/Data/GetChildTypes',
        {
            method: 'GET',
            headers: {
                'Accept': `application/json;v=${apiVer}`,
                'Accept-Language': culture
            }
        })
        .then(response => {
            if (!response.ok) {
                throw Error(response.statusText);
            }
            return response;
        })
        .then(response => response.json() as Promise<Array<any>>)
        .then(data => {
            for (var dataClass in data) {
                router.addRoutes([{
                    path: `/data/${dataClass.toLowerCase()}`,
                    meta: { requiresAuthorize: true },
                    component: require('../components/data/dashboard.vue').default,
                    props: {
                        title: dataClass,
                        iconClass: data[dataClass].iconClass || 'view_list',
                        fontAwesome: data[dataClass].fontAwesome
                    },
                    children: [
                        {
                            name: dataClass + "DataTable",
                            path: 'table',
                            components: {
                                content: data[dataClass].dashboardTableContent
                                    ? require(`../components/data/${data[dataClass].dashboardTableContent}.vue`).default
                                    : require('../components/data/empty.vue').default,
                                data: require('../dynamic-data/dynamic-table/dynamic-table.vue').default
                            },
                            props: { content: false, data: true }
                        },
                        {
                            name: dataClass,
                            path: ':operation/:id',
                            components: {
                                content: data[dataClass].dashboardFormContent
                                    ? require(`../components/data/${data[dataClass].dashboardFormContent}.vue`).default
                                    : require('../components/data/empty.vue').default,
                                data: require('../dynamic-data/dynamic-form/dynamic-form.vue').default
                            },
                            props: { content: true, data: true }
                        }
                    ]
                }]);
            }
        })
        .catch(error => {
            ErrorMsg.logError("uiStore.getChildItems", new Error(error));
        });
}

/**
 * Retrieves the MenuClass data types from the API and generates routes and MenuItems for each.
 * @param {VueRouter} router The SPA framework's VueRouter instance.
 * @param {string} apiVer The current API version.
 * @param {MenuItem} menu The top-level MenuItem under which all data types will be added.
 */
export function getMenuItems(router: VueRouter, apiVer: string, culture: string, menu: MenuItem): Promise<void> {
    return Api.callApi('api/Data/GetTypes',
        {
            method: 'GET',
            headers: {
                'Accept': `application/json;v=${apiVer}`,
                'Accept-Language': culture
            }
        })
        .then(response => {
            if (!response.ok) {
                throw Error(response.statusText);
            }
            return response;
        })
        .then(response => response.json() as Promise<any>)
        .then(data => {
            for (var dataClass in data) {
                addMenuItem(menu, router, data, dataClass, data[dataClass].category, data[dataClass].iconClass);
            }
        })
        .catch(error => {
            ErrorMsg.logError("uiStore.getMenuItems", new Error(error));
        });
}