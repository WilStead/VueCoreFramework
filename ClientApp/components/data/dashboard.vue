<template>
    <div class="main-content">
        <h4><v-icon fa="fontAwesome" dark>{{ iconClass }}</v-icon><span>{{ title }}</span></h4>
        <router-view name="content"></router-view>
        <router-view name="data"></router-view>
    </div>
</template>

<script src="./dashboard.ts"></script>