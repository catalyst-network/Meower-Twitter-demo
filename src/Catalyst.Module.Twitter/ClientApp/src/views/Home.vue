<template>
  <div class="home">
    <div class="page-title">
      <p>Home</p>
    </div>
    <div>
      <TweetBox />
      <div class="grey-separator" />
    </div>
    <Feed />
  </div>
</template>

<script>
// @ is an alias to /src
import Feed from '@/components/Feed/index.vue';
import TweetBox from '@/components/TweetBox/index.vue';

export default {
  name: 'Home',
  components: {
    Feed,
    TweetBox,
  },
};
</script>
 <style>
 .page-title {
   height: 53px;
   border-bottom: 1px solid var(--border-color);
   font-family: 'system-ui';
   font-weight: bold;
   font-size: 19px;
   margin-left: -0.75rem;
   margin-right: -0.75rem;
 }

 .page-title {
   padding: 15px;
 }

 .grey-separator {
   height: 10px;
   background-color: var(--border-color);
   margin-left: -0.75rem;
   margin-right: -0.75rem;
 }
 </style>
