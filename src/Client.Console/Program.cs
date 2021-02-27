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
            _client = new HttpClient {Timeout = TimeSpan.FromMinutes(1)};

            while (true)
            {
                Console.Write("Enter # of orders to send, or empty to quit: ");
                var line = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(line))
                    break;

                int limit;
                int loops = 1;
                var segments = line.Split(',');
                if (segments.Length == 2)
                {
                    loops = int.TryParse(segments[1], out int result) ? result : 1;
                    limit = int.TryParse(segments[0], out result) ? result : 1;
                }
                else if (!int.TryParse(line, out limit))
                    limit = 1;

                for (var pass = 0; pass < loops; pass++)
                {
                    var tasks = new List<Task>();

                    for (var i = 0; i < limit; i++)
                    {
                        var order = new OrderModel
                        {
                            Id = NewId.NextGuid(),
                            CustomerNumber = $"CUSTOMER{i}",
                            PaymentCardNumber = i % 4 == 0 ? "5999" : "4000-1234",
                            Notes = new string('*', 1000 * (i + 1))
                        };

                        tasks.Add(Execute(order));
                    }

                    await Task.WhenAll(tasks.ToArray());

                    Console.WriteLine();
                    Console.WriteLine("Results {0}/{1}", pass+1, loops);

                    foreach (Task<string> task in tasks.Cast<Task<string>>())
                        Console.WriteLine(task.Result);
                }
            }
        }

        static readonly Random _random = new Random();

        static async Task<string> Execute(OrderModel order)
        {
            try
            {
                var json = JsonConvert.SerializeObject(order);
                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var responseMessage = await _client.PostAsync($"http://localhost:5000/Order", data);

                responseMessage.EnsureSuccessStatusCode();

                var result = await responseMessage.Content.ReadAsStringAsync();

                if (responseMessage.StatusCode == HttpStatusCode.Accepted)
                {
                    await Task.Delay(2000);
                    await Task.Delay(_random.Next(6000));

                    var orderAddress = $"http://localhost:5000/Order?id={order.Id:D}";

                    var patchResponse = await _client.PatchAsync(orderAddress, data);

                    patchResponse.EnsureSuccessStatusCode();

                    var patchResult = await patchResponse.Content.ReadAsStringAsync();

                    do
                    {
                        await Task.Delay(5000);

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
        public string Notes { get; set; }
    }


    public class OrderStatusModel
    {
        public Guid OrderId { get; set; }
        public string State { get; set; }
    }
}