using System.Globalization;
using Flow.Infrastructure.IO.Contract;
using Flow.Infrastructure.Plugins.Contract;
using Flow.Plugins.Transactions.Transformer.Rules;
using Microsoft.Extensions.Configuration;

namespace Flow.Plugins.Transactions.Transformer;

public class Bootstrapper : IPluginsBootstrapper
{
    private const string EnvConfigFilePath = "FLOW_TT_CONFIG_PATH";

    private readonly string? _configFilePath;
    private readonly CultureInfo _culture;

    public Bootstrapper(): this(Environment.GetEnvironmentVariable(EnvConfigFilePath, EnvironmentVariableTarget.User), CultureInfo.CurrentCulture) {}

    public Bootstrapper(string? configFilePath, CultureInfo culture)
    {
        _configFilePath = configFilePath;
        _culture = culture;
    }

    public IEnumerable<IPlugin> GetPlugins() => GetRuleSets()
        .Where(r => r.Value != null)
        .Select(r => new Mapper(new SupportedFormat(r.Key), r.Value!, _culture));

    private IDictionary<string, RuleSet?> GetRuleSets()
    {
        var builder = new ConfigurationBuilder().AddEnvironmentVariables();
        if (_configFilePath is not null)
        {
            builder.AddJsonFile(_configFilePath);
        }

        return builder.Build()
            .GetSection("Formats")
            .GetChildren()
            .ToDictionary(c => c.Key, c => c.Get<RuleSet>());
    }
}
