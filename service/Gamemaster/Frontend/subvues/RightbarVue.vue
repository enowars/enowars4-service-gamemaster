<template>
    <div id="rightbar" class="rightbar">

    </div>
</template>
<script lang="ts">
    import { defineComponent, ref, reactive, readonly } from 'vue';
    import { gmState } from "../store/gmstate";
    import router from '../router';
    import axios, { AxiosRequestConfig, AxiosPromise, AxiosResponse } from 'axios';
    export default defineComponent({
        props: ['sessionId'],
        components: {
        },
        data() {
            return { tokendata: [] };
        },
        mounted() {

        }

    });

</script>

<style scoped>
    .rightbar {
        height: 100%;
        width: 20%;
        z-index: -1;
        overflow-x: hidden;
        margin-left: 80%;
        padding: 60px 0px 60px 0px;
        transition: 0.5s;
    }
    @media screen and (max-height: 450px) {
        .rightbar {
            padding-top: 5px;
        }

            .rightbar a {
                font-size: 18px;
            }
    }
</style>