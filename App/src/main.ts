import '@/assets/main.css';

import { createApp } from 'vue';
import App from './App.vue';
import router from './router';
import PrimeVue from 'primevue/config';
import ToastService from 'primevue/toastservice';
import 'primevue/resources/themes/viva-dark/theme.css';

const app = createApp(App);

app.use(router);
app.use(PrimeVue, { ripple: true });
app.use(ToastService);

app.mount('#app');
