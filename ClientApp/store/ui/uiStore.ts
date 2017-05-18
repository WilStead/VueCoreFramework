import { countryData } from '../../viewmodels/country';

export interface MenuItem {
    text: string,
    iconClasses: Array<string>,
    route: string,
    routeExact: boolean,
    submenu: Array<MenuItem>
}

const menuItems = <MenuItem[]>[
    {
        text: 'Home',
        iconClasses: ['glyphicon', 'glyphicon-home'],
        route: '/',
        routeExact: true
    },
    {
        text: 'Countries',
        iconClasses: ['glyphicon', 'glyphicon-flag'],
        submenu: [
            {
                text: 'Dashboard',
                iconClasses: ['glyphicon', 'glyphicon-expand'],
                route: '/countries'
            },
            {
                text: 'Select',
                iconClasses: ['glyphicon', 'glyphicon-expand']
            },
            {
                text: 'Top 3',
                iconClasses: ['glyphicon', 'glyphicon-flag'],
                route: '/countries/list/3'
            },
            {
                text: 'Top 10',
                iconClasses: ['glyphicon', 'glyphicon-flag'],
                route: '/countries/list/10'
            },
            {
                text: 'All',
                iconClasses: ['glyphicon', 'glyphicon-flag'],
                route: '/countries/list/0'
            },
            {
                text: 'Maintenance',
                iconClasses: ['glyphicon', 'glyphicon-wrench'],
                route: '/countries/maintenance'
            }
        ]
    },
    {
        text: 'Default',
        iconClasses: ['glyphicon', 'glyphicon-wrench'],
        submenu: [
            {
                text: 'Counter',
                iconClasses: ['glyphicon', 'glyphicon-education'],
                route: '/counter'
            },
            {
                text: 'Fetch data',
                iconClasses: ['glyphicon', 'glyphicon-th-list'],
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
                    iconClasses: ['glyphicon', 'glyphicon-flag'],
                    route: `/countries/${c.id}`,
                    routeExact: false,
                    submenu: []
                }
            });
        });
    return menuItems;
}

export const uiState = {
    menuItems: getMenuItems(),
    verticalMenuShown: false
};