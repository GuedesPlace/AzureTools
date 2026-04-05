using GuedesPlace.AzureTools.Functions.FunctionRunContext;

namespace GuedesPlace.AzureTools.Tests.Functions;

/// <summary>
/// Helper that sets process environment variables for the duration of a test
/// and restores the original value on disposal. Safe for sequential tests.
/// </summary>
internal sealed class EnvVarScope : IDisposable
{
    private readonly Dictionary<string, string?> _originals = new();

    public EnvVarScope Set(string name, string? value)
    {
        _originals[name] = Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
        return this;
    }

    public void Dispose()
    {
        foreach (var (name, original) in _originals)
        {
            Environment.SetEnvironmentVariable(name, original, EnvironmentVariableTarget.Process);
        }
    }
}

public class AppFunctionRunContextTests
{
    [Fact]
    public void GetUserId_ReturnsApplicationId()
    {
        var ctx = new AppFunctionRunContext("my-app-id", []);
        Assert.Equal("my-app-id", ctx.GetUserId());
    }

    [Fact]
    public void FunctionRunContextType_IsApplication()
    {
        var ctx = new AppFunctionRunContext("app", []);
        Assert.Equal(FunctionRunContextType.APPLICATION, ctx.FunctionRunContextType);
    }

    [Fact]
    public void IsAuthenticated_ReturnsFalse_ByDefault()
    {
        // AppFunctionRunContext does not set _authenticated
        var ctx = new AppFunctionRunContext("app", []);
        Assert.False(ctx.IsAuthenticated());
    }

    [Fact]
    public void IsDev_WhenAzureFunctionsEnvironmentIsDevelopmentAndIsNotDevAbsent_ReturnsTrue()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Development")
            .Set("IS_NOT_DEV", null);

        var ctx = new AppFunctionRunContext("app", []);
        Assert.True(ctx.IsDev());
    }

    [Fact]
    public void IsDev_WhenAzureFunctionsEnvironmentIsNotDevelopment_ReturnsFalse()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Production")
            .Set("IS_NOT_DEV", null);

        var ctx = new AppFunctionRunContext("app", []);
        Assert.False(ctx.IsDev());
    }

    [Fact]
    public void IsDev_WhenIsNotDevIsSet_ReturnsFalse()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Development")
            .Set("IS_NOT_DEV", "true");

        var ctx = new AppFunctionRunContext("app", []);
        Assert.False(ctx.IsDev());
    }

    [Fact]
    public void GetEnvironmentVariable_ReturnsProcessVariable()
    {
        using var scope = new EnvVarScope().Set("TEST_VAR_AZURE_TOOLS", "hello");
        var ctx = new AppFunctionRunContext("app", []);
        Assert.Equal("hello", ctx.GetEnvironmentVariable("TEST_VAR_AZURE_TOOLS"));
    }

    [Fact]
    public void GetEnvironmentVariable_WithMissingVariable_ReturnsNull()
    {
        // Ensure the variable is not set
        using var scope = new EnvVarScope().Set("TEST_MISSING_VAR_AZURE_TOOLS", null);
        var ctx = new AppFunctionRunContext("app", []);
        Assert.Null(ctx.GetEnvironmentVariable("TEST_MISSING_VAR_AZURE_TOOLS"));
    }
}
