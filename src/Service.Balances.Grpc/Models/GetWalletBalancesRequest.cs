using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.Balances.Domain.Models;

namespace Service.Balances.Grpc.Models
{
    [DataContract]
    public class GetWalletBalancesRequest
    {
        [DataMember(Order = 1)] public string WalletId { get; set; }
        [DataMember(Order = 2)] public string Symbol { get; set; }
    }

    [DataContract]
    public class WalletBalanceList
    {
        [DataMember(Order = 1)] public List<WalletBalance> Balances { get; set; }
    }
}