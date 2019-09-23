<template>
  <div class="right-bar">
    <div class="section">
      <b-field>
        <b-input
          v-model="search"
          class="search-box"
          placeholder="Search Twitter"
          icon="search"
          rounded
        />
      </b-field>
    </div>
    <!-- <div class="section">
      <div class="panel">
        <p class="panel-header">
          Worldwide Trends
        </p>
      </div>
    </div> -->
  </div>
</template>

<script>
import User from '@/store/User/index';

export default {
  name: 'Sidebar',
  computed: {
    user() {
      return User.all()[0];
    },
    search: {
      get() {
        return this.user.search;
      },
      set(val) {
        User.update({
          where: this.user.publicKey,
          data: {
            search: val,
          },
        });
      },
    },
  },
};
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->
<style>

.right-bar input::placeholder {
  color: var(--text-color);

}

.panel-header {
  font-family: 'system-ui';
  font-size: 19px;
  font-weight: bold;
  border-left: none;
  border-right: none;
  border-radius: 15px 15px 0 0;
  padding: 0.5em 0.75em;
      background-color: var(--box-color);
}
.panel-header:first-child {
border-top: none;
}
.right-bar .section {
  padding: 10px;
}

.right-bar .input {
  border: none;
  box-shadow: none;
  color: var(--text-color);
}

.search-box input {
  background-color: var(--box-color);
}
</style>
