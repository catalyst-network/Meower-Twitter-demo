import { Model } from '@vuex-orm/core';

export default class Tweet extends Model {
  // This is the name used as module name of the Vuex Store.
  static entity = 'tweets'

  static primaryKey = 'tweetHash'

  static fields() {
    return {
      tweetHash: this.attr(''),
      message: this.attr(''),
      publicKey: this.attr(''),
      date: this.attr(''),
      likes: this.attr(''),
    };
  }
}
