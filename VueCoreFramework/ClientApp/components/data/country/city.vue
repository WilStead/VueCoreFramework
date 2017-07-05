<template>
    <v-card>
        <v-alert error :value="errorMessage">{{ errorMessage }}</v-alert>
        <v-card-row>
            <v-card-text>
                <v-checkbox v-model="isCapitol"
                            label="Is Capitol"
                            :disabled="otherCapitol || operation === 'view'"
                            hint="Changes to this value take effect immediately, not on Save"
                            persistent-hint></v-checkbox>
            </v-card-text>
        </v-card-row>
    </v-card>
</template>

<script src="./city.ts"></script>