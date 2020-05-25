import { createApp} from 'vue';
import App from './App.vue';
import router from './router';
import Framevuerk from 'framevuerk/dist/framevuerk.js';
import 'framevuerk/dist/framevuerk.css';
const app = createApp(App);
app.use(router);
app.use(Framevuerk);
app.mount('#app');
