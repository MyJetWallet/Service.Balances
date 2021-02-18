using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Service.Balances.Grpc;
using Service.Balances.Grpc.Models;
using Service.Balances.Settings;

namespace Service.Balances.Services
{
    public class HelloService: IHelloService
    {
        private readonly ILogger<HelloService> _logger;

        public HelloService(ILogger<HelloService> logger)
        {
            _logger = logger;
        }

        public Task<HelloMessage> SayHelloAsync(HelloRequest request)
        {
            _logger.LogInformation("Hello from {name}", request.Name);

            return Task.FromResult(new HelloMessage
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
