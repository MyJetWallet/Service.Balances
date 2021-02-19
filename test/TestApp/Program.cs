using System;
using System.Threading.Tasks;
using MyNoSqlServer.DataReader;
using Newtonsoft.Json;
using ProtoBuf.Grpc.Client;
using Service.Balances.Client;
using Service.Balances.Domain.Models;
using Service.Balances.Grpc.Models;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            GrpcClientFactory.AllowUnencryptedHttp2 = true;

            Console.Write("Press enter to start");
            Console.ReadLine();


            var myNoSqlClient = new MyNoSqlTcpClient(() => "192.168.10.80:5125", "Test-app");

            var subs = new MyNoSqlReadRepository<WalletBalanceNoSqlEntity>(myNoSqlClient, WalletBalanceNoSqlEntity.TableName);


            myNoSqlClient.Start();
            await Task.Delay(2000);
            
            var factory = new BalancesClientFactory("http://localhost:80", subs);
            var client = factory.GetWalletBalanceService();

            var resp = await client.GetWalletBalancesAsync(new GetWalletBalancesRequest()
            {
                WalletId = "manual-test-w-003"
            });
            Console.WriteLine(JsonConvert.SerializeObject(resp, Formatting.Indented));

            await Task.Delay(3000);
            Console.WriteLine();

            resp = await client.GetWalletBalancesAsync(new GetWalletBalancesRequest()
            {
                WalletId = "manual-test-w-003"
            });
            Console.WriteLine(JsonConvert.SerializeObject(resp, Formatting.Indented));

            await Task.Delay(3000);
            Console.WriteLine();

            resp = await client.GetWalletBalancesAsync(new GetWalletBalancesRequest()
            {
                WalletId = "alex--default"
            });
            Console.WriteLine(JsonConvert.SerializeObject(resp, Formatting.Indented));

            Console.WriteLine("End");
            Console.ReadLine();
        }
    }
}
