<template>
  <div
    id="app"
    :style="cssVars"
  >
    <div class="columns">
      <div class="column is-3 left-bar">
        <Sidebar />
      </div>
      <div class="column is-5 main">
        <div v-if="ready">
          <router-view />
        </div>
      </div>
      <div class="column is-3">
        <SidebarRight v-if="ready" />
      </div>
    </div>
  </div>
</template>
<script>
import Sidebar from '@/components/Sidebar/index.vue';
import SidebarRight from '@/components/SidebarRight/index.vue';
import Tweet from '@/store/Tweet/index';
import User from '@/store/User/index';


export default {

  components: {
    Sidebar,
    SidebarRight,
  },
  data() {
    return {
      ready: false,
      bgColor: '#0a2d3e',
      textColor: 'white',
      borderColor: 'rgb(37, 51, 65)',
      boxColor: 'rgb(25, 39, 52)',
      style: {
        'background-color': '#0a2d3e',
        color: '#ffffff',
      },
    };
  },

  computed: {
    user() {
      return User.all()[0];
    },
    cssVars() {
      if (this.user && this.user.darkMode) {
        return {
          '--bg-color': this.bgColor,
          '--text-color': this.textColor,
          '--border-color': this.borderColor,
          '--box-color': this.boxColor,
        };
      }
      return {
        '--bg-color': 'white',
        '--text-color': 'black',
        '--border-color': 'rgb(238, 236, 240)',
        '--box-color': 'rgb(245, 248, 250)',
      };
    },
  },

  async mounted() {
    await this.loadUser();
    await this.loadTweets();
    this.ready = true;

    setInterval(async () => {
      await this.loadTweets();
    }, 5000);
  },

  methods: {
    async loadUser() {
      const user = await this.axios.get('/api/Tweet/Me');
      if (!this.user) {
        User.insert({ data: [user.data] });
      } else {
        User.update({
          where: this.user.publicKey,
          data: user.data,
        });
      }
    },

    async loadTweets() {
      const tweets = await this.axios.get('/api/Tweet');
      Tweet.insert({
        data: tweets.data,
      });
    },

  },

};
</script>

<style>
@import "https://cdn.materialdesignicons.com/2.5.94/css/materialdesignicons.min.css";
@import "https://use.fontawesome.com/releases/v5.2.0/css/all.css";

html {
  overflow-y: hidden;
}

#app body {
  background-color: var(--bg-color);

}

#app {
  font-family: 'system-ui', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  background-color: var(--bg-color);
  color: var(--text-color);
  height: 101vh;
  width: 100%;
}
#nav {
  padding: 30px;
}

#nav a {
  font-weight: bold;
  color: #2c3e50;
}

#nav a.router-link-exact-active {
  color: #42b983;
}

.main {
  border-right: 1px solid var(--border-color);
  border-left: 1px solid var(--border-color);
}
</style>
