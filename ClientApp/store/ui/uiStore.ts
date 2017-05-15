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
                iconClasses: ['glyphicon', 'glyphicon-expand'],
                submenu: [
                    {
                        text: 'USA',
                        iconClasses: ['glyphicon', 'glyphicon-flag'],
                        route: '/countries/USA'
                    },
                    {
                        text: 'India',
                        iconClasses: ['glyphicon', 'glyphicon-flag'],
                        route: '/countries/India'
                    },
                    {
                        text: 'Switzerland',
                        iconClasses: ['glyphicon', 'glyphicon-flag'],
                        route: '/countries/Switzerland'
                    }
                ]
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

export const uiState = {
    menuItems,
    verticalMenuShown: false
};