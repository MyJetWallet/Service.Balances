using System.Runtime.Serialization;
using Service.Balances.Domain.Models;

namespace Service.Balances.Grpc.Models
{
    [DataContract]
    public class HelloMessage : IHelloMessage
    {
        [DataMember(Order = 1)]
        public string Message { get; set; }
    }
}