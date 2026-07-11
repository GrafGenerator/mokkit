// @ts-check
import { defineConfig } from 'astro/config';
import starlight from '@astrojs/starlight';

// The project's custom domain — drives canonical URLs and the sitemap.
export default defineConfig({
  site: 'https://mokkit.net',
  integrations: [
    starlight({
      title: 'Mokkit',
      description: "Write tests that read like a story in your domain's language — as plain, compilable C#.",
      social: [
        { icon: 'github', label: 'GitHub', href: 'https://github.com/GrafGenerator/mokkit' },
      ],
      editLink: {
        baseUrl: 'https://github.com/GrafGenerator/mokkit/edit/main/docs/',
      },
      sidebar: [
        {
          label: 'Start here',
          items: [
            { label: 'Introduction', slug: 'introduction' },
            { label: 'Why Mokkit?', slug: 'why-mokkit' },
            { label: 'Installation', slug: 'installation' },
            { label: 'Quickstart', slug: 'quickstart' },
          ],
        },
        {
          label: 'Core concepts',
          items: [
            { label: 'Arrange / Act / Inspect', slug: 'concepts/aai' },
            { label: 'Building your test vocabulary', slug: 'concepts/vocabulary' },
            { label: 'Scenario tests', slug: 'concepts/scenarios' },
          ],
        },
        {
          label: 'Reference',
          items: [
            // DocFX-generated static site under /api (built separately in CI).
            { label: 'API reference', link: '/api/' },
          ],
        },
        // Guides are added as those pages land.
      ],
    }),
  ],
});
