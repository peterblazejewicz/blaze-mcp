using Blaze.MCP;
using Blaze.MCP.Tools;

using FluentAssertions;
using Xunit;

namespace Blaze.MCP.Tests;

public class RandomNumberToolsTests
{
    [Fact]
    public void GetRandomNumber_ReturnsWithinRange()
    {
        var tools = new RandomNumberTools();
        var n = tools.GetRandomNumber(1, 3);
        n.Should().BeGreaterOrEqualTo(1).And.BeLessThan(3);
    }
}
