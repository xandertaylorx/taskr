<h1 align="center">taskr</h1>

<p align="center">
  A fast, friendly to-do list for your terminal. One command to add, one to finish.
</p>

<p align="center">
  <a href="https://github.com/xandertaylorx/taskr/actions/workflows/ci.yml"><img alt="CI" src="https://github.com/xandertaylorx/taskr/actions/workflows/ci.yml/badge.svg"></a>
  <img alt=".NET 10" src="https://img.shields.io/badge/.NET-10-512bd4">
  <img alt="License: MIT" src="https://img.shields.io/badge/license-MIT-green">
  <img alt="Dependencies: none" src="https://img.shields.io/badge/dependencies-none-brightgreen">
</p>

## Features

- **Add tasks in one line** with due dates, priorities and tags
- **Friendly dates**: `today`, `tomorrow`, `fri`, `+3d`, `+2w` or `2026-12-25`
- **Smart ordering**: soonest due date first, then highest priority
- **Overdue warnings** so nothing slips
- **Plain JSON storage** you can read, back up or sync with git
- Cross-platform, with no dependencies beyond the .NET runtime

## Install

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/xandertaylorx/taskr.git
cd taskr
dotnet pack src/Taskr -o nupkg
dotnet tool install --global --add-source nupkg Taskr
```

Or run it straight from source without installing:

```bash
dotnet run --project src/Taskr -- list
```

## Usage

```bash
taskr add "Finish uni report" --due +3d --priority high --tag uni
taskr add "Buy milk"
taskr list
taskr done 2
taskr list --all
```

```text
ID       PRI   DUE         TASK               TAGS
1   [ ]  high  2026-10-20  Finish uni report  #uni
2   [x]  med   -           Buy milk

1 open, 1 done
```

### Commands

| Command | Description |
| --- | --- |
| `taskr add <title>` | Add a task. Options: `--due`/`-d`, `--priority`/`-p` (`low`, `medium`, `high`), `--tag`/`-t` (repeatable) |
| `taskr list` | Show open tasks. Options: `--all`/`-a`, `--done`, `--tag`/`-t` |
| `taskr done <id>` | Mark a task complete |
| `taskr undo <id>` | Reopen a completed task |
| `taskr rm <id>` | Delete a task |
| `taskr clear` | Delete all completed tasks |
| `taskr --help`, `--version` | Show help or the version |

Running `taskr` with no command is the same as `taskr list`.

### Due dates

| Input | Meaning |
| --- | --- |
| `today`, `tomorrow` | Relative to now |
| `mon` ... `sun` | The next time that weekday occurs |
| `+3d`, `+2w` | Days or weeks from today |
| `2026-12-25` | An exact date |

### Where are my tasks?

In `~/.taskr/tasks.json`. Set the `TASKR_FILE` environment variable to use a different file, for example one inside a synced folder or a project.

## How it works

| File | Responsibility |
| --- | --- |
| `Models/` | Immutable `TaskItem` record and `Priority` enum |
| `TaskList.cs` | All task operations (add, complete, reopen, remove, clear) |
| `TaskStore.cs` | `ITaskStore` interface and an atomic JSON implementation |
| `DateParser.cs` | Friendly date parsing |
| `Formatter.cs` | Aligned table output |
| `App.cs` | Command parsing and dispatch, with the store and clock injected for tests |

## Development

```bash
dotnet build
dotnet test
```

The CLI is tested end to end against an in-memory store and a fixed clock, so tests are fast and deterministic. See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

## Roadmap

- [ ] `taskr edit <id>` to change a title, date or tags
- [ ] Recurring tasks
- [ ] Coloured output
- [ ] Export to Markdown or CSV

## Licence

[MIT](LICENSE) © Xander Taylor
