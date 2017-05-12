<template>
    <div id='app-root'>
        <header-component />
        <main>
            <navmenu-component class="navmenu-v" v-responsiveness="['hidden-lg', 'hidden-xl']" />
            <router-view></router-view>
        </main>
        <footer-component />
    </div>
</template>

<script src="./app.ts"></script>

<style src="../../vue-responsive-custom.css"></style>
<style src="./app.scss" lang="scss"></style>
