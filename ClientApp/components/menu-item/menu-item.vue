<template>
    <v-divider v-if="menuItem.divider"></v-divider>
    <v-subheader v-else-if="menuItem.header" v-text="menuItem.text"></v-subheader>
    <v-list-group v-else :value="active" @click.native.stop="toggle">
        <v-list-tile slot="item" :router="menuItem.route !== undefined" :href="menuItem.route">
            <v-list-tile-action><v-icon light :fa="menuItem.fontAwesome">{{ menuItem.iconClass }}</v-icon></v-list-tile-action>
            <v-list-tile-content><v-list-tile-title>{{ menuItem.text }}</v-list-tile-title></v-list-tile-content>
            <v-list-tile-action v-if="submenu && submenu.length"><v-icon light>expand_more</v-icon></v-list-tile-action>
        </v-list-tile>
        <template v-for="subItem in submenu">
            <menu-item-component :menuItem="subItem" :submenu="subItem.submenu" />
        </template>
    </v-list-group>
</template>

<script src="./menu-item.ts"></script>