using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using NeoGasDistributor.Contract.Commands;

namespace Lykke.Job.NeoGasDistributor.Workflow.CommandHandlers
{
    public class ExecuteDistributionPlanCommandHandler
    {
        private readonly IDistributionPlanService _distributionPlanService;
        private readonly ILog _log;

        public ExecuteDistributionPlanCommandHandler(
            IDistributionPlanService distributionPlanService,
            ILogFactory logFactory)
        {
            _distributionPlanService = distributionPlanService;
            _log = logFactory.CreateLog(this);
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(
            ExecuteDistributionPlanCommand command,
            IEventPublisher publisher)
        {
            if (await _distributionPlanService.PlanExistsAsync(command.PlanId))
            {
                await _distributionPlanService.ExecutePlanAsync(command.PlanId);
            }
            else
            {
                _log.Warning
                (
                    $"Distribution plan [{command.PlanId}] has not been executed: plan does not exist.",
                    context: command
                );
            }
            
            return CommandHandlingResult.Ok();
        }
    }
}
