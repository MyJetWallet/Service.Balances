using MyNoSqlServer.Abstractions;

namespace Service.Balances.Domain.Models
{
    public class WalletBalanceNoSqlEntity: MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-client-wallet-balance";

        public static string GeneratePartitionKey(string walletId) => walletId;
        public static string GenerateRowKey(string assetId) => assetId;

        public WalletBalance Balance { get; set; }

        public static WalletBalanceNoSqlEntity Create(string walletId, WalletBalance balance)
        {
            return new WalletBalanceNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(walletId),
                RowKey = GenerateRowKey(balance.AssetId),
                Balance = balance
            };
        }
    }
}