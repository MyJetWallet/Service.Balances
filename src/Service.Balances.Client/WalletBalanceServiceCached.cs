using System.Linq;
using System.Threading.Tasks;
using MyNoSqlServer.Abstractions;
using MyNoSqlServer.DataReader;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;

namespace Service.Balances.Client
{
    public class WalletBalanceServiceCached: IWalletBalanceService
    {
        private readonly IWalletBalanceService _service;
        private readonly IMyNoSqlServerDataReader<WalletBalanceNoSqlEntity> _reader;

        public WalletBalanceServiceCached(IWalletBalanceService service, MyNoSqlReadRepository<WalletBalanceNoSqlEntity> reader)
        {
            _service = service;
            _reader = reader;
        }

        public Task<WalletBalanceList> GetWalletBalancesAsync(GetWalletBalancesRequest request)
        {
            var data = _reader.Get(WalletBalanceNoSqlEntity.GeneratePartitionKey(request.WalletId));
            if (data != null && data.Any())
            {
                var res = data
                    .Where(e => string.IsNullOrEmpty(request.Symbol) || e.Balance.AssetId == request.Symbol)
                    .Select(e => e.Balance)
                    .ToList();

                return Task.FromResult(new WalletBalanceList() {Balances = res});
            }

            return _service.GetWalletBalancesAsync(request);
        }
    }
}