# Security Policy

## Supported Versions

| Version | Target Frameworks | Supported |
|---------|-------------------|-----------|
| 1.x     | net8.0, net9.0, net10.0 | Yes |

## Reporting a Vulnerability

**Please do not open a public GitHub issue for security vulnerabilities.**

Instead, report them privately via email:

- **Email:** contact@emu-fake.com
- **Subject:** `[SECURITY] KASserver.NET — <brief description>`

Please include:

- A description of the vulnerability
- Steps to reproduce or a proof-of-concept
- The affected version(s)
- Any potential impact assessment

## Response Timeline

- **Acknowledgment:** Within 48 hours of receiving the report.
- **Assessment:** Within 7 days we will confirm the vulnerability and communicate next steps.
- **Fix:** We aim to release a patch within 30 days of confirmation, depending on severity.

We appreciate responsible disclosure and will credit reporters in the release notes (unless you prefer to remain anonymous).

## Handling of Credentials

KASserver.NET sends your KAS login and password to the official ALL-INKL.COM endpoints (`https://kasapi.kasserver.com/...`) over HTTPS only. Credentials are never logged, persisted, or transmitted anywhere else. Treat your KAS password like any other secret — prefer configuration providers / environment variables over hardcoding.
