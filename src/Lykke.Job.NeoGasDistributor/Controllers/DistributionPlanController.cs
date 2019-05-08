using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using Microsoft.AspNetCore.Mvc;
using NeoGasDistributor.Contract;
using NeoGasDistributor.Contract.Commands;

namespace Lykke.Job.NeoGasDistributor.Controllers
{
    [PublicAPI, Route("/api/distribution-plan")]
    public class DistributionPlanController : Controller
    {
        private readonly ICqrsEngine _cqrsEngine;
        private readonly IDistributionPlanService _distributionPlanService;

        
        public DistributionPlanController(
            ICqrsEngine cqrsEngine,
            IDistributionPlanService distributionPlanService)
        {
            _cqrsEngine = cqrsEngine;
            _distributionPlanService = distributionPlanService;
        }

        
        [HttpPost("{planId}/executions")]
        public async Task<IActionResult> ExecutePlanAsync(
            Guid planId)
        {
            if (await _distributionPlanService.PlanExistsAsync(planId))
            {
                _cqrsEngine.SendCommand
                (
                    command: new ExecuteDistributionPlanCommand { PlanId = planId },
                    boundedContext: NeoGasDistributionPlannerBoundedContext.Name,
                    remoteBoundedContext: NeoGasDistributionPlannerBoundedContext.Name
                );

                return Accepted();
            }
            else
            {
                return NotFound($"Distribution plan [{planId}] has not been found.");
            }
        }
    }
}
