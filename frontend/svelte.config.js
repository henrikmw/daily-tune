import adapter from '@sveltejs/adapter-static';

export default {
  kit: {
    adapter: adapter({
      // default options are fine for basic usage
      pages: 'build',
      assets: 'build',
      fallback: null
    })
  }
};
