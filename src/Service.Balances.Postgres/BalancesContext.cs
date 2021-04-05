using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyJetWallet.Sdk.Service;

namespace Service.Balances.Postgres
{
    public class BalancesContext: DbContext
    {
        public const string Schema = "balances";

        public DbSet<BalanceEntity> Balances { get; set; }

        private readonly Activity _activity;

        public BalancesContext(DbContextOptions options) : base(options)
        {
            InitSqlStatement();
            _activity = MyTelemetry.StartActivity($"Database context {Schema}")?.AddTag("db-schema", Schema);
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schema);

            modelBuilder.Entity<BalanceEntity>().ToTable("balances");
            modelBuilder.Entity<BalanceEntity>().HasKey(e => new { e.WalletId, e.AssetId}).HasName("PK_balances_balances");
            modelBuilder.Entity<BalanceEntity>().HasIndex(e => new {e.BrokerId, e.ClientId}).HasDatabaseName("IX_balances_balances_broker_client");
            modelBuilder.Entity<BalanceEntity>().HasIndex(e => e.WalletId).HasDatabaseName("IX_balances_balances_wallet");
            modelBuilder.Entity<BalanceEntity>().Property(e => e.Balance).HasPrecision(20);
            modelBuilder.Entity<BalanceEntity>().Property(e => e.Reserve).HasPrecision(20);

            base.OnModelCreating(modelBuilder);
        }

        public override void Dispose()
        {
            _activity?.Dispose();
            base.Dispose();
        }

        public async Task<int> InsertOrUpdateAsync(IEnumerable<BalanceEntity> entities)
        {
            var list = entities.ToList();
            var index = 0;
            var countInsert = 0;

            while (index < list.Count)
            {
                var paramString = "";
                foreach (var entity in list.Skip(index).Take(500))
                {
                    if (!string.IsNullOrEmpty(paramString))
                        paramString += ",";

                    paramString += string.Format(_sqlInsertValues, 
                        entity.WalletId, 
                        entity.AssetId,
                        entity.BrokerId,
                        entity.ClientId,
                        entity.Balance.ToString(CultureInfo.InvariantCulture),
                        entity.Reserve.ToString(CultureInfo.InvariantCulture),
                        entity.LastUpdate.ToString("O"),
                        entity.SequenceId.ToString(CultureInfo.InvariantCulture));

                    index++;
                }

                var sql = $"{_sqlInsert} {paramString} {_sqlInsertWhere}";

                try
                {
                    var result = await Database.ExecuteSqlRawAsync(sql);

                    countInsert += result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"InsertOrUpdateAsync exception:\n{ex}\n{sql}");
                    throw;
                }
            }

            return countInsert;
        }

        private string _sqlInsert;
        private string _sqlInsertValues;
        private string _sqlInsertWhere;
        
        private void InitSqlStatement()
        {
            // ReSharper disable once EntityNameCapturedOnly.Local
            BalanceEntity entity;

            _sqlInsert = $" insert into {Schema}.balances " +
                         $" (\"{nameof(entity.WalletId)}\", \"{nameof(entity.AssetId)}\", \"{nameof(entity.BrokerId)}\", \"{nameof(entity.ClientId)}\", \"{nameof(entity.Balance)}\", \"{nameof(entity.Reserve)}\", \"{nameof(entity.LastUpdate)}\", \"{nameof(entity.SequenceId)}\") " +
                         " values ";

            _sqlInsertWhere = $" ON CONFLICT( \"{nameof(entity.WalletId)}\", \"{nameof(entity.AssetId)}\" )" +
                              " DO UPDATE SET" +
                              $" \"{nameof(entity.Balance)}\" = EXCLUDED.\"{nameof(entity.Balance)}\"," +
                              $" \"{nameof(entity.Reserve)}\" = EXCLUDED.\"{nameof(entity.Reserve)}\"," +
                              $" \"{nameof(entity.LastUpdate)}\" = EXCLUDED.\"{nameof(entity.LastUpdate)}\"," +
                              $" \"{nameof(entity.SequenceId)}\" = EXCLUDED.\"{nameof(entity.SequenceId)}\"" +
                              $" WHERE EXCLUDED.\"{nameof(entity.SequenceId)}\" > {Schema}.balances.\"{nameof(entity.SequenceId)}\"";

            //"(WalletId, AssetId, BrokerId, ClientId, Balance, Reserve, LastUpdate, SequenceId)"
            _sqlInsertValues = " ('{0}', '{1}', '{2}', '{3}', {4}, {5}, '{6}', {7})";
        }

    }


}
