# Passokey OpenID Connect Integration Guide

## Authentication Flow

```
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│  Service         │      │    Passokey     │      │   User Browser  │
│  Provider (SP)  │      │  (OIDC Server)  │      │                 │
└────────┬────────┘      └────────┬────────┘      └────────┬────────┘
         │                        │                        │
         │  1. Redirect to        │                        │
         │     /oidc/authorize    │                        │
         │ ───────────────────────>                        │
         │                        │                        │
         │                        │  2. Show passkey prompt│
         │                        │ ───────────────────────>
         │                        │                        │
         │                        │  3. User authenticates │
         │                        │ <───────────────────────
         │                        │                        │
         │  4. Redirect with      │                        │
         │     authorization code │                        │
         │ <───────────────────────                        │
         │                        │                        │
         │  5. Exchange code for  │                        │
         │     tokens (server)    │                        │
         │ ───────────────────────>                        │
         │                        │                        │
         │  6. Return id_token +  │                        │
         │     access_token       │                        │
         │ <───────────────────────                        │
         │                        │                        │
         │  7. Fetch user info    │                        │
         │     (optional)         │                        │
         │ ───────────────────────>                        │
         │                        │                        │
         │  8. Return user claims │                        │
         │ <───────────────────────                        │
         │                        │                        │
```

## Discovery

Passokey exposes a standard OpenID Connect Discovery endpoint:

```
GET /.well-known/openid-configuration
```

Service providers can use this URL to automatically discover all endpoints, supported scopes, and signing keys.

## Endpoints

| Endpoint | Method | URL | Description |
|----------|--------|-----|-------------|
| Authorization | GET | `/oidc/authorize` | Initiates the authorization flow |
| Token | POST | `/oidc/token` | Exchanges authorization code for tokens |
| UserInfo | GET/POST | `/oidc/userinfo` | Returns authenticated user's claims |
| Discovery | GET | `/.well-known/openid-configuration` | OpenID Provider configuration |
| JWKS | GET | `/.well-known/jwks` | JSON Web Key Set for token verification |

## Authorization Request

**URL:** `GET /oidc/authorize`

**Query Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `client_id` | Yes | Client ID from the admin panel |
| `redirect_uri` | Yes | Must match one of the client's configured Redirect URIs |
| `response_type` | Yes | Must be `code` |
| `scope` | Yes | Space-separated scopes (e.g., `openid email profile`) |
| `state` | Recommended | Opaque value for CSRF protection, returned unchanged |
| `nonce` | Recommended | Value to associate with the ID token |
| `code_challenge` | Optional | PKCE code challenge |
| `code_challenge_method` | Optional | PKCE method (`S256` recommended) |

**Example:**
```
https://auth.example.com/oidc/authorize?client_id=550e8400-e29b-41d4-a716-446655440000&redirect_uri=https://myapp.com/oauth/callback&response_type=code&scope=openid+email+profile&state=random-csrf-token&nonce=random-nonce
```

**Successful Response:**

The user is redirected to the `redirect_uri` with an authorization code:
```
https://myapp.com/oauth/callback?code=AUTHORIZATION_CODE&state=random-csrf-token&iss=https://auth.example.com/
```

## Token Exchange

**URL:** `POST /oidc/token`

**Request:**
```http
POST /oidc/token HTTP/1.1
Content-Type: application/x-www-form-urlencoded

grant_type=authorization_code
&code=AUTHORIZATION_CODE
&client_id=550e8400-e29b-41d4-a716-446655440000
&client_secret=your-client-secret-key
&redirect_uri=https://myapp.com/oauth/callback
```

**Response:**
```json
{
  "access_token": "eyJhbGciOiJSUzI1NiIs...",
  "id_token": "eyJhbGciOiJSUzI1NiIs...",
  "token_type": "Bearer",
  "expires_in": 3600
}
```

## UserInfo Endpoint

**URL:** `GET /oidc/userinfo`

**Request:**
```http
GET /oidc/userinfo HTTP/1.1
Authorization: Bearer {access_token}
```

**Response:**
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "john@auth.example.com",
  "name": "john"
}
```

## ID Token Claims

The ID token is a JWT signed with **RS256** containing:

| Claim | Description |
|-------|-------------|
| `sub` | The authenticated user's ID |
| `email` | User email in format `username@{server-host}` |
| `name` | Username |
| `iss` | Issuer URL (your Passokey server) |
| `aud` | Audience (the client_id) |
| `iat` | Issued at timestamp |
| `exp` | Token expiration timestamp |
| `nonce` | The nonce value (if provided in authorization request) |

Token signatures can be verified using the public key from the JWKS endpoint (`/.well-known/jwks`).

## Scopes

| Scope | Description |
|-------|-------------|
| `openid` | Required. Enables OpenID Connect and returns an ID token |
| `email` | Includes the `email` claim |
| `profile` | Includes the `name` claim |

## Client Configuration

In the Passokey admin panel, each client has the values needed for OIDC:

| OIDC Parameter | Passokey Source |
|----------------|----------------|
| `client_id` | Client ID shown in the admin panel |
| `client_secret` | Client's Secret Key |
| `redirect_uri` | One of the client's configured Redirect URIs |

## Provider Configuration Example (Cloudflare Access)

When configuring Passokey as a generic OIDC provider in Cloudflare Access:

| Field | Value |
|-------|-------|
| Auth URL | `https://{your-passokey-domain}/oidc/authorize` |
| Token URL | `https://{your-passokey-domain}/oidc/token` |
| Certificate URL | `https://{your-passokey-domain}/.well-known/jwks` |
| Client ID | Client ID from the admin panel |
| Client Secret | Client's Secret Key from the admin panel |

## Notes

- **Authorization Code Flow:** Only the Authorization Code flow is supported. Implicit and hybrid flows are not available.

- **Confidential Clients:** All clients are registered as confidential clients. The `client_secret` is required for the token exchange.

- **Redirect URIs:** Each client can have multiple Redirect URIs configured in the admin panel. The `redirect_uri` parameter in the authorization request must exactly match one of the configured URIs.

- **User Permissions:** Users must have permission granted for the specific client to authenticate. Permissions are managed in the admin panel under "User Client Permissions".

- **SSO Behavior:** Once a user authenticates for a client, their session remains valid (based on `AuthorizedClientLifetimeMinutes` setting). Subsequent authorization requests will automatically issue a new code without requiring re-authentication.

- **Implicit Consent:** There is no consent screen. Authorization is granted automatically if the user has a valid session and the required permissions.

- **Email Format:** The email claim is generated as `username@{host}`, where `host` is extracted from the `BaseUrl` configuration. These are not real email addresses but unique identifiers. If you wish, you can add a real email column to the user table.

- **Token Signing:** All tokens are signed with RS256 (RSA 2048-bit). The public key is available at the JWKS endpoint for signature verification.

- **PKCE Support:** PKCE (Proof Key for Code Exchange) with S256 method is supported for enhanced security.
