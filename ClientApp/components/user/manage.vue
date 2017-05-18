<template>
    <div class="user-container">
        <div class="user-form">
            <h4>Manage your account:</h4>
            <ul class="text-danger">
                <li v-for="error in model.errors">{{ error }}</li>
            </ul>
            <vue-form-generator :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            <div v-if="changingEmail || changingPassword || settingPassword" class="submit-row">
                <button class="btn btn-default" @click.stop.prevent="cancelChange">Cancel</button>
                <button class="btn btn-primary" @click.stop.prevent="onSubmit">Submit</button>
            </div>
            <div v-if="!changingEmail && !changingPassword && !settingPassword">
                <a href="#" @click.stop.prevent="changeEmail">Change email</a>
            </div>
            <div v-if="pendingEmailChange && !changingEmail && !changingPassword && !settingPassword">
                <a href="#" @click.stop.prevent="cancelEmailChange">Cancel pending email change</a>
            </div>
            <div v-if="hasPassword && !changingEmail">
                <a href="#" @click.stop.prevent="changePassword">Change password</a>
            </div>
            <div v-if="!hasPassword && !changingEmail">
                <a href="#" @click.stop.prevent="setPassword">Add a local password</a>
            </div>
            <div>
                <!--TODO: manage external logins-->
            </div>
        </div>
    </div>
</template>

<script src="./manage.ts"></script>