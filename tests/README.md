# Arkanis Overlay Tests

---

## 🧪 Unit and Integration Testing

To ensure the reliability and stability of the project, **unit tests** are implemented using the `xUnit` testing
framework.
Developers can run the tests locally using the following commands:

### Running All Tests

```bash
dotnet test
```

### Filtering Tests by Traits

You can filter test cases based on specific traits (e.g., types, categories) using the `--filter` CLI option.
For example, to run only tests that do not go against live external APIs (either do not need external APIs at all or use
locally cached data):

```bash
dotnet test --filter "DataState!=Live"
```

### Adding Traits to Tests

To categorize tests, use the `TraitAttribute` in your test methods.
Place trait names and their corresponding values in the `TestConstants` or other project-appropriate class.

```csharp
[Trait(nameof(TestConstants.Traits.DataSource), TestConstants.Traits.DataSource.ExternalApi)]
[Trait(nameof(TestConstants.Traits.DataState), TestConstants.Traits.DataState.Cached)]
public class CachedUexItemSyncRepositoryTest(ITestOutputHelper testOutputHelper, CachedUexSyncRepositoryTestFixture fixture)
{
    [Fact]
    public void TestMethod()
    {
        // Your test code here
    }
}
```

This allows for better organization and selective execution of tests during development.
