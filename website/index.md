---
layout: home

hero:
  name: Cocoar.Json.Mutable
  text: Fast. Mutable. Merge-Focused.
  tagline: High-performance mutable JSON DOM for .NET. UTF-8 native, zero-copy where possible, built for merging.
  actions:
    - theme: brand
      text: Get Started
      link: /guide/getting-started
    - theme: alt
      text: Why Cocoar.Json.Mutable?
      link: /guide/why
    - theme: alt
      text: GitHub
      link: https://github.com/cocoar-dev/Cocoar.Json.Mutable

features:
  - icon: |-
      <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M17 3a2.85 2.83 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5Z"/></svg>
    title: Mutable by Design
    details: Create, modify, and merge JSON documents in-memory. Set properties, add array items, replace values — all without rebuilding the tree.
  - icon: |-
      <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z"/></svg>
    title: UTF-8 Native
    details: Strings and numbers stored as raw UTF-8 byte arrays. ReadOnlySpan&lt;byte&gt; and ReadOnlyMemory&lt;byte&gt; for zero-allocation access.
  - icon: |-
      <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M16 16v6"/><path d="M16 22h6"/><path d="M21 10V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4"/><path d="m3.3 7 8.7 5 8.7-5"/><path d="M12 22V12"/></svg>
    title: Deep Merge
    details: Recursive deep merging of JSON objects. Non-destructive (clone) or destructive (move) — you choose the trade-off.
  - icon: |-
      <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect width="18" height="18" x="3" y="3" rx="2"/><path d="M3 9h18"/><path d="M9 21V9"/></svg>
    title: Zero Dependencies
    details: Built on System.Text.Json — no external dependencies. Uses Utf8JsonReader and Utf8JsonWriter under the hood.
  - icon: |-
      <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M2 12h20"/><path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/></svg>
    title: Provider-Friendly
    details: Accepts ReadOnlyMemory&lt;byte&gt; directly from data providers. No intermediate string conversion needed.
  - icon: |-
      <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
    title: Dual API
    details: String-based API for convenience, UTF-8 byte API for performance. Use whichever fits your context — or mix them freely.
---
