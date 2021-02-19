using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyNoSqlServer.Abstractions;
using Service.Balances.Domain.Models;
using Service.Balances.Postgres;

namespace Service.Balances.Services
{
    public class BalanceCacheManager : IBalanceCacheManager
    {
        private readonly IMyNoSqlServerDataWriter<WalletBalanceNoSqlEntity> _writer;
        private readonly DbContextOptionsBuilder<BalancesContext> _dbContextOptionsBuilder;

        public BalanceCacheManager(IMyNoSqlServerDataWriter<WalletBalanceNoSqlEntity> writer, DbContextOptionsBuilder<BalancesContext> dbContextOptionsBuilder)
        {
            _writer = writer;
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
        }

        public async Task SaveInNoSqlCache(List<BalanceEntity> updates)
        {
            var wallets = updates.Select(e => e.WalletId).Distinct();

            foreach (var wallet in updates.GroupBy(e => e.WalletId))
            {
                if (!await IsWalletExistInCache(wallet.Key))
                {
                    await AddWalletToCache(wallet.Key);
                }
                else
                {
                    var data = wallet.Select(e => WalletBalanceNoSqlEntity.Create(e.WalletId, e)).ToList();
                    await _writer.BulkInsertOrReplaceAsync(data);
                }
            }
        }

        public async ValueTask<bool> IsWalletExistInCache(string walletId)
        {
            var entityList = await _writer.GetAsync(WalletBalanceNoSqlEntity.GeneratePartitionKey(walletId));

            return entityList != null && entityList.Any();
        }

        public async ValueTask<List<WalletBalanceNoSqlEntity>> AddWalletToCache(string walletId)
        {
            await using var ctx = GetDbContext();

            var balances = ctx.Balances.Where(e => e.WalletId == walletId);

            var entityList = await balances.Select(e => WalletBalanceNoSqlEntity.Create(e.WalletId, e)).ToListAsync();

            await _writer.BulkInsertOrReplaceAsync(entityList);

            return entityList;
        }

        private BalancesContext GetDbContext()
        {
            return new BalancesContext(_dbContextOptionsBuilder.Options);
        }
    }
}