import VueRouter from 'vue-router';
import * as ErrorMsg from '../../error-msg';
import { Repository } from '../repository';

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
     * An optional route name which will be navigated to when the menu item is selected.
     */
    route?: string;

    /**
     * An optional collection of sub-items contained within this menu item.
     */
    submenu?: Array<MenuItem>;
}

function addMenuItem(menu: MenuItem, router: VueRouter, name: string, fullCategory: string, category: string, iconClass: string) {
    let lowerName = name.toLowerCase();
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
                iconClass,
                submenu: []
            };
            menu.submenu.push(menuItem);
        }
        addMenuItem(menuItem, router, name, fullCategory, category, iconClass);
    } else {
        let baseRoute = `/data/${lowerName}`;
        let tableRoute = baseRoute + '/table';

        router.addRoutes([{
            path: baseRoute,
            meta: { requiresAuth: true },
            component: require('../../components/data/dashboard.vue'),
            props: { title: name },
            children: [
                {
                    name: name + "DataTable",
                    path: 'table/:operation?/:parentType?/:parentId?/:parentProp?/:childProp?',
                    component: require('../../dynamic-data/dynamic-table/dynamic-table.vue'),
                    props: true
                },
                {
                    name: name,
                    path: ':operation/:id',
                    component: require('../../dynamic-data/dynamic-form/dynamic-form.vue'),
                    props: true
                }
            ]
        }]);

        let menuItem: MenuItem = null;
        if (menu.submenu && menu.submenu.length) {
            for (var i = 0; i < menu.submenu.length; i++) {
                if (menu.submenu[i].text === name) {
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
                text: name,
                iconClass,
                route: tableRoute
            };
            menu.submenu.push(menuItem);
        } else {
            menuItem.iconClass = iconClass;
            menuItem.route = tableRoute;
        }
    }
}

/**
 * Retrieves the child data types (non-MenuClass types) from the API and generates routes for each.
 * @param {VueRouter} router The SPA framework's VueRouter instance.
 */
export function getChildItems(router: VueRouter): Promise<void> {
    return fetch('/api/Data/GetChildTypes')
        .then(response => {
            if (!response.ok) {
                throw Error(response.statusText);
            }
            return response;
        })
        .then(response => response.json() as Promise<Array<string>>)
        .then(data => {
            for (var i = 0; i < data.length; i++) {
                router.addRoutes([{
                    path: `/data/${data[i].toLowerCase()}`,
                    meta: { requiresAuth: true },
                    component: require('../../components/data/dashboard.vue'),
                    props: { title: data[i] },
                    children: [
                        {
                            name: data[i] + "DataTable",
                            path: 'table/:operation?/:parentType?/:parentId?/:parentProp?/:childProp?',
                            component: require('../../dynamic-data/dynamic-table/dynamic-table.vue'),
                            props: true
                        },
                        {
                            name: data[i],
                            path: ':operation/:id',
                            component: require('../../dynamic-data/dynamic-form/dynamic-form.vue'),
                            props: true
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
 * @param {MenuItem} menu The top-level MenuItem under which all data types will be added.
 */
export function getMenuItems(router: VueRouter, menu: MenuItem): Promise<void> {
    return fetch('/api/Data/GetTypes')
        .then(response => {
            if (!response.ok) {
                throw Error(response.statusText);
            }
            return response;
        })
        .then(response => response.json() as Promise<any>)
        .then(data => {
            for (var dataClass in data) {
                addMenuItem(menu, router, dataClass, data[dataClass].category, data[dataClass].category, data[dataClass].iconClass);
            }
        })
        .catch(error => {
            ErrorMsg.logError("uiStore.getMenuItems", new Error(error));
        });
}

/**
 * An object containing information about the state of the UI.
 */
export const uiState = {
    /**
     * The MenuItems displayed in the SPA framework's main menu.
     */
    menuItems: <MenuItem[]>[
        {
            text: 'Home',
            iconClass: 'home',
            route: '/'
        },
        {
            text: 'Data',
            iconClass: 'view_list'
        },
        {
            text: 'Fetch data',
            iconClass: 'wb_cloudy',
            route: '/fetchdata'
        }
    ]
};