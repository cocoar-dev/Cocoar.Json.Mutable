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

**Important: This library is NOT designed for sensitive data handling.**

Cocoar.Json.Mutable does not provide:
- Memory zeroing after operations
- Secure disposal patterns  
- Protection against memory dumps

If you're working with sensitive data (secrets, credentials, personal information), use specialized libraries designed for secure data handling.

For general security when using this library:
- Don't store sensitive data in JSON processed by this library
- Use appropriate encryption for data at rest and in transit
- Follow standard security practices for your application

Thank you for helping keep Cocoar.Json.Mutable secure!
