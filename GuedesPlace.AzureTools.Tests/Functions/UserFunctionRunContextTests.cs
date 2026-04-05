using System.Text;
using System.Text.Json;
using GuedesPlace.AzureTools.Functions.FunctionRunContext;
using Microsoft.AspNetCore.Http;

namespace GuedesPlace.AzureTools.Tests.Functions;

public class UserFunctionRunContextTests
{
    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static HttpRequest BuildRequestWithHeader(string headerName, string headerValue)
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Headers[headerName] = headerValue;
        return ctx.Request;
    }

    private static HttpRequest BuildRequestWithoutHeaders()
    {
        return new DefaultHttpContext().Request;
    }

    private static string BuildClientPrincipalHeader(
        string identityProvider,
        string objectId,
        string? upn = null,
        IEnumerable<string>? roles = null)
    {
        var claims = new List<object>
        {
            new { typ = "http://schemas.microsoft.com/identity/claims/objectidentifier", val = objectId }
        };
        if (upn != null)
        {
            claims.Add(new { typ = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn", val = upn });
        }
        foreach (var role in roles ?? [])
        {
            claims.Add(new { typ = "roles", val = role });
        }

        var principal = new
        {
            auth_typ = identityProvider,
            name_typ = "name",
            role_typ = "roles",
            claims
        };

        var json = JsonSerializer.Serialize(principal);
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
    }

    // ─── Dev mode ────────────────────────────────────────────────────────────

    [Fact]
    public void DevMode_WithDevUserId_GetUserIdReturnsEnvVarValue()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Development")
            .Set("IS_NOT_DEV", null)
            .Set("DEV_USER_ID", "dev-user-42")
            .Set("DEV_USER_ROLES", null);

        var ctx = new UserFunctionRunContext(BuildRequestWithoutHeaders());

        Assert.Equal("dev-user-42", ctx.GetUserId());
    }

    [Fact]
    public void DevMode_WithoutDevUserId_DefaultsToLocalDev()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Development")
            .Set("IS_NOT_DEV", null)
            .Set("DEV_USER_ID", null)
            .Set("DEV_USER_ROLES", null);

        var ctx = new UserFunctionRunContext(BuildRequestWithoutHeaders());

        Assert.Equal("LocalDev", ctx.GetUserId());
    }

    [Fact]
    public void DevMode_IsDevReturnsTrue()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Development")
            .Set("IS_NOT_DEV", null)
            .Set("DEV_USER_ID", null)
            .Set("DEV_USER_ROLES", null);

        var ctx = new UserFunctionRunContext(BuildRequestWithoutHeaders());

        Assert.True(ctx.IsDev());
    }

    [Fact]
    public void DevMode_IsAuthenticatedReturnsTrue()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Development")
            .Set("IS_NOT_DEV", null)
            .Set("DEV_USER_ID", "dev-user")
            .Set("DEV_USER_ROLES", null);

        var ctx = new UserFunctionRunContext(BuildRequestWithoutHeaders());

        Assert.True(ctx.IsAuthenticated());
    }

    [Fact]
    public void DevMode_WithDevRoles_SetsRolesFromEnvVar()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Development")
            .Set("IS_NOT_DEV", null)
            .Set("DEV_USER_ID", "dev-user")
            .Set("DEV_USER_ROLES", "admin,reader");

        var ctx = new UserFunctionRunContext(BuildRequestWithoutHeaders());

        Assert.True(ctx.IsInAtLeastOneRole("admin"));
        Assert.True(ctx.IsInAtLeastOneRole("reader"));
        Assert.False(ctx.IsInAtLeastOneRole("writer"));
    }

    [Fact]
    public void DevMode_WithoutDevRoles_DefaultsToAdminRole()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Development")
            .Set("IS_NOT_DEV", null)
            .Set("DEV_USER_ID", "dev-user")
            .Set("DEV_USER_ROLES", null);

        var ctx = new UserFunctionRunContext(BuildRequestWithoutHeaders());

        Assert.True(ctx.IsInAtLeastOneRole("admin"));
    }

    [Fact]
    public void DevMode_FunctionRunContextType_IsUser()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Development")
            .Set("IS_NOT_DEV", null)
            .Set("DEV_USER_ID", null)
            .Set("DEV_USER_ROLES", null);

        var ctx = new UserFunctionRunContext(BuildRequestWithoutHeaders());

        Assert.Equal(FunctionRunContextType.USER, ctx.FunctionRunContextType);
    }

    // ─── Production mode (x-ms-client-principal header) ──────────────────────

    [Fact]
    public void ProdMode_WithValidHeader_GetUserIdReturnsObjectIdentifier()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Production")
            .Set("IS_NOT_DEV", "true");

        var header = BuildClientPrincipalHeader("aad", "user-oid-123");
        var request = BuildRequestWithHeader("x-ms-client-principal", header);
        var ctx = new UserFunctionRunContext(request);

        Assert.Equal("user-oid-123", ctx.GetUserId());
    }

    [Fact]
    public void ProdMode_WithValidHeader_IsAuthenticatedReturnsTrue()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Production")
            .Set("IS_NOT_DEV", "true");

        var header = BuildClientPrincipalHeader("aad", "user-oid-456");
        var request = BuildRequestWithHeader("x-ms-client-principal", header);
        var ctx = new UserFunctionRunContext(request);

        Assert.True(ctx.IsAuthenticated());
    }

    [Fact]
    public void ProdMode_WithUpnClaim_GetUPNReturnsValue()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Production")
            .Set("IS_NOT_DEV", "true");

        var header = BuildClientPrincipalHeader("aad", "user-oid", upn: "user@example.com");
        var request = BuildRequestWithHeader("x-ms-client-principal", header);
        var ctx = new UserFunctionRunContext(request);

        Assert.Equal("user@example.com", ctx.GetUPN());
    }

    [Fact]
    public void ProdMode_WithRoleClaims_RoleChecksWork()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Production")
            .Set("IS_NOT_DEV", "true");

        var header = BuildClientPrincipalHeader("aad", "user-oid", roles: ["admin", "reader"]);
        var request = BuildRequestWithHeader("x-ms-client-principal", header);
        var ctx = new UserFunctionRunContext(request);

        Assert.True(ctx.IsInAtLeastOneRole("admin"));
        Assert.True(ctx.IsInAtLeastOneRole("reader"));
        Assert.False(ctx.IsInAtLeastOneRole("writer"));
    }

    [Fact]
    public void ProdMode_WithoutHeader_IsAuthenticatedReturnsFalse()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Production")
            .Set("IS_NOT_DEV", "true");

        var ctx = new UserFunctionRunContext(BuildRequestWithoutHeaders());

        Assert.False(ctx.IsAuthenticated());
    }

    [Fact]
    public void ProdMode_WithoutHeader_GetClaimsPrincipalReturnsNull()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Production")
            .Set("IS_NOT_DEV", "true");

        var ctx = new UserFunctionRunContext(BuildRequestWithoutHeaders());

        Assert.Null(ctx.GetClaimsPrincipal());
    }

    [Fact]
    public void ProdMode_WithValidHeader_GetClaimsPrincipalReturnsNonNull()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Production")
            .Set("IS_NOT_DEV", "true");

        var header = BuildClientPrincipalHeader("aad", "user-oid");
        var request = BuildRequestWithHeader("x-ms-client-principal", header);
        var ctx = new UserFunctionRunContext(request);

        Assert.NotNull(ctx.GetClaimsPrincipal());
    }

    [Fact]
    public void ProdMode_IsDevReturnsFalse()
    {
        using var scope = new EnvVarScope()
            .Set("AZURE_FUNCTIONS_ENVIRONMENT", "Production")
            .Set("IS_NOT_DEV", "true");

        var ctx = new UserFunctionRunContext(BuildRequestWithoutHeaders());

        Assert.False(ctx.IsDev());
    }
}
