<template>
    <header>
        <div class="header-container" v-responsiveness="['hidden-all', 'lg', 'xl']">
            <div class="logo-container"></div>
            <div class="menu-container">
                <topbar-component />
                <navmenu-component />
            </div>
        </div>
        <div class="header-container header-container-small" v-responsiveness="['hidden-lg', 'hidden-xl']">
            <a href="#" title="Toggle Menu" v-on:click.stop.prevent="toggleVerticalMenu"><span class="glyphicon glyphicon-menu-hamburger"></span></a>
            <div class="logo-container logo-container-small"></div>
            <span class="glyphicon glyphicon-option-vertical"></span>
        </div>
    </header>
</template>

<script src="./header.ts"></script>

<style src="./header.scss" lang="scss"></style>