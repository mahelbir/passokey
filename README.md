# Passokey

Passokey is a self-hosted Passkey (WebAuthn/FIDO2) authentication server with multi-client SSO support.

<a target="_blank" href="https://hub.docker.com/r/mahelbir/passokey"><img src="https://img.shields.io/docker/pulls/mahelbir/passokey" /></a>
<a target="_blank" href="https://hub.docker.com/r/mahelbir/passokey"><img src="https://img.shields.io/docker/v/mahelbir/passokey?label=docker%20image%20ver." /></a>


## ⭐ Features

- Passwordless authentication using Passkeys (WebAuthn/FIDO2)
- Multi-client architecture with SSO support
- Easy JWT-based integration integration with existing applications
- User permission management per client
- Admin panel for managing clients, users, and permissions

## 🔧 How to Install

### 🐳 Docker (Recommended)

```bash
mkdir passokey && cd passokey
mkdir data
curl -o docker-compose.yml https://raw.githubusercontent.com/mahelbir/passokey/main/docker-compose.yml
curl -o data/appsettings.json https://raw.githubusercontent.com/mahelbir/passokey/main/data/appsettings.json.example
```

Edit `data/appsettings.json` with your configuration:

```json
{
  "BaseUrl": , // Site URL (e.g., "https://auth.example.com")
  "AppName": , // Site name displayed to users
  "Session": {
    "IdleTimeoutMinutes": , // Session cleanup time after no activity
    "AuthorizedClientLifetimeMinutes": , // How long a user session remains valid after login
    "AdminSessionLifetimeMinutes":  // How long an admin session remains valid after login
  },
  "Fido2": {
    "ServerDomain": , // Your domain (e.g., "example.com")
    "ServerName": , // Display name shown in passkey prompt
    "Origins": [], // Additional allowed origins (BaseUrl is included automatically)
    "TimestampDriftTolerance":  // Allowed time difference (ms) between client and server clocks for authenticator timestamp validation
  },
  "Jwt": {
    "TokenLifetimeMinutes":  // JWT token validity period; typically used by callback URL to create a real session
  }
}
```

Then start the container:

```bash
docker compose up -d
```

> Passokey is now running on http://localhost:4050

### 💪🏻 Non-Docker

Requirements:

- [.NET 10 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)
- [Git](https://git-scm.com/downloads)

```bash
git clone https://github.com/mahelbir/passokey.git
cd passokey/Application

# Configure appsettings.json
cp ../data/appsettings.json.example App_Data/appsettings.json

# Run the application
dotnet run
```

### 🌐 Production Deployment

For production environments, you should expose Passokey behind a reverse proxy with HTTPS:

- **Nginx / Apache** - Traditional reverse proxy setup
- **Cloudflare Tunnel** - Zero-config secure tunnel without opening ports
- **Traefik** - Docker-native reverse proxy with automatic SSL
- **IIS** - Windows Server with ASP.NET Core Module
- etc.

> **Important:** WebAuthn/Passkeys require HTTPS in production (except for `localhost`)

## 🔐 Integration
The documentation is available at [docs folder](docs/INDEX.md).

## 🛠️ Technology Stack

| Technology | Purpose                                                                                                                                                        |
|------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| .NET 10 | MVC framework                                                                                                                                                  |
| SQLite | Database (file-based, no external dependencies)                                                                                                                |
| FIDO2 (WebAuthn) | Passkey authentication via [Fido2NetLib](https://github.com/passwordless-lib/fido2-net-lib) + [SimpleWebAuthn](https://www.npmjs.com/package/@simplewebauthn/browser) |
| JWT | Token-based authentication for client integration                                                                                                              |
| In-Memory Session | Session management (non-distributed)                                                                                                                           |

> **Note:** This project is designed as a lightweight, simple authentication server suitable for small to medium-scale deployments. If you need enterprise-level scalability, you can fork this project and extend it with:
> - Distributed databases (PostgreSQL, MySQL, SQL Server)
> - Redis or distributed cache for session management
> - Horizontal scaling with load balancers
> - Advanced logging and monitoring solutions

## 🖼 Screenshots


<img src="./docs/images/passkey-login.png" width="512" alt="Login" />
<img src="./docs/images/admin-clients.png" width="512" alt="Admin Clients" />
<img src="./docs/images/admin-permissions.png" width="512" alt="Admin Permissions" />


## 🗣️ Discussion / Bug Report

- [GitHub Issues](https://github.com/mahelbir/passokey/issues)

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
