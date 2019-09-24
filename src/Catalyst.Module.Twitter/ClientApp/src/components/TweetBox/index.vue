<template>
  <div>
    <div class="tweet-box">
      <div>
        <b-field>
          <figure class="image">
            <img
              class="is-rounded"
              width="49px"
              height="49px"
              :src="avatar"
            >
          </figure>
          <b-input
            v-if="!modal"
            v-model="message"
            placeholder="What's happening?"
            maxlength="240"
          />
          <b-input
            v-if="modal"
            v-model="message"
            placeholder="What's happening?"
            type="textarea"
            maxlength="240"
          />
        </b-field>
      </div>
      <div class="columns">
        <div class="column tweet-buttons">
          <div class="block">
            <b-icon
              pack="far"
              icon="image"
              size="is-medium"
            />
            <span @click="isEmojiModalActive = true">
              <b-icon
                pack="far"
                icon="smile"
                size="is-medium"
              />
            </span>

            <b-icon
              icon="poll"
              size="is-medium"
              type="is-info"
            />
          </div>
        </div>
        <div class="column tweet-button2 is-pulled-right">
          <b-button
            rounded
            class="tweet-button is-pulled-right"
            @click="tweet"
          >
            Tweet
          </b-button>
        </div>
      </div>
    </div>
    <div
      v-if="isEmojiModalActive"
      class="emoji-container"
      @click="isEmojiModalActive = false"
    />
    <Picker
      v-if="isEmojiModalActive"
      set="twitter"
      style="position: absolute; z-index: 900;"
      color="#44ac9f"
      emoji=""
      title=""
      @select="addEmoji"
    />
  </div>
</template>

<script>
import { Picker } from 'emoji-mart-vue';
import User from '@/store/User/index';

export default {
  name: 'TweetBox',
  components: {
    Picker,
  },
  props: {
    modal: Boolean,
  },
  data() {
    return {
      message: '',
      isEmojiModalActive: false,
    };
  },

  computed: {
    user() {
      return User.all()[0];
    },

    avatar() {
      return `https://robohash.org/${this.user.publicKey}.png?set=set4`;
    },
  },

  methods: {
    addEmoji(emoji) {
      this.message += emoji.native;
    },
    async tweet() {
      const tweet = this.message;
      this.message = null;
      if (tweet) {
        try {
          if (this.modal) {
            await this.axios.post('/api/Tweet/', {
              message: tweet,
            });
            this.$parent.close();
          } else {
            await this.axios.post('/api/Tweet/', {
              message: tweet,
            });
          }
        } catch (e) {
          console.error(e);
        }
      }
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style>
.tweet-box {
    margin-top:10px;
}

.tweet-box .image img {
    width: 49px;
    height: 49px;
}

.tweet-box input {
    margin-top: 5px;
    border: none;
    box-shadow: none;
    width: 100%;
    background-color: var(--bg-color);
    color: var(--text-color);
}

.tweet-box textarea {
    margin-top: 5px;
    border: none;
    box-shadow: none;
    width: 100%
}

.tweet-box input:focus {
    outline: none;
    box-shadow: none;
}

.tweet-box input::placeholder {
    color: var(--text-color);
}

.tweet-box textarea:focus {
    outline: none;
    box-shadow: none;
}
.tweet-box .control {
    width: 100%;
}
.tweet-button2 button {
  color: white;
  background-color: #44ac9f;
  border: none;
}
.tweet-button2 button:hover {
  color: white;
}
.tweet-button2 button:active {
  color: white;
}
.tweet-button2 button:focus {
  color: white;
}


.tweet-buttons {
    margin-top: 10px;
    margin-left: 50px;
}

.tweet-buttons .icon {
    margin-right: 10px;
    color: #44ac9f;
}

.tweet-buttons .fa-2x {
    font-size: 22.5px;
}

.emoji-container {
  position: absolute;
  left: 0;
  top: 0;
  width: 100%;
  height:  100%;
}

.emoji-container .emoji-mart {
  background-color: var(--bg-color);
  color: var(--text-color);
}
</style>
