import Vue from 'vue';
import Vuex from 'vuex';
import VuexPersistence from 'vuex-persist';
import VuexORM from '@vuex-orm/core';

import Tweet from '@/store/Tweet/index';
import User from '@/store/User/index';

const vuexLocal = new VuexPersistence({
  key: 'twitter-demo',
  storage: localStorage,
});

const database = new VuexORM.Database();

database.register(Tweet);
database.register(User);

window.Tweet = Tweet;
window.User = User;

Vue.use(Vuex);

export default new Vuex.Store({
  state: {
  },
  mutations: {
  },
  actions: {
  },
  plugins: [VuexORM.install(database), vuexLocal.plugin],
});
