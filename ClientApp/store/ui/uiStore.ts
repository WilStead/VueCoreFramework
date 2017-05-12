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
        text: 'Counter',
        iconClasses: ['glyphicon', 'glyphicon-education'],
        route: '/counter'
    },
    {
        text: 'Fetch data',
        iconClasses: ['glyphicon', 'glyphicon-th-list'],
        route: '/fetchdata'
    }
];

export const uiState = {
    menuItems,
    verticalMenuShown: false
};