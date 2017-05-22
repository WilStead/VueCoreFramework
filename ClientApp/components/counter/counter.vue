<template>
    <v-layout row wrap>
        <v-flex xs10 offset-xs1>
            <h1>Counter</h1>

            <p>This is a simple example of a Vue.js component.</p>

            <p>Current count: <strong>{{ $store.state.count }}</strong></p>

            <v-btn primary @click.native="increment">Increment</v-btn>
        </v-flex>
    </v-layout>
</template>

<script src="./counter.ts"></script>
