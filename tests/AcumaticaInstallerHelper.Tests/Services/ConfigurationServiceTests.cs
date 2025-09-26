using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;
using AcumaticaInstallerHelper.Services;
using AcumaticaInstallerHelper.Models;

namespace AcumaticaInstallerHelper.Tests.Services;

public class ConfigurationServiceTests
{
    private readonly Mock<ILogger<ConfigurationService>> _loggerMock;
    private readonly ConfigurationService _configService;

    public ConfigurationServiceTests()
    {
        _loggerMock = new Mock<ILogger<ConfigurationService>>();
        _configService = new ConfigurationService(_loggerMock.Object);
    }

    [Fact]
    public void GetConfiguration_WhenNoConfigFileExists_ShouldReturnDefaults()
    {
        // Act
        var config = _configService.GetConfiguration();

        // Assert
        config.Should().NotBeNull();
        config.AcumaticaDirectory.Should().Be(@"C:\Acumatica");
        config.SiteDirectory.Should().Be("Sites");
        config.VersionDirectory.Should().Be("Versions");
        config.SiteType.Should().Be("Production");
        config.InstallDebugTools.Should().BeFalse();
    }

    [Fact]
    public void GetAcumaticaDirectory_ShouldReturnConfiguredValue()
    {
        // Act
        var directory = _configService.GetAcumaticaDirectory();

        // Assert
        directory.Should().Be(@"C:\Acumatica");
    }

    [Fact]
    public void GetDefaultSiteType_ShouldReturnProduction_WhenConfigIsProduction()
    {
        // Act
        var siteType = _configService.GetDefaultSiteType();

        // Assert
        siteType.Should().Be(SiteType.Production);
    }

    [Fact]
    public void GetDefaultSiteInstallPath_ShouldCombineDirectories()
    {
        // Act
        var path = _configService.GetDefaultSiteInstallPath();

        // Assert
        path.Should().Be(@"C:\Acumatica\Sites");
    }
}