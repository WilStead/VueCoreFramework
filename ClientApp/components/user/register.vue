<template>
    <div class="user-container">
        <div class="user-form">
            <h4>Register for a New Account</h4>
            <ul class="text-danger">
                <li v-for="error in model.errors">{{ error }}</li>
            </ul>
            <vue-form-generator :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            <div class="submit-row">
                <router-link tag="button" class="btn btn-default" :to="{ path: '/login', query: { returnUrl }}">Cancel</router-link>
                <button class="btn btn-primary" @click.stop.prevent="onSubmit">Register</button>
            </div>
        </div>
    </div>
</template>

<script src="./register.ts"></script>