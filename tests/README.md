# Tests

This independently published package keeps its runnable tests in
`Test.Tairitsua.Monica.GachaPool`. The publisher-first name intentionally
overrides Monica's first-party `Test.Monica.*` repository convention.

The host tests register only the graph-entry GachaPool UI module so its
infrastructure, localization, and shell dependencies must compose transitively.
The remaining scenarios exercise the public builder, typed catalog, Facade,
inventory, serialization, host isolation, and concurrency behavior.

Run one test process at a time. The test project consumes the repository-pinned Monica 1.0.0-rc.12 packages:

```bash
dotnet test tests/Test.Tairitsua.Monica.GachaPool/Test.Tairitsua.Monica.GachaPool.csproj
```
