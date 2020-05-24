<template>
    <div id="tokens">
        Token Manager
        <TokenAdd></TokenAdd>
        <TokenList></TokenList>
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';
    import axios, { AxiosRequestConfig, AxiosPromise, AxiosResponse } from 'axios';
    import router from './../router';
    import { METHODS } from 'http';
    import { gmState } from "../store/gmstate";
    import TokenAdd from "./TokenAdd.vue";
    import TokenList from "./TokenList.vue";
    export default defineComponent({
        components: {
            TokenAdd,
            TokenList
        },
        data() {
            return {
                input: {
                    name: "",
                    description: "",
                    isPrivate: true,
                    file: { type: File }
                }
            }
        },
        methods: {
            handleFileSelection(event: InputEvent) {
                console.log(event);
                const foo = this as any;
                this.input.file = foo.$refs.file.files[0];
            },
            add() {
                var bodyFormData = new FormData();
                bodyFormData.set('name', this.input.name);
                bodyFormData.set('description', this.input.description);
                bodyFormData.set('isPrivate', String(this.input.isPrivate));
                bodyFormData.append("icon", this.input.file as any as Blob);
                console.log("Adding Token...");
                const options: AxiosRequestConfig = {
                    method: 'POST',
                    data: bodyFormData,
                    headers: { 'Content-Type': 'multipart/form-data' },
                    url: '/api/account/AddToken',
                };
                axios(options).then(
                    response => {
                        console.log(response);
                        if (response.status == 200) {
                            console.log("Token Added Successfully");
                            alert("Token Added Successfully");
                            router.push("/");
                        } else {
                            console.log("this should not happen...");
                        }
                    }).catch(error => {
                        console.log("Token Add failed");
                        console.log(error);
                        alert("Token Add failed");
                    })
                return false;
            }
        }
    })
</script>

<style scoped>
    #login {
        width: 500px;
        border: 1px solid #CCCCCC;
        background-color: #FFFFFF;
        margin: auto;
        margin-top: 200px;
        padding: 20px;
    }
</style>