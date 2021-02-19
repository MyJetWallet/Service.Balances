using Service.Balances.Domain.Models;

namespace Service.Balances.Postgres
{
    public class BalanceEntity : WalletBalance
    {
        public string WalletId { get; set; }

        public string BrokerId { get; set; }

        public string ClientId { get; set; }

        public static BalanceEntity Create(string walletId, string brokerId, string clientId, WalletBalance balance)
        {
            return new BalanceEntity()
            {
                BrokerId = brokerId,
                ClientId = clientId,
                WalletId = walletId,
                AssetId = balance.AssetId
            }.Apply(balance);
        }

        public BalanceEntity Apply(WalletBalance balance)
        {
            Balance = balance.Balance;
            Reserve = balance.Reserve;
            LastUpdate = balance.LastUpdate;
            SequenceId = balance.SequenceId;

            return this;
        }
    }
}