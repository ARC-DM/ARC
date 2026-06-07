---
name: Command Proposal
about: Propose a new ArcDaemon command
title: "[COMMAND] "
labels: command-proposal
assignees: ''
---

## Command Name
e.g. `GETBATTERY`

## Syntax
```
COMMAND --param1 value --param2 value
```

## Description
What does this command do? Be specific about what it reads, writes, or executes on the target device.

## Permission Level
What permission level should this command require? Explain your reasoning.

- [ ] **Any** — Safe for ArcPortal users to call
- [ ] **Admin / Portal** — Available to ArcPortal only when explicitly permitted by an admin
- [ ] **Admin** — ArcConsole only

## Expected Output
What should the command return? Provide an example of what the response payload would look like.

```
Example output here
```

## Parameters

| Parameter | Required | Description |
|---|---|---|
| `--param` | Yes / No | What it does |

## Implementation Notes
Any notes on how this could be implemented — which Windows APIs, .NET classes, or external tools would be involved?

## Use Case
Describe a real scenario where an IT admin or end user would need this command.

## Related Commands
Are there any existing commands this is similar to or would complement?
