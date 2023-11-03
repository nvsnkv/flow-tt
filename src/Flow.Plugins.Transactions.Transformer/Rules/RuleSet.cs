namespace Flow.Plugins.Transactions.Transformer.Rules;

internal sealed class RuleSet
{
    public IDictionary<string, int>? Mapping { get; set; }

    public IDictionary<string, TransformRule>? Transform { get; set; }

    public IDictionary<string, MatchRule>? SkipIf { get; set; }
}
