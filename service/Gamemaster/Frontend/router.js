"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
const vue_router_1 = require("vue-router");
const Home_vue_1 = require("./subvues/Home.vue");
const Login_vue_1 = require("./subvues/Login.vue");
const Register_vue_1 = require("./subvues/Register.vue");
const Session_vue_1 = require("./subvues/Session.vue");
const SessionList_vue_1 = require("./subvues/SessionList.vue");
const AccountSettings_vue_1 = require("./subvues/AccountSettings.vue");
const SessionDetails_vue_1 = require("./subvues/SessionDetails.vue");
const TokenAdd_vue_1 = require("./subvues/TokenAdd.vue");
const routerHistory = vue_router_1.createWebHistory();
const router = vue_router_1.createRouter({
    history: routerHistory,
    routes: [
        {
            path: '/',
            component: Home_vue_1.default
        },
        {
            path: '/login',
            component: Login_vue_1.default
        },
        {
            path: '/register',
            component: Register_vue_1.default
        },
        {
            path: '/session',
            component: Session_vue_1.default
        },
        {
            path: '/sessionList',
            component: SessionList_vue_1.default
        },
        {
            path: '/accountSettings',
            component: AccountSettings_vue_1.default
        },
        {
            path: '/sessionDetails/:id',
            name: 'SessionDetails',
            component: SessionDetails_vue_1.default
        },
        {
            path: '/tokenAdd',
            component: TokenAdd_vue_1.default
        },
    ]
});
exports.default = router;
//# sourceMappingURL=router.js.map