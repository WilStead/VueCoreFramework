<template>
    <div>
        <h3>Country Maintenance</h3>
        <button class="btn btn-primary" @click.stop.prevent="createCountry">Add New Country</button>
        <table>
            <thead>
                <tr>
                    <th style="width: 20rem">Country</th>
                    <th style="width: 7rem">EPI Index</th>
                    <th style="min-width: 10rem"></th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="row in countries">
                    <td>{{ row.name }}</td>
                    <td>{{ row.epiIndex }}</td>
                    <td v-if="row.id != deleteId">
                        <button class="btn btn-sm btn-primary" @click.stop.prevent="showCountryDetail(row.id)"><i class="fa fa-info" aria-hidden="true"></i><span class="sr-only">Details</span></button>
                        <button class="btn btn-sm btn-warning" @click.stop.prevent="editCountry(row.id)"><i class="fa fa-pencil" aria-hidden="true"></i><span class="sr-only">Edit</span></button>
                        <button class="btn btn-sm btn-danger" @click.stop.prevent="deleteCountryQuestion(row.id)"><i class="fa fa-trash" aria-hidden="true"></i><span class="sr-only">Delete</span></button>
                    </td>
                    <td v-if="row.id == deleteId && !isDeleting">
                        <span>Delete this country?</span>
                        <button class="btn btn-sm" @click.stop.prevent="cancelDelete()"><i class="fa fa-ban" aria-hidden="true"></i><span class="sr-only">Cancel</span></button>
                        <button class="btn btn-sm btn-danger" @click.stop.prevent="deleteCountry(row.id)"><i class="fa fa-trash" aria-hidden="true"></i><span class="sr-only">Delete</span></button>
                    </td>
                    <td v-if="isDeleting">
                        <span class="submitting">Deleting</span>
                    </td>
                </tr>
            </tbody>
        </table>
    </div>
</template>