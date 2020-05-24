<template>
    <router-link :to="{name: 'TokenDetails', params: {id:data.UUID} }">
        <tr>
            <td>{{data.name}}</td>
            <td>{{data.description}}</td>
        </tr></router-link>
</template>

<script lang="ts">
    import { defineComponent, ref } from "vue";
    export default defineComponent({
        props: ['data'],
        setup(props) {
            console.log(props['data'].id);
            return;
        }
    });
</script>

<style scoped>
</style>