using System;
using System.Runtime.Serialization;

namespace Service.Balances.Domain.Models
{
    [DataContract]
    public class WalletBalance
    {
        public WalletBalance()
        {
        }

        public WalletBalance(string assetId, double balance, double reserve, DateTime lastUpdate, long sequenceId)
        {
            AssetId = assetId;
            Balance = balance;
            Reserve = reserve;
            LastUpdate = lastUpdate;
            SequenceId = sequenceId;
        }

        [DataMember(Order = 1)]
        public string AssetId { get; set; }

        [DataMember(Order = 2)]
        public double Balance { get; set; }

        [DataMember(Order = 3)]
        public double Reserve { get; set; }

        [DataMember(Order = 4)]
        public DateTime LastUpdate { get; set; }

        [DataMember(Order = 5)]
        public long SequenceId { get; set; }
    }
}