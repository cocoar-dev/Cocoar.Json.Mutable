# Security Policy

## Supported Versions

| Version | Supported          |
| ------- | ------------------ |
| 0.x.x   | :white_check_mark: |

## Reporting a Vulnerability

If you discover a security vulnerability, please email bwi@cocoar.dev instead of opening a public issue.

Please include:
- Description of the vulnerability
- Steps to reproduce
- Potential impact
- Suggested fix (if any)

We will respond within 48 hours and work with you to address the issue.

## Security Best Practices

When using Cocoar.Json.Zero for handling sensitive data:
- Always dispose `JsonZeroNode` instances to ensure memory is zeroed
- Use UTF-8 APIs (`GetByPathUtf8`, etc.) to avoid string allocations
- Prefer `ParseFromFileAndZero` over `Parse` for sensitive files
- Zero input byte arrays after parsing if they contain secrets
- See [docs/security-implementation.md](docs/security-implementation.md) for detailed security guarantees and best practices

Thank you for helping keep Cocoar.Json.Zero secure!
