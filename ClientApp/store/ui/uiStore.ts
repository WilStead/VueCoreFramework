import { countryData } from '../../viewmodels/country';

export interface MenuItem {
    text: string,
    iconClass: string,
    route: string,
    routeExact: boolean,
    submenu: Array<MenuItem>
}

const menuItems = <MenuItem[]>[
    {
        text: 'Home',
        iconClass: 'home',
        route: '/',
        routeExact: true
    },
    {
        text: 'Countries',
        iconClass: 'public',
        submenu: [
            {
                text: 'Dashboard',
                iconClass: 'view_list',
                route: '/countries'
            },
            {
                text: 'Select',
                iconClass: 'trending_flat'
            },
            {
                text: 'Top 3',
                iconClass: 'public',
                route: '/countries/list/3'
            },
            {
                text: 'Top 10',
                iconClass: 'public',
                route: '/countries/list/10'
            },
            {
                text: 'All',
                iconClass: 'public',
                route: '/countries/list/0'
            },
            {
                text: 'Maintenance',
                iconClass: 'build',
                route: '/countries/maintenance'
            }
        ]
    },
    {
        text: 'Default',
        iconClass: 'reorder',
        submenu: [
            {
                text: 'Counter',
                iconClass: 'account_balance',
                route: '/counter'
            },
            {
                text: 'Fetch data',
                iconClass: 'wb_cloudy',
                route: '/fetchdata'
            }
        ]
    }
];

function getMenuItems(): Array<MenuItem> {
    countryData.getAll()
        .then(data => {
            menuItems[1].submenu[1].submenu = data.map(c => {
                return {
                    text: c.name,
                    iconClass: 'public',
                    route: `/countries/${c.id}`,
                    routeExact: false,
                    submenu: []
                }
            });
        });
    return menuItems;
}

export const uiState = {
    menuItems: getMenuItems()
};