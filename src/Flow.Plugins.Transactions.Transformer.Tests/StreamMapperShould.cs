using System.Globalization;
using Flow.Application.Transactions.Contract;
using Flow.Infrastructure.IO.Contract;
using FluentAssertions;

namespace Flow.Plugins.Transactions.Transformer.Tests;

public class StreamMapperShould
{
    [Fact]
    public async Task ProcessValidFileWithTransformsAndSkipIfRules()
    {
        var bootstrapper = new Bootstrapper("TestData/format.json", CultureInfo.GetCultureInfo("ru-RU"));
        var mapper = bootstrapper.GetPlugins().SingleOrDefault() as IFormatSpecificReader<IncomingTransaction>;
        mapper.Should().NotBeNull("only one mapper expected from config");

        mapper!.Format.Name.Should().Be("custom-csv", "as defined in format.json");

        using var streamReader = new StreamReader(File.OpenRead("TestData/valid.csv"));

        var transactions = await mapper.Read(streamReader, CancellationToken.None);
        transactions.Should().BeEquivalentTo(ValidCSV);
    }

    private static readonly IncomingTransaction[] ValidCSV =
    {
        new(
            new(new DateTime(2023, 10, 1, 12, 03, 00), -5.90m, "$", String.Empty, "Some withdraw",
                new("Test Account", "Test Bank")),
            new(null, null, "MCC 9000")
        ),
        new(
            new(new DateTime(2023, 10, 1, 12, 04, 00), -4.90m, "$", String.Empty, "Some withdraw",
                new("Test Account", "Test Bank")),
            new(null, null, "MCC 9000")
        ),
        new(
            new(new DateTime(2023, 10, 1, 12, 05, 00), -3.90m, "$", String.Empty, "Some withdraw",
                new("Test Account", "Test Bank")),
            new(null, null, "MCC 9000")
        ),
    };
}
