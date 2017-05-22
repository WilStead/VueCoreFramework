<template>
    <dynamic-table-component class="main-content"
                             title="Countries"
                             route-name="country"
                             repository="$store.state.countryData"
                             vm-definition="countryDefs"></dynamic-table-component>
</template>

<script src="./maintenance.ts"></script>