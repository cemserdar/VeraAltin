using RabbitMQ.Client;
using Newtonsoft.Json;
using System.Text;


var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();


string apiKey = "APİ KEY HERE";
string symbol = "XAU";
string curr = "USD";
string date = "";
string message = "";

channel.QueueDeclareAsync(queue: "gold_price_queue",
                     durable: false,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);

System.Console.WriteLine("Altın Fiyat Takip Sistemi Başlatıldı...");



using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(5));



while (await timer.WaitForNextTickAsync())
{

    using (HttpClient client = new HttpClient())
    {
        string url = $"https://www.goldapi.io/api/{symbol}/{curr}/{date}";
        var json = "{\"key\": \"value\"}";
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Add("x-access-token", apiKey);

        try
        {
            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            string result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);

            var body = Encoding.UTF8.GetBytes(result);

            var basicProperties = new BasicProperties();

            await channel.BasicPublishAsync(exchange: "",
                                             routingKey: "gold_price_queue",
                                             mandatory: false,
                                             basicProperties: basicProperties,
                                             body: body);


            Console.WriteLine($"[x] Kuyruğa Gönderildi: {message}");

            Thread.Sleep(5000);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

    }




}
