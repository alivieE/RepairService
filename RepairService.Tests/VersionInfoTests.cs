using RepairService.Core;

namespace RepairService.Tests;

public class VersionInfoTests
{
    [Fact]
    public void Current_FromBuildProps_ReturnsVersionWithoutCommitHash()
    {
        // Act
        string version = VersionInfo.Current();

        // Assert
        Assert.Equal("9.9.9", version);
    }
}
