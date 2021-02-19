using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc.Models;

namespace Service.Balances.Grpc
{
    [ServiceContract]
    public interface IWalletBalanceService
    {
        [OperationContract]
        Task<WalletBalanceList> GetWalletBalancesAsync(GetWalletBalancesRequest request);
    }

    public static class WalletBalanceServiceHelper
    {
        public static async Task<List<WalletBalance>> GetBalancesByWallet(this IWalletBalanceService service, string walletId)
        {
            var data = await service.GetWalletBalancesAsync(new GetWalletBalancesRequest() {WalletId = walletId});
            return data.Balances;
        }

        public static async Task<WalletBalance> GetBalancesByWalletAndSymbol(this IWalletBalanceService service, string walletId, string assetSymbol)
        {
            var data = await service.GetWalletBalancesAsync(new GetWalletBalancesRequest() { WalletId = walletId, Symbol = assetSymbol });
            return data.Balances.FirstOrDefault() ?? new WalletBalance()
            {
                AssetId = assetSymbol,
                SequenceId = 0,
                Balance = 0,
                Reserve = 0,
                LastUpdate = DateTime.UtcNow
            };
        }
    }
}