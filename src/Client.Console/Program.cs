namespace Client.Console
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using MassTransit;
    using Newtonsoft.Json;


    class Program
    {
        static HttpClient _client;

        static async Task Main(string[] args)
        {
            while (true)
            {
                Console.Write("Enter # of orders to send, or empty to quit: ");
                var line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                    break;

                if (!int.TryParse(line, out int limit))
                    limit = 1;

                var tasks = new List<Task>();

                _client = new HttpClient {Timeout = TimeSpan.FromMinutes(1)};

                for (var i = 0; i < limit; i++)
                {
                    var order = new OrderModel
                    {
                        Id = NewId.NextGuid(),
                        CustomerNumber = $"CUSTOMER{i}",
                        PaymentCardNumber = i % 4 == 0 ? "5999" : "4000-1234"
                    };

                    tasks.Add(Execute(order));
                }

                await Task.WhenAll(tasks.ToArray());

                Console.WriteLine();
                Console.WriteLine("Results");

                foreach (Task<string> task in tasks.Cast<Task<string>>())
                    Console.WriteLine(task.Result);
            }
        }

        static async Task<string> Execute(OrderModel order)
        {
            try
            {
                var json = JsonConvert.SerializeObject(order);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var responseMessage = await _client.PostAsync($"https://localhost:5001/Order", data);

                responseMessage.EnsureSuccessStatusCode();

                var result = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.StatusCode == HttpStatusCode.Accepted)
                {
                    await Task.Delay(1000);

                    var orderAddress = $"https://localhost:5001/Order?id={order.Id:D}";

                    var patchResponse = await _client.PatchAsync(orderAddress, data);

                    patchResponse.EnsureSuccessStatusCode();

                    var patchResult = await patchResponse.Content.ReadAsStringAsync();

                    do
                    {
                        await Task.Delay(1000);

                        var getResponse = await _client.GetAsync(orderAddress);

                        getResponse.EnsureSuccessStatusCode();

                        var getResult = await getResponse.Content.ReadAsAsync<OrderStatusModel>();

                        if (getResult.State == "Completed" || getResult.State == "Faulted")
                            return $"ORDER: {order.Id:D} STATUS: {getResult.State}";

                        Console.Write(".");
                    }
                    while (true);
                }

                return result;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);

                return exception.Message;
            }
        }
    }


    public class OrderModel
    {
        public Guid Id { get; set; }
        public string CustomerNumber { get; set; }
        public string PaymentCardNumber { get; set; }
    }


    public class OrderStatusModel
    {
        public Guid OrderId { get; set; }
        public string State { get; set; }
    }
}