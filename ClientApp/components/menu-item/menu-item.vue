<template>
    <v-list-group v-model="active" @click.native.stop="toggle">
        <v-list-tile slot="item" :router="menuItem.route" :href="menuItem.route">
            <v-list-tile-action><v-icon>{{ menuItem.iconClass }}</v-icon></v-list-tile-action>
            <v-list-tile-content><v-list-tile-title>{{ menuItem.text }}</v-list-tile-title></v-list-tile-content>
            <v-list-tile-action v-if="menuItem.submenu && menuItem.submenu.length"><v-icon>expand_more</v-icon></v-list-tile-action>
        </v-list-tile>
        <template v-for="subItem in menuItem.submenu">
            <menu-item-component :menuItem="subItem" />
        </template>
    </v-list-group>
</template>

<script src="./menu-item.ts"></script>