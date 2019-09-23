import Vue from 'vue';
import Buefy from 'buefy';
import axios from 'axios';
import VueAxios from 'vue-axios';
import VueLazyload from 'vue-lazyload';
import twemoji from 'twemoji';
import App from './App.vue';
import router from './router';
import store from './store';
import 'buefy/dist/buefy.css';

Vue.use(Buefy, {
  defaultIconPack: 'fas',
});

Vue.use(VueAxios, axios);

Vue.use(VueLazyload);

Vue.config.productionTip = false;

Vue.directive('emoji', {
  inserted(el) {
    // eslint-disable-next-line no-param-reassign
    el.innerHTML = twemoji.parse(el.innerHTML);
  },
});

new Vue({
  router,
  store,
  render: h => h(App),
}).$mount('#app');
