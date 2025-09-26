using FluentAssertions;
using Xunit;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Tests.Models;

public class AcumaticaVersionTests
{
    [Theory]
    [InlineData("24.100.0023", "24.100")]
    [InlineData("23.200.0045", "23.200")]
    [InlineData("25.105.0001", "25.105")]
    public void MajorVersion_ShouldReturnCorrectMajorVersion(string version, string expectedMajor)
    {
        // Arrange
        var acuVersion = new AcumaticaVersion { Version = version };

        // Act
        var majorVersion = acuVersion.MajorVersion;

        // Assert
        majorVersion.Should().Be(expectedMajor);
    }

    [Fact]
    public void MajorVersion_WithEmptyVersion_ShouldReturnEmpty()
    {
        // Arrange
        var acuVersion = new AcumaticaVersion { Version = "" };

        // Act
        var majorVersion = acuVersion.MajorVersion;

        // Assert
        majorVersion.Should().BeEmpty();
    }

    [Fact]
    public void MajorVersion_WithSinglePart_ShouldReturnWhole()
    {
        // Arrange
        var acuVersion = new AcumaticaVersion { Version = "24" };

        // Act
        var majorVersion = acuVersion.MajorVersion;

        // Assert
        majorVersion.Should().Be("24");
    }
}