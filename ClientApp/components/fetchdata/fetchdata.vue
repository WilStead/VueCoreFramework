<template>
    <v-layout row wrap>
        <v-flex xs10 offset-xs1>
            <h1>Weather forecast</h1>

            <p>This component demonstrates fetching data from the server.</p>

            <table v-if="forecasts.length" class="table">
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Temp. (C)</th>
                        <th>Temp. (F)</th>
                        <th>Summary</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="item in forecasts">
                        <td>{{ item.dateFormatted }}</td>
                        <td>{{ item.temperatureC }}</td>
                        <td>{{ item.temperatureF }}</td>
                        <td>{{ item.summary }}</td>
                    </tr>
                </tbody>
            </table>

            <p v-else><em>Loading...</em></p>
        </v-flex>
    </v-layout>
</template>

<script src="./fetchdata.ts"></script>
