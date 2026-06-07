<h1 align="center">
  <picture>
    <source height="125" media="(prefers-color-scheme: dark)" srcset="https://github.com/ARC-DM/ARC/raw/master/docs/static/ARC-Logo-Light.svg">
    <img height="125" alt="Arc Logo" src="https://github.com/ARC-DM/ARC/raw/master/docs/static/ARC-Logo-Dark.svg">
  </picture>
</h1>

<p align="center">
  <img height="80" src="https://github.com/ARC-DM/ARC/raw/master/docs/static/built_with_c%23.svg" alt="Built with C#">
  <img height="80" src="https://github.com/ARC-DM/ARC/raw/master/docs/static/project_in_alpha.svg" alt="Project in ALPHA">
</p>

<p align="center">
  <a href="https://github.com/ARC-DM/ARC/actions?query=workflow%3ABuild">
    <img src="https://img.shields.io/github/actions/workflow/status/ARC-DM/ARC/build.yml?branch=main&label=%E2%9A%99%EF%B8%8F%20build&style=flat-square&color=512bd4">
  </a>
  <a href="https://github.com/ARC-DM/ARC/releases">
    <img src="https://img.shields.io/github/v/release/ARC-DM/ARC?include_prereleases&label=%F0%9F%93%A6%20release&style=flat-square&color=512bd4">
  </a>
  <a href="https://dotnet.microsoft.com/en-us/download">
    <img src="https://img.shields.io/badge/%F0%9F%92%BF%20platform-windows-0078d4?style=flat-square">
  </a>
  <a href="#%EF%B8%8F-roadmap">
    <img src="https://img.shields.io/badge/%F0%9F%96%BC%EF%B8%8F%20custom%20isos-upcoming-orange?style=flat-square">
  </a>
  <a href="https://github.com/ARC-DM/ARC/blob/master/LICENSE">
    <img src="https://img.shields.io/github/license/ARC-DM/ARC?style=flat-square&color=512bd4">
  </a>
</p>

<p align="center">
  <em><b>Arc</b> is an open-source, lightweight <b>remote device management platform</b> built with <b>C# and .NET 10</b>.<br/>
  Engineered for Windows environments, Arc delivers a transparent, fast, and developer-friendly alternative to heavyweight MDM solutions like Microsoft Intune.</em>
</p>

<p align="center">
  <a href="#%EF%B8%8F-installation">Installation</a> •
  <a href="#-project-contents">Project Contents</a> •
  <a href="#-architecture">Architecture</a> •
  <a href="#-command-reference">Commands</a> •
  <a href="#-features">Features</a> •
  <a href="#-philosophy">Philosophy</a> •
  <a href="#%EF%B8%8F-roadmap">Roadmap</a> •
  <a href="#-contributing">Contributing</a>
</p>

---

> [!CAUTION]
> ARC is under active development. APIs, command syntax, and internal structure are subject to change between releases. Not recommended for production use yet.

## ⚙️ Installation

Arc requires the **.NET 10.0 Runtime or SDK** (or higher) to execute natively on Windows machines. 

## 📦 Project Contents

Arc is split into four focused C# projects designed to work together:

### 💻 ArcConsole (`ARC-C`)
The IT administrator interface. Issue real-time commands to individual devices or entire fleets, manage software deployments, push configuration changes, and monitor device health — all from a single, fast console built with WPF and WebView2.

### ⚙️ ArcDaemon (`ARC-D`)
The background engine deployed on every managed device. Runs as a native Windows Service with SYSTEM-level privileges, receives authenticated commands, executes them, and streams results back in real time. ArcDaemon is the core of Arc — everything else talks to it.

### 🌐 ArcPortal (`ARC-P`)
The end-user self-service interface. Lets employees install approved software, check their device status, and manage permitted settings — all within boundaries defined by the IT admin in ArcConsole. Think Microsoft Company Portal, but faster and fully under your control.

### 🔗 ArcShared (`ARC-S`)
The shared contract layer. Defines the command and message structures (`ArcCommand`, `ArcMessage`) used across all three projects, ensuring every component speaks the same language. Neither ArcConsole, ArcDaemon nor ArcPortal can disagree on data shapes — ArcShared is the single source of truth.

## 🔀 Architecture

Arc uses a hub-and-spoke model. ArcConsole and ArcPortal send structured JSON commands over encrypted named pipes to ArcDaemon on the target device. Every command carries the identity of the requesting tool (`ARC-C` or `ARC-P`) and the requester, allowing ArcDaemon to enforce permission levels before executing anything.

```
    ┌───────────────┐               ┌───────────────┐
    │  ArcConsole   │               │   ArcPortal   │
    │    (ARC-C)    │               │    (ARC-P)    │
    └───────┬───────┘               └───────┬───────┘
            │                               │
            └───────────────┬───────────────┘
                            │  Named Pipe (local) / Network Pipe (remote)
                            │  Structured JSON · Permission-enforced
                            ▼
                    ┌───────────────┐
                    │   ArcDaemon   │ ◄──► Windows Native OS
                    │    (ARC-D)    │      SYSTEM privileges
                    └───────▲───────┘
                            │
                    ┌───────┴───────┐
                    │   ArcShared   │
                    │    (ARC-S)    │
                    └───────────────┘
```

---

## 📋 Command Reference

Commands follow the syntax: `ACTION --param value --param2 value2`

A selection of available commands:

| Command | Permission | Description |
|---|---|---|
| `PING` | Any | Returns machine name, OS, uptime and .NET version |
| `WHOAMI` | Any | Returns requesting tool, requester, daemon user and session info |
| `SYSREPORT` | Any | Full system snapshot: hardware, processes, services, network, disk |
| `PROCESSES` | Admin | Lists all running processes sorted by memory usage |
| `SERVICES` | Admin | Lists all Windows services and their status |
| `DISKUSAGE` | Any | Returns disk space per drive |
| `GETIP` | Any | Returns network adapter info and IP addresses |
| `INSTALL` | Admin / Portal | Installs an application via winget |
| `UNINSTALL` | Admin | Uninstalls an application via winget |
| `RUNSCRIPT` | Admin | Executes a PowerShell script on the target device |
| `KILL` | Admin | Kills a running process by name or PID |
| `GETENV` | Any | Returns environment variables |
| `GETREG` | Admin | Reads a registry value |
| `SETREG` | Admin | Writes a registry value |
| `RESTART` | Admin | Restarts the machine |
| `SHUTDOWN` | Admin | Shuts down the machine |

For the full command reference including all parameters and permission levels, see [docs/COMMANDS.md](docs/COMMANDS.md).

---


## 🎯 Features

- **Real-time streaming output** — Progress messages arrive as they happen, not after the fact. Watch script output line by line, live.
- **Permission-layered command execution** — Every command is tagged with who sent it and from which tool. ArcDaemon enforces what each tool and user is allowed to do.
- **winget integration** — Install, uninstall, and update applications on managed devices using the Windows Package Manager.
- **PowerShell & batch execution** — Deploy and run scripts remotely with full stdout streaming back to the console.
- **Multi-client architecture** — ArcConsole and ArcPortal can connect to the same ArcDaemon simultaneously. Each connection is handled on its own async task.
- **Low-footprint daemon** — ArcDaemon is a lean Worker Service. No bloated agent, no unnecessary dependencies.
- **Developer-first design** — Built entirely on .NET 10, open source, and designed to be extended. Adding a new command is two steps: write a handler, register it.

## 💡 Philosophy

System administrators moving away from on-premise setups often find cloud-based MDM solutions like Microsoft Intune slow to sync, opaque about what's actually happening on the device, and expensive to license.

ARC takes a different approach: **immediacy, transparency, and control.**

- Commands reach the target device and begin executing in milliseconds, not minutes or hours
- Every action is auditable — ARC logs what ran, who ran it, and what the result was
- Nothing is hidden behind a cloud sync cycle — if you tell a device to install something, it happens now
- The codebase is open and readable — you can see exactly what ARC does and doesn't do on your devices

ARC is not trying to replace Intune for every enterprise. It's for teams that want speed, transparency, and the ability to extend their management tooling without waiting on a vendor.

---

## 🗺️ Roadmap

### In Progress
- [ ] Full command handler set (PROCESSES, SERVICES, DISKUSAGE, RUNSCRIPT, INSTALL, KILL, and more)
- [ ] Permission layer enforcement in ArcDaemon
- [ ] Audit logging — every command logged with requester, timestamp and result
- [ ] ArcPortal initial build

### Planned
- [ ] Arc Server — central device registry, command queue, offline delivery, App Catalogue API
- [ ] Custom ISO — a custom ISO injected with ARC so you can get started straight away
- [ ] Fleet management in ArcConsole — device list, group targeting, broadcast commands
- [ ] LDAP / Active Directory identity integration
- [ ] OIDC authentication (Azure AD, Okta, Keycloak)
- [ ] MSI installer with silent deploy support
- [ ] Custom ISO provisioning — bake ArcDaemon into a Windows ISO for zero-touch deployment
- [ ] Cross-platform agent core — headless Linux support via .NET Standard

### Known Limitations
- Remote connections over the network are designed but not yet hardened for production
- No authentication layer yet — currently relies on Windows pipe ACLs and network trust
- ArcPortal is planned but not yet built

---

## 👍 Contribute

**Ways to contribute:**
- Browse [open issues](https://github.com/ARC-DM/ARC/issues) and pick something up
- Propose a new command via the [Command Proposal](.github/ISSUE_TEMPLATE/command_proposal.md) template
- Report a bug or unexpected behaviour
- Improve documentation

If you want to say **Thank You** and support the development of `ARC`:

1. Add a **GitHub Star** ⭐ to the project.
2. Submit bug reports or feature ideas through the repository [Issues](https://github.com/Ethernalcom/Arc/issues) tab!

If you find a security vulnerability, please do **not** open a public issue. Use the [Security Vulnerability](.github/ISSUE_TEMPLATE/security_vulnerability.md) template or contact the maintainers directly.

---

<p align="center">
  Built with C# · .NET 10 · Windows · Open Source
</p>
