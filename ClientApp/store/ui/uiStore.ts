import { countryData } from '../../viewmodels/country';

export interface MenuItem {
    text: string,
    iconClass: string,
    route: string,
    submenu: Array<MenuItem>
}

export const uiState = {
    menuItems: <MenuItem[]>[
        {
            text: 'Home',
            iconClass: 'home',
            route: '/'
        },
        {
            text: 'Countries',
            iconClass: 'public',
            route: '/countries/table'
        },
        {
            text: 'Fetch data',
            iconClass: 'wb_cloudy',
            route: '/fetchdata'
        }
    ]
};