using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotNetCoreDecorators;
using ME.Contracts.OutgoingMessages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Service.Balances.Domain.Models;
using Service.Balances.Postgres;
using Service.Balances.Services;

namespace Service.Balances.Jobs
{
    public class BalanceUpdateJob
    {
        private readonly IBalanceCacheManager _cacheManager;
        private readonly ILogger<BalanceUpdateJob> _logger;
        private readonly DbContextOptionsBuilder<BalancesContext> _dbContextOptionsBuilder;

        public BalanceUpdateJob(ISubscriber<IReadOnlyList<MeEvent>> subscriber,
            IBalanceCacheManager cacheManager,
            ILogger<BalanceUpdateJob> logger,
            DbContextOptionsBuilder<BalancesContext> dbContextOptionsBuilder)
        {
            _cacheManager = cacheManager;
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

                await _cacheManager.SaveInNoSqlCache(updates);
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
    }
}