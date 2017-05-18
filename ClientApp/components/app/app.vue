<template>
    <div id='app-root'>
        <header-component />
        <main>
            <div class="navmenu-v-container" v-responsiveness="['hidden-lg', 'hidden-xl']">
                <transition name="slide-left">
                    <navmenu-component class="navmenu-v" v-if="$store.state.uiState.verticalMenuShown" :vertical="true" />
                </transition>
            </div>
            <router-view></router-view>
        </main>
        <footer-component />
    </div>
</template>

<script src="./app.ts"></script>

<style src="../../vue-responsive-custom.css"></style>
<style src="./app.scss" lang="scss"></style>
<style src="../common.scss" lang="scss"></style>
<style src="../vue-form-generator-custom.scss" lang="scss"></style>
