using System.Globalization;
using RepairService.Legacy;

namespace RepairService.Legacy.Tests;

public class LegacyCharacterizationTests
{
    public LegacyCharacterizationTests()
    {
        // формат чисел залежить від культури середовища, тому фіксуємо її явно
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
    }

    [Theory]
    [InlineData(null, TicketState.New, "a@b.c", "ERR: null")]
    [InlineData("empty", TicketState.New, "a@b.c", "ERR: empty")]
    [InlineData("two", TicketState.Draft, "a@b.c", "ERR: state")]
    [InlineData("two", TicketState.New, "no-mail", "ERR: mail")]
    public void Handle_BadInput_ReturnsErrorCode(
        string? kindOfLines, TicketState state, string mail, string expected)
    {
        List<WorkLine>? lines = kindOfLines switch
        {
            "two" => TwoLines(),
            "empty" => new List<WorkLine>(),
            _ => null,
        };
        var sut = new LegacyTicketProcessor();

        string actual = sut.Handle(
            1001, 7, "Іван", mail, ClientKind.Regular, 0, lines, state, "UAH", new DateTime(2026, 3, 1), false);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Handle_VipTicket_ReturnsExactReport()
    {
        var sut = new LegacyTicketProcessor();

        string report = sut.Handle(
            1001, 7, "Іван", "a@b.c", ClientKind.Vip, 3, TwoLines(), TicketState.New, "UAH", new DateTime(2026, 3, 1), false);

        string expected =
            "Документ #1001\n" +
            "Клієнт: Іван\n" +
            "WRK-1 x2 = 300.00 UAH\n" +
            "PRT-2 x1 = 400.00 UAH\n" +
            "Знижка: 105.00 UAH\n" +
            "Доставка: 60.00 UAH\n" +
            "Разом: 655.00 UAH\n";
        Assert.Equal(expected, report);
    }

    [Theory]
    [InlineData(ClientKind.Regular, 0, 760)]
    [InlineData(ClientKind.Vip, 0, 655)]
    [InlineData(ClientKind.Staff, 0, 550)]
    [InlineData(ClientKind.Regular, 11, 725)]
    public void Preview_ByKind_ReturnsTotal(ClientKind kind, int done, decimal expected)
    {
        var sut = new LegacyTicketProcessor();

        decimal total = sut.Preview(kind, done, TwoLines());

        Assert.Equal(expected, total);
    }

    [Fact]
    public void Preview_TotalExactly1000_NoShipping()
    {
        var sut = new LegacyTicketProcessor();
        var lines = new List<WorkLine>
        {
            new WorkLine { WorkCode = "WRK-9", Quantity = 1, UnitPrice = 1000m },
        };

        decimal total = sut.Preview(ClientKind.Regular, 0, lines);

        Assert.Equal(1000m, total);
    }

    [Fact]
    public void Preview_StaffLargeSum_DiscountCappedAt500()
    {
        var sut = new LegacyTicketProcessor();
        var lines = new List<WorkLine>
        {
            new WorkLine { WorkCode = "WRK-9", Quantity = 2, UnitPrice = 1000m },
        };

        decimal total = sut.Preview(ClientKind.Staff, 0, lines);

        Assert.Equal(1500m, total);
    }

    [Fact]
    public void DescribeClient_Vip_BuildsCaption()
    {
        var client = new Client
        {
            Name = " іван ",
            Email = "IVAN@MAIL.COM",
            Kind = ClientKind.Vip,
            DoneCount = 12,
            SinceUtc = new DateTime(2020, 1, 1),
        };

        // метод залежить від системного годинника, тому очікування обчислюємо
        int years = DateTime.Now.Year - 2020;
        string expected = "ІВАН [VIP] [ЛОЯЛЬНИЙ] <ivan@mail.com> стаж " + years;

        string actual = new LegacyTicketProcessor().DescribeClient(client);

        Assert.Equal(expected, actual);
    }

    private static List<WorkLine> TwoLines() => new()
    {
        new WorkLine { WorkCode = "WRK-1", Quantity = 2, UnitPrice = 150m },
        new WorkLine { WorkCode = "PRT-2", Quantity = 1, UnitPrice = 400m },
    };
}
