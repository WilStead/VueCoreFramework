<template>
    <div class="user-container">
        <div class="user-form">
            <h4>Register for a New Account</h4>
            <ul class="text-danger">
                <li v-for="error in model.errors">{{ error }}</li>
            </ul>
            <vue-form-generator :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            <div v-if="!submitting" class="submit-row">
                <router-link tag="button" class="btn btn-default" :to="{ path: '/login', query: { returnUrl }}">Cancel</router-link>
                <v-btn primary @click.native.stop.prevent="onSubmit">Register</v-btn>
            </div>
            <div v-if="submitting">
                <p class="submitting">Registering</p>
            </div>
        </div>
    </div>
</template>

<script src="./register.ts"></script>

<style src="./user.scss" lang="scss"></style>