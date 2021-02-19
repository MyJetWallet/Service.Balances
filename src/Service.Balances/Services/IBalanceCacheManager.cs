using System.Collections.Generic;
using System.Threading.Tasks;
using Service.Balances.Domain.Models;
using Service.Balances.Postgres;

namespace Service.Balances.Services
{
    public interface IBalanceCacheManager
    {
        Task SaveInNoSqlCache(List<BalanceEntity> updates);
        ValueTask<List<WalletBalanceNoSqlEntity>> AddWalletToCache(string walletId);
    }
}