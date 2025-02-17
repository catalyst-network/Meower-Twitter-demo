<template>
  <div class="columns tweet-container">
    <div class="avatar-section">
      <div class="avatarContainer">
        <img v-lazy="avatar">
      </div>
    </div>
    <div class="column">
      <div class="tweetBody">
        <div class="tweetHead">
          <b-tooltip
            position="is-right"
            type="is-dark"
            :label="tweet.publicKey"
          >
            <span class="author">
              {{ name }}
            </span>
          </b-tooltip>
          {{ date }}
        </div>
        <div
          v-emoji
          class="tweetContent"
        >
          {{ tweet.message }}
        </div>
      </div>
      <div class="tweetLikes">
        <div @click="likeTweet">
          <b-icon
            v-if="!userLiked"
            pack="far"
            icon="heart"
            size="is-small"
          />
        </div>
        <div @click="unlikeTweet">
          <b-icon
            v-if="userLiked"
            pack="fas"
            icon="heart"
            size="is-small"
            type="is-danger"
          />
        </div>
        <div>
          <span class="like-number">{{ likes }}</span>
        </div>
      </div>
    </div>
  </div>
</template>
<script>
import User from '@/store/User/index';

export default {
  props: {
    tweet: {
      type: Object,
      required: true,
    },
  },
  data() {
    return {
      userLiked: false,
      likes: 0,
    };
  },

  computed: {
    avatar() {
      return `https://robohash.org/${this.tweet.publicKey}.png?set=set4`;
    },
    date() {
      const now = new Date().getTime();
      const date = new Date(this.tweet.date).getTime();
      const timeSince = now - date;
      const minutesSince = Math.floor(timeSince / 60000);
      if (minutesSince < 60) {
        return `${minutesSince}m`;
      }
      if (minutesSince < 1440) {
        return `${Math.floor(minutesSince / 60)}h`;
      }
      return `${Math.floor(minutesSince / 1440)}d`;
    },
    user() {
      return User.all()[0];
    },
    name() {
      if (this.tweet.publicKey === '5oNCckFnRY8MKCVC1htNfeDRpyt2vMMC6tngjcyTNw9a') {
        return 'Alice';
      }
      if (this.tweet.publicKey === '4rsFxRNsT1ZUtLNeWTnUHd3X732hh8KAKNPJcCSUmizg') {
        return 'Bob';
      }
      return 'Charlie';
    },

    liked() {
      return this.tweet.likes.map(like => like.publicKey).includes(this.user.publicKey);
    },
    tweetLikes() {
      return this.tweet.likes.length;
    },
  },

  watch: {
    tweetLikes(val) {
      this.likes = val;
      if (this.liked) {
        this.userLiked = true;
      } else {
        this.userLiked = false;
      }
    },
  },

  mounted() {
    this.userLiked = this.liked;
    this.likes = this.tweet.likes.length;
  },

  methods: {
    async likeTweet() {
      try {
        this.userLiked = true;
        this.likes += 1;
        await this.axios.post(`/api/Tweet/Like/${this.tweet.tweetHash}`);
      } catch (e) {
        console.error(e);
        this.userLiked = false;
      }
    },

    async unlikeTweet() {
      try {
        this.userLiked = false;
        this.likes -= 1;
        await this.axios.post(`/api/Tweet/Unlike/${this.tweet.tweetHash}`);
      } catch (e) {
        this.userLiked = false;
        console.error(e);
      }
    },
  },
};
</script>

<style>
.like-number {
  padding-left: 5px;
  color: #8899a6
}

.tweetLikes div {
  display: inline;
}
.tweetLikes {
  margin-top: 5px;
  color: #8899a6;
}

.tweetContent .emoji {
  width: 18px;
  margin-left: 1px;
  margin-right: 1px;
}
</style>
