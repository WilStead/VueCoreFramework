<template>
    <ul class="popup-menu">
        <li class="menu-listing popup" v-for="menuItem in menuItems">
            <menu-item-component :menuItem="menuItem" :inPopup="true" :vertical="vertical" />
        </li>
    </ul>
</template>

<script src="./popup-menu.ts"></script>

<style src="./popup-menu.scss" lang="scss"></style>