using Autofac;
using MyJetWallet.Sdk.Service;
using MyNoSqlServer.Abstractions;
using MyServiceBus.Abstractions;
using MyServiceBus.TcpClient;
using Service.Balances.Domain.Models;
using Service.Balances.Jobs;
using Service.Balances.Services;
using Service.MatchingEngine.EventBridge.ServiceBus;

namespace Service.Balances.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            RegisterMyNoSqlWriter<WalletBalanceNoSqlEntity>(builder, WalletBalanceNoSqlEntity.TableName);

            var serviceBusClient = new MyServiceBusTcpClient(Program.ReloadedSettings(e => e.SpotServiceBusHostPort), ApplicationEnvironment.HostName);
            builder.RegisterInstance(serviceBusClient).AsSelf().SingleInstance();

            builder.RegisterMeEventSubscriber(serviceBusClient, "wallet-balance", TopicQueueType.Permanent);


            builder.RegisterType<BalanceUpdateJob>().AutoActivate().SingleInstance();

            builder
                .RegisterType<NoSqlCleanupJob>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();

            builder
                .RegisterType<BalanceCacheManager>()
                .As<IBalanceCacheManager>()
                .SingleInstance();

        }

        private void RegisterMyNoSqlWriter<TEntity>(ContainerBuilder builder, string table)
            where TEntity : IMyNoSqlDbEntity, new()
        {
            builder.Register(ctx => new MyNoSqlServer.DataWriter.MyNoSqlServerDataWriter<TEntity>(
                    Program.ReloadedSettings(e => e.MyNoSqlWriterUrl), table, true))
                .As<IMyNoSqlServerDataWriter<TEntity>>()
                .SingleInstance();
        }


    }
}