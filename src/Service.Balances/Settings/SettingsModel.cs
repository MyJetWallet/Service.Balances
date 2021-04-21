using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.Balances.Settings
{
    public class SettingsModel
    {
        [YamlProperty("Balances.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("Balances.PostgresConnectionString")]
        public string PostgresConnectionString { get; set; }

        [YamlProperty("Balances.MyNoSqlWriterUrl")]
        public string MyNoSqlWriterUrl { get; set; }

        [YamlProperty("Balances.SpotServiceBusHostPort")]
        public string SpotServiceBusHostPort { get; set; }

        [YamlProperty("Balances.MaxClientInCache")]
        public int MaxClientInCache { get; set; }

        [YamlProperty("Balances.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("Balances.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
    }
}