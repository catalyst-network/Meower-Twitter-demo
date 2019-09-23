import { Model } from '@vuex-orm/core';
import Tweets from '@/store/Tweet/';

export default class User extends Model {
  static entity = 'users'

  static primaryKey = 'publicKey'

  static fields() {
    return {
      publicKey: this.attr(''),
      search: this.attr(''),
      darkMode: this.attr(''),
      tweets: this.hasMany(Tweets, 'publicKey'),
    };
  }
}
