using System.Linq;
using ECommerceApp.ConsoleClient;
using ECommerceApp.RyanW84;
using NetArchTest.Rules;
using Xunit;

namespace ECommerceApp.ArchitectureTests;

public class ClientServerSeparationTests
{
    [Fact]
    public void ApiAssemblyShouldNotDependOnConsoleClient()
    {
        var result = Types
            .InAssembly(typeof(Program).Assembly)
            .Should()
            .NotHaveDependencyOn("ECommerceApp.ConsoleClient")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailure(result));
    }

    [Fact]
    public void ConsoleClientShouldNotDependOnServerInternalLayers()
    {
        var forbidden = new[]
        {
            "ECommerceApp.RyanW84.Data",
            "ECommerceApp.RyanW84.Services",
            "ECommerceApp.RyanW84.Repositories",
            "ECommerceApp.RyanW84.Controllers",
            "Microsoft.EntityFrameworkCore"
        };

        var result = Types
            .InAssembly(typeof(ConsoleApp).Assembly)
            .Should()
            .NotHaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailure(result));
    }

    [Fact]
    public void ApiControllersShouldRemainUiAgnostic()
    {
        var result = Types
            .InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace("ECommerceApp.RyanW84.Controllers")
            .Should()
            .NotHaveDependencyOn("Spectre.Console")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailure(result));
    }

    private static string FormatFailure(TestResult result)
    {
        if (result.IsSuccessful)
        {
            return string.Empty;
        }

        var failing = result.FailingTypes.Select(type => type.FullName);
        return "Detected forbidden dependencies:\n" + string.Join("\n", failing);
    }
}
