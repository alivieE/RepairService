using RepairService.Core.Reporting;

namespace RepairService.Tests;

public class ReportBuilderTests
{
    [Fact]
    public void Build_TwoRows_TitleInUpperCaseThenRows()
    {
        // Arrange
        var builder = new ReportBuilder { Title = "Заявки" };
        string[] rows = { "WKR-1", "PRT-7" };
        string nl = Environment.NewLine;

        // Act
        string text = builder.Build(rows);

        // Assert
        Assert.Equal("ЗАЯВКИ" + nl + "WKR-1" + nl + "PRT-7" + nl, text);
    }

    [Fact]
    public void Build_NoRows_ReturnsOnlyDefaultTitle()
    {
        // Act
        string text = new ReportBuilder().Build(Array.Empty<string>());

        // Assert
        Assert.Equal("ЗВІТ" + Environment.NewLine, text);
    }

    [Fact]
    public void Build_NullRows_ThrowsArgumentNull()
    {
        // Arrange
        var builder = new ReportBuilder();

        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => builder.Build(null!));
    }
}
