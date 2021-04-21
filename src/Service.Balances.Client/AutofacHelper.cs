using Autofac;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;

namespace Service.Balances.Client
{
    public static class AutofacHelper
    {
        public static void RegisterBalancesClients(this ContainerBuilder builder, string balancesGrpcServiceUrl, IMyNoSqlSubscriber myNoSqlSubscriber)
        {
            var subs = new MyNoSqlReadRepository<WalletBalanceNoSqlEntity>(myNoSqlSubscriber, WalletBalanceNoSqlEntity.TableName);
            
            var factory = new BalancesClientFactory(balancesGrpcServiceUrl, subs);

            builder.RegisterInstance(factory.GetWalletBalanceCachedService()).As<IWalletBalanceService>().SingleInstance();

            builder.RegisterInstance(subs).As<IMyNoSqlServerDataReader<WalletBalanceNoSqlEntity>>().SingleInstance();
        }

        public static void RegisterBalancesClientsWithoutCache(this ContainerBuilder builder, string balancesGrpcServiceUrl)
        {
            var factory = new BalancesClientFactory(balancesGrpcServiceUrl, null);

            builder.RegisterInstance(factory.GetWalletBalanceService()).As<IWalletBalanceService>().SingleInstance();
        }
    }
}