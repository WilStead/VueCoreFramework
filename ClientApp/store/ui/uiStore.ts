import * as ErrorMsg from '../../error-msg';
import { Repository } from '../repository';

export interface MenuItem {
    text: string,
    iconClass: string,
    route: string,
    submenu: Array<MenuItem>
}

function addMenuItem(menu: Array<any>, router: any, name: string, entityName: string, fullCategory: string, category: string, iconClass: string) {
    if (!iconClass) {
        iconClass = 'view_list';
    }
    let index = category.indexOf('/');
    if (index === 0 && category.length > 1) {
        index = category.substring(1).indexOf('/'); // skip first if it's not the only character
    }
    let currentCategory = index <= 0 ? '' : category.substring(0, index);
    if (currentCategory) {
        let menuItem = null;
        for (var i = 0; i < menu.length; i++) {
            if (menu[i].text === currentCategory) {
                menuItem = menu[i];
                break;
            }
        }
        if (!menuItem) {
            menu.push({
                text: currentCategory,
                iconClass,
                submenu: []
            });
        }
        if (!menuItem.submenu) {
            menuItem.submenu = [];
        }
        addMenuItem(menuItem.submenu, router, name, entityName, fullCategory, category.substring(index + 1), iconClass);
    } else {
        let baseRoute = (fullCategory !== undefined && fullCategory !== null && fullCategory.length > 0 && fullCategory !== '/')
            ? `/data/${fullCategory}/${name}`
            : `/data/${name}`;
        let tableRoute = baseRoute + '/table';
        let repository = new Repository(entityName);

        router.addRoutes([{
            path: baseRoute,
            meta: { requiresAuth: true },
            component: require('../../components/data/dashboard.vue'),
            props: { title: name },
            children: [
                {
                    path: 'table',
                    component: require('../../dynamic-data/dynamic-table/dynamic-table.vue'),
                    props: {
                        routeName: name,
                        repository
                    }
                },
                {
                    name: name,
                    path: ':operation/:id',
                    component: require('../../dynamic-data/dynamic-form/dynamic-form.vue'),
                    props: (route) => ({
                        id: route.params.id,
                        operation: route.params.operation,
                        repository,
                        routeName: name
                    })
                }
            ]
        }]);

        menu.push({
            text: name,
            iconClass,
            route: tableRoute
        });
    }
}

export function getMenuItems(router: any): Promise<any[]> {
    return fetch('/api/Data/GetTypes')
        .then(response => {
            if (!response.ok) {
                throw Error(response.statusText);
            }
            return response;
        })
        .then(response => response.json() as Promise<any>)
        .then(data => {
            let menuItems = [];
            for (var dataClass in data) {
                addMenuItem(menuItems, router, dataClass, data[dataClass].entityName, data[dataClass].category, data[dataClass].category, data[dataClass].iconClass);
            }
            return menuItems;
        })
        .catch(error => {
            ErrorMsg.logError("uiStore.getMenuItems", error);
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