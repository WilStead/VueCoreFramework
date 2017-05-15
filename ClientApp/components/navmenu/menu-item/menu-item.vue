<template>
    <div @mouseenter="onItemMouseEnter" @mouseleave="onItemMouseLeave" @click.stop="toggle">
        <router-link v-if="menuItem.route !== undefined" class="menu-item" :to="menuItem.route" :exact="menuItem.routeExact">
            <i :class="menuItem.iconClasses"></i><span>{{ menuItem.text }}</span><span v-if="menuItem.submenu && menuItem.submenu.length"><i class="glyphicon" v-bind:class="chevronDirection"></i></span>
        </router-link>
        <div v-if="menuItem.route === undefined" class="menu-item">
            <i :class="menuItem.iconClasses"></i><span>{{ menuItem.text }}</span><span v-if="menuItem.submenu && menuItem.submenu.length"><i class="glyphicon" v-bind:class="chevronDirection"></i></span>
        </div>
        <transition name="fade">
            <popup-menu-component v-if="menuItem.submenu && menuItem.submenu.length && (mouseInPopup || mouseInItem || manualShow)"
                                  :menuItems="menuItem.submenu"
                                  :vertical="vertical"
                                  @mouseenter="onPopupMouseEnter"
                                  @mouseleave="onPopupMouseLeave"
                                  :style="{ left: popupLeft, top: popupTop }" />
        </transition>
    </div>
</template>

<script src="./menu-item.ts"></script>

<style src="./menu-item.scss" lang="scss"></style>