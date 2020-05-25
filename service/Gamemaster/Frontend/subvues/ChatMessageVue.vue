<template>
        <tr>
            <td>{{data.senderName}}</td>
            <td>{{data.timestamp}}</td>
            <td>{{data.content}}</td>
        </tr>
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