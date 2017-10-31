<template>
    <v-card>
        <v-alert color="error" :value="errorMessage">{{ errorMessage }}</v-alert>
        <dynamic-data-table :allowEdit="true"
                            :dataType="$route.name.substr(0, $route.name.length - 9)"
                            :search="$route.query.search"
                            @onError="onError"
                            @onPagination="onPagination"></dynamic-data-table>
    </v-card>
</template>

<script src="./dynamic-table.ts"></script>