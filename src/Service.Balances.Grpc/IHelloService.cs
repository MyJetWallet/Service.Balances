using System.ServiceModel;
using System.Threading.Tasks;
using Service.Balances.Grpc.Models;

namespace Service.Balances.Grpc
{
    [ServiceContract]
    public interface IHelloService
    {
        [OperationContract]
        Task<HelloMessage> SayHelloAsync(HelloRequest request);
    }
}