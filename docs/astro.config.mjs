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
      favicon: '/favicon.svg',
      head: [
        // PNG fallbacks for browsers that don't use the SVG favicon, plus the Apple touch icon.
        { tag: 'link', attrs: { rel: 'icon', href: '/favicon-32.png', type: 'image/png', sizes: '32x32' } },
        { tag: 'link', attrs: { rel: 'apple-touch-icon', href: '/favicon.png', sizes: '180x180' } },
      ],
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
            { label: 'The Stage & lifecycle', slug: 'concepts/stage' },
            { label: 'Captures: Capture vs Trapture', slug: 'concepts/captures' },
            { label: 'Containers & the mock→DI bridge', slug: 'concepts/containers' },
          ],
        },
        {
          label: 'Guides',
          items: [
            { label: 'Unit-test a service with a mock', slug: 'guides/unit-mocked-dependency' },
            { label: 'Pick a mock library', slug: 'guides/mock-libraries' },
            { label: 'Wire a real DI container', slug: 'guides/real-di-container' },
            { label: 'The Bag container', slug: 'guides/bag-container' },
            { label: 'Integration-test a database', slug: 'guides/integration-database' },
            { label: 'Full black-box E2E', slug: 'guides/end-to-end' },
            { label: 'Test a Kafka consumer / producer', slug: 'guides/kafka' },
            { label: 'Async / eventually-consistent assertions', slug: 'guides/eventually-consistent' },
            { label: 'Deterministic time & ids', slug: 'guides/deterministic-time-ids' },
          ],
        },
        {
          label: 'Techniques',
          items: [
            { label: 'Value & context scopes', slug: 'guides/inspect-scopes' },
            { label: 'Parallel inspects with ThenAll', slug: 'guides/thenall' },
            { label: 'Ensure: derive, guard, capture', slug: 'guides/ensure' },
            { label: 'Snapshot assertions with Verify', slug: 'guides/verify-snapshots' },
            { label: 'Source-generated arranges', slug: 'guides/mokkit-capture' },
          ],
        },
        {
          label: 'Extending',
          items: [
            { label: 'Advanced vocabulary techniques', slug: 'guides/advanced-vocabulary' },
            { label: 'Write a custom container adapter', slug: 'guides/custom-container-adapter' },
          ],
        },
        {
          label: 'Reference',
          items: [
            { label: 'How to structure a test project', slug: 'reference/project-structure' },
            { label: 'Conventions cheat-sheet', slug: 'reference/conventions' },
            // DocFX-generated static site under /api (built separately in CI).
            { label: 'API reference', link: '/api/' },
          ],
        },
      ],
    }),
  ],
});
