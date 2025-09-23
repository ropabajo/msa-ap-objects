/*
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using Ropabajo.Churc.Sanluis.Framework.RabbitMq;

namespace Ropabajo.Church.Sanluis.Objects.Infraestructure.Consumers
{
    public class ObjectsConsumer : Consumer, IHostedService
    {
        private readonly ILogger<ObjectsConsumer> _logger;

        protected override string QueueName => "nc.objects.queue";

        public ObjectsConsumer(
            ILogger<ObjectsConsumer> logger,
            ConnectionFactory connectionFactory,
            ILogger<RabbitMqClient> clientLogger,
            ILogger<Consumer> consumerLogger,
            IServiceScopeFactory serviceScopeFactory
            ) :
            base(connectionFactory, clientLogger, consumerLogger, serviceScopeFactory)
        {
            _logger = logger;
        }

        public override IRequest<Unit> GetCommand(dynamic message)
        {
            var eventName = MessageParser.GetEventName(message);
            var body = MessageParser.GetBody(message);

            switch (eventName)
            {
                case "AfpCreatedEvent":
                    return JsonConvert.DeserializeObject<object>(body);

                default:
                    return null;
            }
        }

        public virtual Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose();
            return Task.CompletedTask;
        }
    }
}
*/