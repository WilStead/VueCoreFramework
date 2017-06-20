<template>
    <div class="main-content">
        <h4><v-icon dark class="secondary--text" :fa="fontAwesome">{{ iconClass }}</v-icon><span>{{ title }}</span></h4>
        <router-view name="content"></router-view>
        <router-view name="data"></router-view>
    </div>
</template>

<script src="./dashboard.ts"></script>