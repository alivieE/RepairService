using RepairService.Core.Domain;
using RepairService.Core.Input;

namespace RepairService.Tests;

public class RepairLineParserTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("WKR-1;2")]
    [InlineData("WKR-1;2;250.50;1")]
    [InlineData(";2;250.50")]
    [InlineData("ABCDEFGHIJKLMNOPQRSTU;2;250.50")]
    [InlineData("WKR-1;0;250.50")]
    [InlineData("WKR-1;-2;250.50")]
    [InlineData("WKR-1;1001;250.50")]
    [InlineData("WKR-1;два;250.50")]
    [InlineData("WKR-1;2;-10.50")]
    [InlineData("WKR-1;2;100000.01")]
    [InlineData("WKR-1;2;abc")]
    public void TryParse_InvalidInput_ReturnsFalse(string raw)
    {
        // Act
        bool ok = RepairLineParser.TryParse(raw, out RepairLine? line);

        // Assert
        Assert.False(ok);
        Assert.Null(line);
    }

    [Fact]
    public void TryParse_ValidInput_ReturnsLine()
    {
        // Act
        bool ok = RepairLineParser.TryParse(" wkr-1 ;2;250.50", out RepairLine? line);

        // Assert
        Assert.True(ok);
        Assert.NotNull(line);
        Assert.Equal("WKR-1", line.Code);
        Assert.Equal(2, line.Quantity);
        Assert.Equal(250.50m, line.UnitPrice);
    }

    [Fact]
    public void TryParse_UpperBounds_Accepted()
    {
        // Act
        bool ok = RepairLineParser.TryParse("ABCDEFGHIJKLMNOPQRST;1000;100000", out RepairLine? line);

        // Assert
        Assert.True(ok);
        Assert.NotNull(line);
        Assert.Equal(RepairLineParser.MaxQuantity, line.Quantity);
    }
}
