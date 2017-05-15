<template>
    <nav class="main-nav">
        <ul>
            <li class="menu-listing" v-for="menuItem in $store.state.uiState.menuItems">
                <menu-item-component :menuItem="menuItem" :vertical="vertical" />
            </li>
        </ul>
    </nav>
</template>

<script src="./navmenu.ts"></script>

<style src="./navmenu.scss" lang="scss"></style>
