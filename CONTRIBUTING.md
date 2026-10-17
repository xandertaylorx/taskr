# Contributing

Thanks for helping improve taskr!

1. Fork the repo and create a branch: `git checkout -b my-feature`
2. Make your change and add an xUnit test for it
3. Run `dotnet test`
4. Open a pull request describing what and why

## Guidelines

- Keep domain logic in `TaskList` so it stays easy to test
- Never touch the real filesystem or clock in tests; use the in-memory store and the injected clock
- Small commits with imperative messages ("Add X", not "Added X")

## Reporting bugs

Please include the command you ran, the output you expected and the output you got.
