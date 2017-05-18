<template>
    <div>
        <h3>Country</h3>
        <dynamic-form-component :vm="country"
                                vmDefinition="countryDefinition"
                                operation="operation"
                                :errorMessage="errorMessage"
                                @update="updateCountry($event)"
                                @create="createCountry($event)"></dynamic-form-component>
    </div>
</template>

<script src="./details.ts"></script>