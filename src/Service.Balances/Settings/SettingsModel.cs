using SimpleTrading.SettingsReader;

namespace Service.Balances.Settings
{
    [YamlAttributesOnly]
    public class SettingsModel
    {
        [YamlProperty("Balances.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }
    }
}