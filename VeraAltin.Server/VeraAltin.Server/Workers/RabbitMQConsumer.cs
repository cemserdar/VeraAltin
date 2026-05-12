using System.Text;
using VeraAltin.Shared.Models;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using VeraAltin.Server.Hubs;
using VeraAltin.Shared.Models;

namespace VeraAltin.Server.Workers
{
    public class RabbitMQConsumer : BackgroundService
    {
        private readonly IHubContext<VeraAltinHub> _hubContext;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMQConsumer(IHubContext<VeraAltinHub> hubContext)
        {
            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            try
            {
                _connection = await factory.CreateConnectionAsync(stoppingToken);
                _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await _channel.QueueDeclareAsync(queue: "gold_price_queue",
                                                 durable: false,
                                                 exclusive: false,
                                                 autoDelete: false,
                                                 arguments: null,
                                                 cancellationToken: stoppingToken);

                Console.WriteLine("[Consumer] Kuyruk dinleniyor ve SignalR Yayınına hazır...");

                var consumer = new AsyncEventingBasicConsumer(_channel);

                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    try
                    {
                        var goldData = JsonConvert.DeserializeObject<GoldPriceModel>(message);

                        if (goldData != null)
                        {
                            Console.WriteLine($"[Consumer] Kuyruktan alındı: {goldData.Price}");
                            await _hubContext.Clients.All.SendAsync("ReceiveGoldPrice", goldData, stoppingToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[Consumer] Mesaj işleme hatası: {ex.Message}");
                    }
                };

                await _channel.BasicConsumeAsync(queue: "gold_price_queue",
                                                 autoAck: true,
                                                 consumer: consumer,
                                                 cancellationToken: stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Consumer] RabbitMQ Bağlantı Hatası: {ex.Message}. Docker çalışıyor mu?");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }

        public override async void Dispose()
        {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
            base.Dispose();
        }
    }
}