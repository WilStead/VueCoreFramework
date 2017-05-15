<template>
    <div>
        <h1>Counter</h1>

        <p>This is a simple example of a Vue.js component.</p>

        <p>Current count: <strong>{{ $store.state.count }}</strong></p>

        <button @click="increment">Increment</button>
    </div>
</template>

<script src="./counter.ts"></script>
