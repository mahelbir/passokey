# Passokey Basic Integration Guide

## Authentication Flow

```
┌─────────────────┐      ┌─────────────────┐      ┌─────────────────┐
│   Your App      │      │    Passokey     │      │   User Browser  │
└────────┬────────┘      └────────┬────────┘      └────────┬────────┘
         │                        │                        │
         │  1. Redirect to login  │                        │
         │ ───────────────────────>                        │
         │                        │                        │
         │                        │  2. Show passkey prompt│
         │                        │ ───────────────────────>
         │                        │                        │
         │                        │  3. User authenticates │
         │                        │ <───────────────────────
         │                        │                        │
         │  4. Redirect with JWT  │                        │
         │ <───────────────────────                        │
         │                        │                        │
         │  5. Verify JWT token   │                        │
         │ (using secretKey)      │                        │
         │                        │                        │
```

## Endpoints

### Login

**URL:** `GET /auth/login/{clientId}`

**Query Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `redirectUri` | Yes | Redirect URL to use after authentication
| `state` | No | Arbitrary string passed back to your app after authentication |

**Example:**
```
https://auth.example.com/auth/login/550e8400-e29b-41d4-a716-446655440000?redirectUri=https://myapp.com/callback&state=%2Fadmin%2Fdashboard
```

### Registration

**URL:** `GET /auth/registration/{clientId}`

**Query Parameters:**

| Parameter | Required | Description |
|-----------|----------|-------------|
| `username` | No | Pre-filled username for registration |

**Example:**
```
https://auth.example.com/auth/registration/550e8400-e29b-41d4-a716-446655440000?username=john
```

> **Note:** Registration must be enabled for the client in the admin panel.

### Logout

**URL:** `GET /auth/logout/{clientId}`

**Query Parameters:**

| Parameter     | Required | Description |
|---------------|----------|-------------|
| `redirectUri` | Yes      | Redirect URL to use after authentication |

**Example:**
```
https://auth.example.com/auth/logout/550e8400-e29b-41d4-a716-446655440000?redirectUri=https://myapp.com/callback
```

## Callback Response

After successful authentication, user is redirected to the matching Redirect URI with the following query parameters:

| Parameter | Description |
|-----------|-------------|
| `token` | JWT token signed with client's `secretKey` |
| `clientId` | The client ID |
| `state` | The state parameter (if provided in login request) |

**Example callback URL:**
```
https://myapp.com/callback?token=eyJhbGciOiJIUzI1NiIs...&clientId=550e8400-e29b-41d4-a716-446655440000&state=%2Fadmin%2Fdashboard
```

## JWT Token

The JWT token contains:

| Claim | Description |
|-------|-------------|
| `clientId` | The client ID (GUID) |
| `userId` | The authenticated user's ID (GUID) |
| `aud` | Audience - the domain extracted from redirect URI |
| `exp` | Token expiration timestamp |

### Verifying the Token

The token is signed using **HMAC-SHA256** with the client's `secretKey`. You must verify the token on your backend before trusting its contents.

**Example (Node.js):**
```javascript
const jwt = require('jsonwebtoken');

const secretKey = 'your-client-secret-key';
const token = req.query.token;

try {
    const payload = jwt.verify(
        token,
        secretKey,
        {
            algorithms: ['HS256']
        }
    );
  const userId = payload.userId;
  // Create session for user
} catch (err) {
  // Invalid token
}
```

**Example (C#):**
```csharp
var tokenHandler = new JwtSecurityTokenHandler();
var key = Encoding.UTF8.GetBytes('your-client-secret-key');

var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
{
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateLifetime = true,
    IssuerSigningKey = new SymmetricSecurityKey(key),
    ValidateIssuerSigningKey = true,
    ClockSkew = TimeSpan.Zero
}, out _);

var userId = principal.FindFirst("userId")?.Value;
// Create session for user
```

## Notes

- **Redirect URIs:** Each client can have multiple Redirect URIs configured in the admin panel. The `?redirectUri=` query parameter in the login request must match one of the configured URIs (compared by scheme, host, port, and path).

- **User Permissions:** Users must have permission granted for the specific client to authenticate. Permissions are managed in the admin panel under "User Client Permissions".

- **SSO Behavior:** Once a user authenticates for a client, their session remains valid (based on `AuthorizedClientLifetimeMinutes` setting). Subsequent visits to the login page will automatically redirect with a new JWT token without requiring re-authentication.

- **Token Lifetime:** JWT tokens have a short lifetime (configured via `TokenLifetimeMinutes`). They are meant for one-time use to establish a session on your application, not for long-term storage but you can still use them if needed.
