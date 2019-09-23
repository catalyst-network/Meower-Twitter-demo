<template id="feed">
  <div class="feed-container">
    <div
      v-for="tweet in data.list"
      :key="tweet.tweetHash"
      class="tweet"
    >
      <Tweet :tweet="tweet" />
    </div>
    <infinite-loading @infinite="infiniteHandler" />
  </div>
</template>

<script>
import InfiniteLoading from 'vue-infinite-loading';
import Tweets from '@/store/Tweet/index';
import Tweet from '@/components/Tweet/index.vue';
import User from '@/store/User/index';

export default {
  components: {
    Tweet,
    InfiniteLoading,
  },
  data() {
    return {
      data: {
        search: '',
        start: 0,
        end: 0,
        list: [],
      },
    };
  },
  computed: {
    search() {
      return User.all()[0].search;
    },
    filtered() {
      return Tweets.all()
        .filter(tweet => tweet.message.toLowerCase().indexOf(this.search) > -1)
        .sort((a, b) => {
          const aDate = new Date(a.date);
          const bDate = new Date(b.date);
          if (aDate > bDate) { return -1; } if (aDate < bDate) { return 1; } return 0;
        });
    },
  },

  watch: {
    filtered(val) {
      this.data.list = val.slice(this.data.start, this.data.end);
    },
  },

  methods: {
    infiniteHandler($state) {
      if (this.data.list.length < this.filtered.length) {
        this.data.end += 10;
        this.data.list = this.filtered.slice(this.data.start, this.data.end);
        $state.loaded();
      } else {
        $state.complete();
      }
    },
  },
};
</script>

<style>
    #feed {
      overflow-y: hidden;
    }

    .avatar-section {
        width: 60px;
    }
    .avatarContainer {
        margin-top: 10px;
        margin-left: 10px
    }
    .avatar {
        border-radius: 50%;
    }
    .tweetHead {
        font-size: 13px;
    }
    .author {
        font-size: 14px;
        font-weight: bold;
    }
    .tweetContent {
        font-size: 14px;
        line-height: 20px;
        margin-top: 3px;
    }

    .tweet-container {
      border-top: 1px solid var(--border-color);
      padding-bottom: 0.75rem;
      font-family: 'system-ui';
    }

    .feed-container {
      padding-top: 10px;
      overflow-y: auto;
      overflow-x: hidden;
      height: calc(100vh - 190px);
    }

    .feed-container::-webkit-scrollbar { width: 0 !important }
</style>
