# Integration Methods

### 1. [Basic Integration](BASIC.md)

A simple redirect-based authentication flow. Passokey authenticates the user and redirects back to your application with a signed JWT token. Your backend verifies the token using a shared secret key.

**Best for:** Custom applications where you control both the frontend and backend, and want a straightforward integration without external dependencies.

### 2. [OpenID Connect Integration](OIDC.md)

A standard OpenID Connect (OIDC) Authorization Code flow. Passokey acts as an OIDC Identity Provider, issuing standard ID tokens and access tokens. Any OIDC-compatible service can integrate without custom code.

**Best for:** Third-party services (Cloudflare Access, Portainer, Grafana, etc.) and applications that already support OIDC authentication.

## Comparison

| | Basic | OpenID Connect |
|---|---|---|
| **Protocol** | Custom JWT redirect | OIDC Authorization Code |
| **Token Type** | HMAC-SHA256 JWT (symmetric) | RS256 JWT (asymmetric) |
| **Token Verification** | Shared secret key on your backend | Public key via JWKS endpoint |
| **Secret Sharing** | Secret key must be on your server | Secret only used during token exchange |
| **3rd Party Support** | Requires manual integration | Works with any OIDC-compatible service |
| **Standard Compliance** | Custom protocol | OpenID Connect 1.0 |
| **Auto-Discovery** | No | `/.well-known/openid-configuration` |
| **Setup Complexity** | Minimal | Standard OIDC configuration |
| **UserInfo Endpoint** | Not available (all claims in JWT) | `/oidc/userinfo` |
| **PKCE Support** | Not applicable | Supported (S256) |

## Which One Should You Choose?

**Choose Basic Integration if:**
- You are building a custom application with your own backend
- You want the simplest possible setup
- Your application does not support OIDC natively

**Choose OpenID Connect if:**
- You are integrating with a third-party service (Cloudflare Access, Portainer, Grafana, etc.)
- Your application already supports OIDC / OAuth 2.0 login
- You prefer asymmetric token verification (no shared secret needed for validation)
- You want auto-discovery of endpoints and signing keys

## Common to Both Methods

Regardless of the integration method, the following applies:

- **Passkey Authentication:** Users always authenticate using FIDO2/WebAuthn passkeys. No passwords are involved.
- **User Permissions:** Users must have explicit permission for the specific client. Permissions are managed in the admin panel.
- **SSO Sessions:** Once authenticated, the user's session is reused for subsequent requests within the configured lifetime.
- **Redirect URIs:** Each client can have multiple Redirect URIs. The redirect URI in the request must match one of the configured URIs.
- **Registration:** User registration can be enabled/disabled per client in the admin panel.
