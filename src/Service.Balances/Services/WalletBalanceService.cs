using System;
using System.Linq;
using System.Threading.Tasks;
using MyJetWallet.Sdk.Service;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;

namespace Service.Balances.Services
{
    public class WalletBalanceService: IWalletBalanceService
    {
        private readonly IBalanceCacheManager _cacheManager;

        public WalletBalanceService(IBalanceCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task<WalletBalanceList> GetWalletBalancesAsync(GetWalletBalancesRequest request)
        {
            request.WalletId.AddToActivityAsTag("walletId");

            var data = await _cacheManager.AddWalletToCache(request.WalletId);

            var result = new WalletBalanceList()
            {
                Balances = data
                    .Where(e => e.IsReal)
                    .Where(e => string.IsNullOrEmpty(request.Symbol) || e.Balance.AssetId == request.Symbol)
                    .Select(e => new WalletBalance(e.Balance))
                    .ToList()
            };

            return result;
        }
    }
}