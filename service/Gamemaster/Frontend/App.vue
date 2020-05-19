<template>
    <div class="container">
        <div id='nav'>
            <h1>Gamemaster Service</h1>
            <router-link to='/'>Home</router-link>
            <router-link to='/login'>Login</router-link>
            <router-link to='/register'>Register</router-link>
            <router-link to='/sessionList'>Sessions</router-link>
        </div>
        <router-view />
    </div>
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

<style scoped>

    #root {
        text-align: center;
        font-family: 'Lucida Sans', 'Lucida Sans Regular', 'Lucida Grande', 'Lucida Sans Unicode', Geneva, Verdana, sans-serif;
    }

    img {
        width: 200px;
    }
    h1 {
        display: inline;
    }
    #nav {
        font-size: 1.5em;
        margin-bottom: 30px;
    }

    a {
        text-decoration: none;
        margin: 30px 25px;
        color: #333;
    }

        a:hover {
            text-decoration: underline;
            color:
        }
</style>
