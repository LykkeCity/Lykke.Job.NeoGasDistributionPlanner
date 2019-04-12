using System.Collections.Generic;
using Autofac;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Cqrs.MessageCancellation.Configuration;
using Lykke.Cqrs.MessageCancellation.Interceptors;
using Lykke.Job.NeoClaimTransactionsExecutor.Contract;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using Lykke.Job.NeoGasDistributor.Settings;
using Lykke.Job.NeoGasDistributor.Workflow.CommandHandlers;
using Lykke.Job.NeoGasDistributor.Workflow.Projections;
using Lykke.Messaging;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using Lykke.Service.Balances.Client.Events;
using Lykke.SettingsReader;
using Microsoft.Extensions.Options;
using NeoGasDistributor.Contract;
using NeoGasDistributor.Contract.Commands;
using RabbitMQ.Client;

namespace Lykke.Job.NeoGasDistributor.Modules
{
    [UsedImplicitly]
    public class WorkflowModule : Module
    {
        private readonly CqrsSettings _cqrsSettings;
        private readonly string _neoAssetId;


        public WorkflowModule(
            IReloadingManager<AppSettings> appSettings)
        {
            _cqrsSettings = appSettings.CurrentValue.Cqrs;
            _neoAssetId = appSettings.CurrentValue.NeoGasDistributor.NeoAssetId;
        }

        
        protected override void Load(
            ContainerBuilder builder)
        {
            builder
                .Register(ctx => new AutofacDependencyResolver(ctx))
                .As<IDependencyResolver>()
                .SingleInstance();

            builder
                .Register(ctx => new ExecuteDistributionPlanCommandHandler
                (
                    ctx.Resolve<IDistributionPlanService>()
                ))
                .SingleInstance();

            builder
                .Register(ctx => new BalanceUpdateRegistrationProjection
                (
                    ctx.Resolve<IBalanceService>(),
                    _neoAssetId
                ))
                .SingleInstance();

            builder
                .Register(ctx => new GasClaimRegistrationProjection
                (
                    ctx.Resolve<IGasClaimService>()
                ))
                .SingleInstance();
            
            builder
                .RegisterCqrsMessageCancellation(ConfigureCqrsMessageCancellation);
            
            builder.Register(CreateEngine)
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }

        private CqrsEngine CreateEngine(IComponentContext ctx)
        {
            var rabbitMqSettings = new ConnectionFactory
            {
                Uri = _cqrsSettings.RabbitConnectionString
            };
            
            var messagingEngine = new MessagingEngine
            (
                ctx.Resolve<ILogFactory>(),
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {
                        "RabbitMq",
                        new TransportInfo
                        (
                            broker: rabbitMqSettings.Endpoint.ToString(),
                            login: rabbitMqSettings.UserName,
                            password: rabbitMqSettings.Password, 
                            jailStrategyName: "None", 
                            messaging: "RabbitMq"
                        )
                    }
                }),
                new RabbitMqTransportFactory(ctx.Resolve<ILogFactory>())
            );
            
            var engine = new CqrsEngine
            (
                logFactory: ctx.Resolve<ILogFactory>(),
                dependencyResolver: ctx.Resolve<IDependencyResolver>(),
                messagingEngine: messagingEngine,
                endpointProvider: new DefaultEndpointProvider(),
                createMissingEndpoints: true,
                registrations: ConfigureCqrsEngine()
            );
            
            engine.StartPublishers();

            return engine;
        }

        private IRegistration[] ConfigureCqrsEngine()
        {
            var balancesBoundedContext = Service.Balances.Client.BoundedContext.Name;
            
            const string defaultPipeline = "commands";
            const string defaultRoute = "self";
            
            var retryDelay = (long) _cqrsSettings.RetryDelay.TotalMilliseconds;

            return new[]
            {
                Register.CommandInterceptor<MessageCancellationCommandInterceptor>(),
                Register.EventInterceptor<MessageCancellationEventInterceptor>(),
                Register.DefaultEndpointResolver
                (
                    new RabbitMqConventionEndpointResolver
                    (
                        "RabbitMq",
                        SerializationFormat.ProtoBuf,
                        environment: "lykke"
                    )
                ),
                Register
                    .BoundedContext(NeoGasDistributionPlannerBoundedContext.Name)
                    .FailedCommandRetryDelay(retryDelay)

                    .ListeningCommands(typeof(ExecuteDistributionPlanCommand))
                    .On(defaultPipeline)
                    .WithLoopback()
                    .WithCommandsHandler<ExecuteDistributionPlanCommandHandler>()
                    
                    .ListeningEvents(typeof(BalanceUpdatedEvent))
                    .From(balancesBoundedContext)
                    .On(defaultRoute)
                    .WithProjection(typeof(BalanceUpdateRegistrationProjection), balancesBoundedContext)
                
                    .ListeningEvents(typeof(GasClaimTransactionExecutedEvent))
                    .From(NeoClaimTransactionsExecutorBoundedContext.Name)
                    .On(defaultRoute)
                    .WithProjection(typeof(GasClaimRegistrationProjection), NeoClaimTransactionsExecutorBoundedContext.Name)
                    
                    .ProcessingOptions(defaultRoute)
                    .MultiThreaded(8)
                    .QueueCapacity(1024)
            };
        }
        
        private static void ConfigureCqrsMessageCancellation(
            IOptions<RegisterCommandOption> options)
        {
            options
                .Value
                .MapMessageId<ExecuteDistributionPlanCommand>(x => $"{x.PlanId}");

            options
                .Value
                .MapMessageId<BalanceUpdatedEvent>(x => $"{x.WalletId}-{x.SequenceNumber}");

            options
                .Value
                .MapMessageId<GasClaimTransactionExecutedEvent>(x => $"{x.TransactionId}");
        }
    }
}
