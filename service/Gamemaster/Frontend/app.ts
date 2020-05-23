import { createApp} from 'vue';
import App from './App.vue';
import router from './router';
import { SignalRContext } from "./scripts/SignalRhelper";

const context = SignalRContext.getInstance();
const app = createApp(App);
app.use(router);
app.mount('#app');

