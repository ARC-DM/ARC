<h1 align="center">
  <picture>
    <source height="125" media="(prefers-color-scheme: dark)" srcset="https://github.com/Ethernalcom/Arc/raw/master/docs/static/ARC-Logo-Light.svg">
    <img height="125" alt="Arc Logo" src="https://github.com/Ethernalcom/Arc/raw/master/docs/static/ARC-Logo-Dark.svg">
  </picture>
</h1>

<p align="center">
  <img height="40" src="https://github.com/Ethernalcom/Arc/raw/master/docs/static/built_with_c%23.svg" alt="Built with C#">
</p>

<p align="center">
  <a href="https://github.com/Ethernalcom/Arc/actions?query=workflow%3ABuild">
    <img src="https://img.shields.io/github/actions/workflow/status/Ethernalcom/Arc/build.yml?branch=main&label=%E2%9A%99%EF%B8%8F%20build&style=flat-square&color=512bd4">
  </a>
  <a href="https://github.com/Ethernalcom/Arc/releases">
    <img src="https://img.shields.io/github/v/release/Ethernalcom/Arc?include_prereleases&label=%F0%9F%93%A6%20release&style=flat-square&color=512bd4">
  </a>
  <a href="https://dotnet.microsoft.com/en-us/download">
    <img src="https://img.shields.io/badge/%F0%9F%92%BF%20platform-windows-0078d4?style=flat-square">
  </a>
  <a href="#-roadmap--upcoming-limitations">
    <img src="https://img.shields.io/badge/%F0%9F%96%BC%EF%B8%8F%20custom%20isos-upcoming-orange?style=flat-square">
  </a>
</p>

<p align="center">
  <em><b>Arc</b> is an open-source, lightweight <b>remote device management tool</b> built with <b>C#</b>. Engineered specifically for Windows environments, Arc aims to provide an elegant, transparent, and hyper-fast alternative to traditional MDM solutions like Microsoft Intune.</em>
</p>

<p align="center">
  <a href="#-installation">Installation</a> •
  <a href="#-project-contents">Project Contents</a> •
  <a href="#-architecture">Architecture</a> •
  <a href="#-features">Features</a> •
  <a href="#-philosophy">Philosophy</a> •
  <a href="#-roadmap--upcoming-limitations">Roadmap</a>
</p>

---

## ⚙️ Installation

Arc requires the **.NET 10.0 Runtime or SDK** (or higher) to execute natively on Windows machines. 

## 📦 Project Contents

Arc is modularly split into 4 distinct C# sub-projects designed to work in tandem:

### 💻 ArcConsole

ArcConsole (`ARC-C`) is our state-of-the-art administrative console for system administrators, allowing them to issue real-time commands to every device or select asset groups, alongside managing global software deployment, system updates, and endpoint configurations.

### ⚙️ ArcDaemon

ArcDaemon (`ARC-D`) is our high-performance background utility engine that interfaces directly with the native Windows OS ecosystem. It acts as the core communication bridge connecting the backend environment with our administrative and user-facing frontend components.

### 🌐 ArcPortal

ArcPortal (`ARC-P`) is our end-user interface portal, granting workers the autonomy to install approved software packages and manage authorized device settings locally—strictly adhering to governance limits defined inside ArcConsole.

### 🔗 ArcShared

ArcShared (`ARC-S`) is our centralized helper and data layer framework. It acts as the shared single source of truth for mandatory asset schemas, serialization data structures, and foundational command references required across the ecosystem.

## 🔀 Architecture

The system operates via a tightly coupled, highly responsive mesh routing schema:

```
    ┌───────────────┐               ┌───────────────┐
    │  ArcConsole   │               │   ArcPortal   │
    │  (Admin App)  │               │  (User App)   │
    └───────┬───────┘               └───────┬───────┘
            │                               │
            └───────────────┬───────────────┘
                            │  Encrypted Websocket Hooks
                            ▼
                    ┌───────────────┐
                    │   ArcDaemon   │ <───> [ Windows Native OS ]
                    │ (Target Agent)│
                    └───────▲───────┘
                            │ Dependencies
                    ┌───────┴───────┐
                    │   ArcShared   │
                    │ (Common Core) │
                    └───────────────┘

```

## 🎯 Features

* **Real-time Customisable Telemetry:** Stream running processes, resource loads, and network interfaces instantly.
* **Remote Script Execution:** Deploy signed PowerShell scripts silently to one machine or grouped clusters.
* **Enterprise Software Deployment:** Remotely install, track, or uninstall core Windows applications (`.msi`, `.exe`).
* **Low-Footprint Background Execution:** Runs cleanly as a native Windows Service with minimum memory overhead.
* **Encrypted Control Transport:** End-to-end transport encryption utilizing high-performance WebSockets.

## 💡 Philosophy

System administrators moving away from classic on-premise setups often find cloud-based alternatives like Microsoft Intune slow to sync, overly opaque, and burdened by heavy platform licensing.

**Arc** follows a different path: **immediacy and transparency**. Built purely on C# and native Windows APIs, Arc allows system actions, configuration profile pushes, and telemetry checks to trigger on the target endpoint within milliseconds instead of hours. It bridges the gap between high-overhead enterprise MDM and rapid, scriptable endpoint operations.

## 🗺️ Roadmap / Upcoming Limitations

* **Custom ISO Provisioning (In Progress):** Tools to easily bake the Arc agent client, predefined profile configurations, and local system rules directly into fresh Windows ISO setups.
* **Cross-Platform Agent Core:** Porting core monitoring blocks into .NET Standard to begin supporting headless Linux distribution assets later down the line.

---

## 👍 Contribute

If you want to say **Thank You** and support the development of `Arc`:

1. Add a **GitHub Star** ⭐ to the project.
2. Submit bug reports or feature ideas through the repository [Issues](https://github.com/Ethernalcom/Arc/issues) tab!
