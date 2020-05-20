<template>
    <tr><td>{{data.name}}</td><td>{{data}}</td></tr>
</template>

<script lang="ts">
    import { defineComponent, ref } from "vue";
    export default defineComponent({
        props: ['data'],
        setup() {
            return;
        }
    });
</script>

<style scoped>
</style>