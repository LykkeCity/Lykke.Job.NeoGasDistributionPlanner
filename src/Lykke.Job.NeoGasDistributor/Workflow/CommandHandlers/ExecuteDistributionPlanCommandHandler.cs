using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using NeoGasDistributor.Contract.Commands;

namespace Lykke.Job.NeoGasDistributor.Workflow.CommandHandlers
{
    public class ExecuteDistributionPlanCommandHandler
    {
        private readonly IDistributionPlanService _distributionPlanService;

        public ExecuteDistributionPlanCommandHandler(
            IDistributionPlanService distributionPlanService)
        {
            _distributionPlanService = distributionPlanService;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(
            ExecuteDistributionPlanCommand command,
            IEventPublisher publisher)
        {
            await _distributionPlanService.ExecutePlanAsync(command.PlanId);
            
            return CommandHandlingResult.Ok();
        }
    }
}
