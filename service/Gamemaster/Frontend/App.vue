<template>
    <div class="jumbotron">
        <div class="container">
            <div class="row">
                <div class="col-sm-6 offset-sm-3">
                    <div v-if="alert.message" :class="`alert ${alert.type}`">{{alert.message}}</div>
                    <router-view></router-view>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import { mapState, mapActions } from 'vuex'

    export default {
        name: 'app',
        computed: {
            ...mapState({
                alert: state => state.alert
            })
        },
        methods: {
            ...mapActions({
                clearAlert: 'alert/clear'
            })
        },
        watch: {
            $route(to, from) {
                // clear alert on location change
                this.clearAlert();
            }
        }
    };
</script>

<!--







<template>
    <h2>Welcome to the Interactive online Gaming Room!</h2>
    <p>Username</p>
    <input v-model="username" placeholder="Username">
    <p>Password</p>
    <input v-model="password" placeholder="Password">
    <button @click="increase">Clicked {{ count }} times.</button>
</template>
<script lang="ts">
    import { defineComponent, ref } from "vue";
    export default defineComponent({
        setup() {
            const count = ref(0)
            const increase = () => {
                count.value++
            }

            return {
                count,
                increase,
            }
        }
    });
</script>


-->
