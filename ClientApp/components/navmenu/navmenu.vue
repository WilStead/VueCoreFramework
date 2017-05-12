<template>
    <nav class="main-nav">
        <ul>
            <li class="menu-listing" v-for="menuItem in uiState.menuItems">
                <router-link class="menu-item" :to="menuItem.route" :exact="menuItem.routeExact">
                    <i :class="menuItem.iconClasses"></i><span>{{ menuItem.text }}</span>
                </router-link>
            </li>
        </ul>
    </nav>
</template>

<script src="./navmenu.ts"></script>

<style src="./navmenu.scss" lang="scss"></style>
