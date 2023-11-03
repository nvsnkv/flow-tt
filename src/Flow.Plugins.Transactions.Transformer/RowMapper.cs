using System.Globalization;
using Flow.Application.Transactions.Contract;
using Flow.Domain.Transactions;

namespace Flow.Plugins.Transactions.Transformer;

internal sealed class RowMapper
{
    private readonly CultureInfo _culture;

    public RowMapper(CultureInfo culture)
    {
        _culture = culture;
    }

    public IncomingTransaction Map(Dictionary<string,string?> row)
    {
        var timestamp = DateTime.MinValue;
        var amount = Decimal.MinValue;
        var currency = string.Empty;
        string? category = null;
        var title = String.Empty;
        var name = string.Empty;
        var bank = string.Empty;

        string? titleOverride = null;
        string? categoryOverride = null;
        string? comment = null;

        Overrides? overrides = null;

        foreach (var map in row)
        {
            var value = map.Value;

            switch (map.Key)
            {
                case nameof(Transaction.Timestamp):
                    _ = DateTime.TryParse(value, _culture, out timestamp);
                    break;

                case nameof(Transaction.Amount):
                    _ = decimal.TryParse(value, _culture, out amount);
                    break;

                case nameof(Transaction.Category):
                    category = value;
                    break;

                case nameof(Transaction.Title):
                    title = value ?? string.Empty;
                    break;

                case nameof(Transaction.Currency):
                    currency = value ?? string.Empty;
                    break;

                case $"{nameof(Transaction.Account)}{nameof(Transaction.Account.Name)}":
                    name = value ?? string.Empty;
                    break;

                case $"{nameof(Transaction.Account)}{nameof(Transaction.Account.Bank)}":
                    bank = value ?? string.Empty;
                    break;

                case $"{nameof(Overrides)}{nameof(Overrides.Title)}":
                    titleOverride = value;
                    break;

                case $"{nameof(Overrides)}{nameof(Overrides.Category)}":
                    categoryOverride = value;
                    break;

                case $"{nameof(Overrides)}{nameof(Overrides.Comment)}":
                    comment = value;
                    break;
            }
        }

        if (!string.IsNullOrEmpty(comment) || !string.IsNullOrEmpty(titleOverride) || !string.IsNullOrEmpty(categoryOverride))
        {
            overrides = new(categoryOverride, titleOverride, comment);
        }

        return new IncomingTransaction(
            new Transaction(timestamp, amount, currency, category, title, new AccountInfo(name, bank)),
            overrides
        );
    }
}
