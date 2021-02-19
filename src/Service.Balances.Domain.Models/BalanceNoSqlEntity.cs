using System;
using MyNoSqlServer.Abstractions;

namespace Service.Balances.Domain.Models
{
    public class WalletBalanceNoSqlEntity: MyNoSqlDbEntity
    {
        public const string TableName = "myjetwallet-client-wallet-balance";

        public const string NoneRowKey = "--none--";

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

        public static WalletBalanceNoSqlEntity None(string walletId)
        {
            return new WalletBalanceNoSqlEntity()
            {
                PartitionKey = GeneratePartitionKey(walletId),
                RowKey = GenerateRowKey(NoneRowKey),
                Balance = new WalletBalance(NoneRowKey, 0, 0, DateTime.UtcNow, 0)
            };
        }

        public bool IsReal => RowKey != NoneRowKey;
        public bool IsNone => RowKey == NoneRowKey;
    }
}