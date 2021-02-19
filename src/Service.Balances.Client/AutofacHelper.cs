using Autofac;
using MyNoSqlServer.DataReader;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;

namespace Service.Balances.Client
{
    public static class AutofacHelper
    {
        public static void RegisterClientWalletsClients(this ContainerBuilder builder, string balancesGrpcServiceUrl, IMyNoSqlSubscriber myNoSqlSubscriber)
        {
            var subs = new MyNoSqlReadRepository<WalletBalanceNoSqlEntity>(myNoSqlSubscriber, WalletBalanceNoSqlEntity.TableName);

            var factory = new BalancesClientFactory(balancesGrpcServiceUrl, subs);

            builder.RegisterInstance(factory.GetWalletBalanceService()).As<IWalletBalanceService>().SingleInstance();
        }
    }
}