import { defineConfig } from 'vitepress'
import { withMermaid } from 'vitepress-plugin-mermaid'
import llmstxt from 'vitepress-plugin-llms'

export default withMermaid(
  defineConfig({
    title: 'Cocoar.Json.Mutable',
    description: 'High-performance mutable JSON DOM for .NET, optimized for merging',

    head: [
      ['link', { rel: 'icon', type: 'image/svg+xml', href: '/logo_light.svg' }],
      ['link', { rel: 'alternate', type: 'text/plain', href: '/llms.txt', title: 'LLM documentation (summary)' }],
      ['link', { rel: 'alternate', type: 'text/plain', href: '/llms-full.txt', title: 'LLM documentation (full)' }],
    ],

    vite: {
      plugins: [llmstxt({
        excludeUnnecessaryFiles: false,
        ignoreFiles: ['changelog.md'],
      })],
    },

    themeConfig: {
      logo: {
        light: '/logo_light.svg',
        dark: '/logo_dark.svg',
      },

      siteTitle: 'Cocoar.Json.Mutable v1',

      nav: [
        { text: 'Guide', link: '/guide/getting-started' },
        { text: 'Reference', link: '/reference/api' },
        { text: 'Changelog', link: '/changelog' },
        { text: 'LLM Docs', link: '/llms-full.txt', target: '_blank' },
        { text: 'NuGet', link: 'https://www.nuget.org/packages/Cocoar.Json.Mutable' },
      ],

      sidebar: {
        '/guide/': [
          {
            text: 'Introduction',
            items: [
              { text: 'Getting Started', link: '/guide/getting-started' },
              { text: 'Why Cocoar.Json.Mutable?', link: '/guide/why' },
            ],
          },
          {
            text: 'Core Concepts',
            items: [
              { text: 'Node Types', link: '/guide/node-types' },
              { text: 'Parsing & Serialization', link: '/guide/parsing-serialization' },
              { text: 'Merging', link: '/guide/merging' },
              { text: 'Path Operations', link: '/guide/path-operations' },
            ],
          },
          {
            text: 'Performance',
            items: [
              { text: 'UTF-8 API & Memory <span class="badge-adv" title="Advanced topic"></span>', link: '/guide/utf8-api' },
            ],
          },
        ],
        '/reference/': [
          {
            text: 'Reference',
            items: [
              { text: 'API Overview', link: '/reference/api' },
            ],
          },
        ],
      },

      socialLinks: [
        { icon: 'github', link: 'https://github.com/cocoar-dev/Cocoar.Json.Mutable' },
      ],

      search: {
        provider: 'local',
      },

      footer: {
        message: 'Released under the Apache-2.0 License.',
        copyright: 'Copyright 2025-present Cocoar',
      },
    },

    mermaid: {},

    mermaidPlugin: {
      class: 'mermaid',
    },
  }),
)
