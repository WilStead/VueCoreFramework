import * as ErrorMsg from '../../error-msg';
import { Repository } from '../repository';

export interface MenuItem {
    text: string,
    iconClass: string,
    route?: string,
    submenu?: Array<MenuItem>
}

function addMenuItem(menu: MenuItem, router: any, name: string, fullCategory: string, category: string, iconClass: string) {
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
                    name: lowerName + "DataTable",
                    path: 'table/:operation?/:parentType?/:parentId?/:parentProp?/:childProp?',
                    component: require('../../dynamic-data/dynamic-table/dynamic-table.vue'),
                    props: (route) => ({
                        childProp: route.params.childProp,
                        operation: route.params.operation,
                        parentId: route.params.parentId,
                        parentProp: route.params.parentProp,
                        parentType: route.params.parentType,
                        repositoryType: name,
                        routeName: lowerName
                    })
                },
                {
                    name: lowerName,
                    path: ':operation/:id/:parentType?/:parentId?/:parentProp?/:childProp?',
                    component: require('../../dynamic-data/dynamic-form/dynamic-form.vue'),
                    props: (route) => ({
                        childProp: route.params.childProp,
                        id: route.params.id,
                        operation: route.params.operation,
                        parentId: route.params.parentId,
                        parentProp: route.params.parentProp,
                        parentType: route.params.parentType,
                        repositoryType: name,
                        routeName: lowerName
                    })
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

export function getChildItems(router: any): Promise<void> {
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
                let name = data[i].toLowerCase();
                router.addRoutes([{
                    path: `/data/${name}`,
                    meta: { requiresAuth: true },
                    component: require('../../components/data/dashboard.vue'),
                    props: { title: data[i] },
                    children: [{
                        name,
                        path: ':operation/:id/:parentType?/:parentId?/:parentProp?/:childProp?',
                        component: require('../../dynamic-data/dynamic-form/dynamic-form.vue'),
                        props: (route) => ({
                            childProp: route.params.childProp,
                            id: route.params.id,
                            operation: route.params.operation,
                            parentId: route.params.parentId,
                            parentProp: route.params.parentProp,
                            parentType: route.params.parentType,
                            repositoryType: data[i],
                            routeName: name
                        })
                    }]
                }]);
            }
        })
        .catch(error => {
            ErrorMsg.logError("uiStore.getChildItems", new Error(error));
        });
}

export function getMenuItems(router: any, menu: MenuItem): Promise<void> {
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

export const uiState = {
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