using MyJetWallet.Sdk.Postgres;

namespace Service.Balances.Postgres.DesignTime
{
    public class ContextFactory : MyDesignTimeContextFactory<BalancesContext>
    {
        public ContextFactory() : base(options => new BalancesContext(options))
        {
        }
    }
}