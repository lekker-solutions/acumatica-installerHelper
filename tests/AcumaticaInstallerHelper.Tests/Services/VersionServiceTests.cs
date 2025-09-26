using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;
using AcumaticaInstallerHelper.Services;

namespace AcumaticaInstallerHelper.Tests.Services;

public class VersionServiceTests
{
    private readonly Mock<IConfigurationService> _configServiceMock;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ILogger<VersionService>> _loggerMock;
    private readonly Mock<HttpClient> _httpClientMock;
    private readonly VersionService _versionService;

    public VersionServiceTests()
    {
        _configServiceMock = new Mock<IConfigurationService>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _loggerMock = new Mock<ILogger<VersionService>>();
        _httpClientMock = new Mock<HttpClient>();
        
        _configServiceMock.Setup(x => x.GetAcumaticaDirectory()).Returns(@"C:\Acumatica");
        _configServiceMock.Setup(x => x.GetVersionDirectory()).Returns("Versions");
        
        _versionService = new VersionService(
            _configServiceMock.Object,
            _loggingServiceMock.Object,
            _loggerMock.Object,
            new HttpClient());
    }

    [Theory]
    [InlineData("24.100.0023")]
    [InlineData("23.200.0045")]
    [InlineData("25.105.0001")]
    public void ValidateVersionFormat_WithValidVersion_ShouldNotThrow(string version)
    {
        // Act & Assert
        _versionService.Invoking(x => x.ValidateVersionFormat(version))
            .Should().NotThrow();
    }

    [Theory]
    [InlineData("24.100")]
    [InlineData("24.100.23")]
    [InlineData("24.1000.0023")]
    [InlineData("invalid")]
    [InlineData("24.100.0023.1")]
    public void ValidateVersionFormat_WithInvalidVersion_ShouldThrow(string version)
    {
        // Act & Assert
        _versionService.Invoking(x => x.ValidateVersionFormat(version))
            .Should().Throw<VersionFormatException>()
            .WithMessage($"Version '{version}' is invalid. Expected format is ##.###.####");
    }

    [Fact]
    public void GetVersionPath_ShouldReturnCorrectPath()
    {
        // Arrange
        var version = "24.100.0023";

        // Act
        var path = _versionService.GetVersionPath(version);

        // Assert
        path.Should().Be(@"C:\Acumatica\Versions\24.100.0023");
    }

    [Fact]
    public void GetAcuExePath_ShouldReturnCorrectPath()
    {
        // Arrange
        var version = "24.100.0023";

        // Act
        var path = _versionService.GetAcuExePath(version);

        // Assert
        path.Should().Be(@"C:\Acumatica\Versions\24.100.0023\Data\ac.exe");
    }
}