<template>
  <div class="left-nav is-pulled-right">
    <b-menu>
      <b-menu-list>
        <img
          class="logo"
          width="40px"
          src="@/assets/catalyst-icon.png"
        >
        <b-menu-item
          icon="home"
          label="Home"
          active
        />
        <b-menu-item
          icon="hashtag"
          label="Explore"
        />
        <b-menu-item
          icon-pack="far"
          icon="bell"
          label="Notifications"
        />
        <b-menu-item
          icon-pack="far"
          icon="envelope"
          label="Messages"
        />
        <b-menu-item
          icon-pack="far"
          icon="bookmark"
          label="Bookmarks"
        />
        <b-menu-item
          icon-pack="far"
          icon="list-alt"
          label="Lists"
        />
        <b-menu-item
          icon-pack="far"
          icon="user"
          label="Profile"
        />
        <b-menu-item
          icon="angle-double-right"
          label="More"
        />
        <div class="columns tweet-button">
          <b-button
            rounded
            class="tweet-button"
            @click="isTweetModalActive = true"
          >
            Tweet
          </b-button>
        </div>
        <div class="columnns">
          <div class="field">
            <b-switch
              v-model="darkMode"
              type="is-info"
            >
              Dark Mode
            </b-switch>
          </div>
        </div>
      </b-menu-list>
    </b-menu>
    <b-modal
      :active.sync="isTweetModalActive"
      has-modal-card
      class="tweet-dialog"
    >
      <div class="card">
        <div class="card-content">
          <TweetBox :modal="true" />
        </div>
      </div>
    </b-modal>
  </div>
</template>

<script>
import User from '@/store/User/index';
import TweetBox from '@/components/TweetBox/index.vue';

export default {
  name: 'Sidebar',
  components: {
    TweetBox,
  },

  data() {
    return {
      isTweetModalActive: false,
    };
  },

  computed: {
    user() {
      return User.all()[0];
    },
    darkMode: {
      get() {
        if (this.user) {
          return this.user.darkMode;
        }
        return false;
      },
      set(val) {
        User.update({
          where: this.user.publicKey,
          data: {
            darkMode: val,
          },
        });
      },
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style>
.left-nav span {
  margin-right: 20px;
}
.left-nav li {
    height: 60.5px;
    font-size: 19px;
    font-family: 'system-ui';
    font-weight: bold;
}

.left-nav .menu-list a {
  color: var(--text-color);
}

.left-nav .menu-list a.is-active {
    background: none;
    color: #44ac9f;
}

.left-nav .menu-list a:hover {
    background-color: #14ac9e0f;
    color: #44ac9f;
    border-radius: 50px;
}

.left-nav  .card{
  border-radius: 20px;
}

.tweet-dialog .moadal-background {
  background-color: rgba(10, 10, 10, 0.3);
}
.tweet-dialog span {
  margin-right: 0;
}

.logo {
  margin: 10px;
}
.tweet-button button {
  color: white;
  background-color: #44ac9f;
  margin: 15px;
  width: 90%;
  border: none;
}

.tweet-button button {
  height: 49px;
}

.tweet-button button:hover {
  color: white;
}

.tweet-button span {
  margin-right: 0;
}
</style>
