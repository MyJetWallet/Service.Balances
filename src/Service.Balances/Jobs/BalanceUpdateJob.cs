using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using ME.Contracts.OutgoingMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyNoSqlServer.Abstractions;
using Newtonsoft.Json;
using Service.Balances.Domain.Models;
using Service.Balances.Postgres;

namespace Service.Balances.Jobs
{
    public class BalanceUpdateJob
    {
        private readonly IMyNoSqlServerDataWriter<WalletBalanceNoSqlEntity> _writer;
        private readonly ILogger<BalanceUpdateJob> _logger;
        private readonly DbContextOptionsBuilder<BalancesContext> _dbContextOptionsBuilder;

        public BalanceUpdateJob(ISubscriber<IReadOnlyList<MeEvent>> subscriber, 
            IMyNoSqlServerDataWriter<WalletBalanceNoSqlEntity> writer,
            ILogger<BalanceUpdateJob> logger,
            DbContextOptionsBuilder<BalancesContext> dbContextOptionsBuilder)
        {
            _writer = writer;
            _logger = logger;
            _dbContextOptionsBuilder = dbContextOptionsBuilder;
            subscriber.Subscribe(HandleEvents);
        }

        private async ValueTask HandleEvents(IReadOnlyList<MeEvent> events)
        {
            _logger.LogDebug("Receive {count} events", events.Count);

            try
            {
                var updates = events
                    .SelectMany(e => e.BalanceUpdates.Select(u => new {Update = u, e.Header.SequenceNumber, e.Header.Timestamp}))
                    .GroupBy(e => new {e.Update.WalletId, e.Update.AssetId})
                    .Select(e => e.OrderByDescending(i => i.SequenceNumber).First())
                    .Select(e => BalanceEntity.Create(
                        e.Update.WalletId,
                        e.Update.BrokerId,
                        e.Update.AccountId,
                        new WalletBalance(
                            e.Update.AssetId, 
                            double.Parse(e.Update.NewBalance),
                            double.Parse(e.Update.NewReserved),
                            e.Timestamp.ToDateTime(),
                            e.SequenceNumber
                        )))
                    .ToList();

                await SaveBalanceUpdateToDatabaseAsync(updates);

                await SaveInNoSqlCache(updates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cannot handle batch of MeEvent's");
                throw;
            }
        }

        private async Task SaveBalanceUpdateToDatabaseAsync(List<BalanceEntity> updates)
        {
            await using var ctx = GetDbContext();
            var count = await ctx.InsertOrUpdateAsync(updates);

            _logger.LogDebug("Successfully insert or update: {count}", count);
        }

        private BalancesContext GetDbContext()
        {
            return new BalancesContext(_dbContextOptionsBuilder.Options);
        }

        private async Task SaveInNoSqlCache(List<BalanceEntity> updates)
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

        private async ValueTask<bool> IsWalletExistInCache(string walletId)
        {
            var entityList = await _writer.GetAsync(WalletBalanceNoSqlEntity.GeneratePartitionKey(walletId));

            return entityList != null && entityList.Any();
        }

        private async ValueTask<List<WalletBalanceNoSqlEntity>> AddWalletToCache(string walletId)
        {
            await using var ctx = GetDbContext();

            var balances = ctx.Balances.Where(e => e.WalletId == walletId);

            var entityList = await balances.Select(e => WalletBalanceNoSqlEntity.Create(e.WalletId, e)).ToListAsync();

            await _writer.BulkInsertOrReplaceAsync(entityList);

            return entityList;
        }
    }
}