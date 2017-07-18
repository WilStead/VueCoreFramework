import Vue from 'vue';
import { Component } from 'vue-property-decorator';
import { Pagination } from './dynamic-data-table';

@Component
export default class DynamicTableComponent extends Vue {
    errorMessage = '';

    onError(error: string) {
        this.errorMessage = error;
    }

    onPagination(search: string, pagination: Pagination) {
        this.$router.push({
            name: this.$route.name,
            params: this.$route.params,
            query: {
                search: search || '',
                sortBy: pagination.sortBy || '',
                descending: pagination.descending.toString(),
                page: pagination.page.toString(),
                rowsPerPage: pagination.rowsPerPage.toString()
            }
        });
    }
}
