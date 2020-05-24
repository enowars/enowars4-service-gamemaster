<template>
    <div>
        SessionList
        <table>
            <thead>
                <tr>
                    <th>Timestamp</th>
                    <th>Name</th>
                    <th>Owner</th>
                </tr>
            </thead>
            <tbody>
                <TokenListElement v-for="d in tabledata" :data="d"></TokenListElement>
            </tbody>
        </table>
    </div>
</template>
<script lang="ts">
    import axios, { AxiosRequestConfig, AxiosPromise, AxiosResponse } from 'axios';
    import { defineComponent, ref } from "vue";
    import TokenListElement from "./TokenListElement.vue";
    export default defineComponent({
        components: {
            TokenListElement
        },
        data() {
            return { tabledata: [] };
        },
        mounted() {
            console.log("Setup");
            console.log(this);
            const options: AxiosRequestConfig = {
                method: 'GET',
                params: { "take": 100, "skip": 0 },
                headers: { 'Content-Type': 'x-www-form-urlencoded' },
                url: '/api/token/list',
            };
            axios(options).then(
                response => {
                    console.log(response);
                    console.log(this);
                    this.tabledata = response.data;
                });
            return;
        }
    });
</script>