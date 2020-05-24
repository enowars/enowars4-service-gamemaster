import { createRouter, createWebHistory } from 'vue-router'
import Home from './subvues/Home.vue'
import Login from './subvues/Login.vue'
import Register from './subvues/Register.vue'
import Session from './subvues/Session.vue'
import SessionList from './subvues/SessionList.vue'
import AccountSettings from './subvues/AccountSettings.vue'
import SessionDetails from './subvues/SessionDetails.vue'
import Tokens from './subvues/Tokens.vue'

const routerHistory = createWebHistory()

const router = createRouter({
    history: routerHistory,
    routes: [
        {
            path: '/',
            component: Home
        },
        {
            path: '/login',
            component: Login
        },
        {
            path: '/register',
            component: Register
        },
        {
            path: '/session',
            component: Session
        },       
        {
            path: '/sessionList',
            component: SessionList
        },
        {
            path: '/accountSettings',
            component: AccountSettings
        },
        {
            path: '/sessionDetails/:id',
            name: 'SessionDetails',
            component: SessionDetails
        },
        {
            path: '/tokens',
            component: Tokens
        },
        


        
    ]
})

export default router