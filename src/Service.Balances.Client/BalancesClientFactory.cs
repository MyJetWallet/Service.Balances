using System;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using JetBrains.Annotations;
using MyJetWallet.Sdk.GrpcMetrics;
using MyNoSqlServer.DataReader;
using ProtoBuf.Grpc.Client;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;

namespace Service.Balances.Client
{
    [UsedImplicitly]
    public class BalancesClientFactory
    {
        private readonly MyNoSqlReadRepository<WalletBalanceNoSqlEntity> _reader;
        private readonly CallInvoker _channel;

        public BalancesClientFactory(string balancesGrpcServiceUrl, MyNoSqlReadRepository<WalletBalanceNoSqlEntity> reader)
        {
            _reader = reader;
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress(balancesGrpcServiceUrl);
            _channel = channel.Intercept(new PrometheusMetricsInterceptor());
        }

        public IWalletBalanceService GetWalletBalanceService()
        {
            return new WalletBalanceServiceCached(
                _channel.CreateGrpcService<IWalletBalanceService>(),
                _reader);
        }
    }
}
