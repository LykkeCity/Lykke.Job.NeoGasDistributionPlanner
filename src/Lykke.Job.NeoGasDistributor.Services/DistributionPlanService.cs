using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Job.NeoGasDistributor.Domain;
using Lykke.Job.NeoGasDistributor.Domain.Repositories;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using Lykke.Job.NeoGasDistributor.Services.Utils;
using Lykke.MatchingEngine.Connector.Abstractions.Services;
using Lykke.MatchingEngine.Connector.Models.Api;
using Lykke.Service.Assets.Client;

namespace Lykke.Job.NeoGasDistributor.Services
{
    [UsedImplicitly]
    public class DistributionPlanService : IDistributionPlanService
    {
        private readonly IAssetsService _assetService;
        private readonly IClaimedGasAmountRepository _claimedGasAmountRepository;
        private readonly IDistributionPlanRepository _distributionPlanRepository;
        private readonly string _gasAssetId;
        private readonly ILog _log;
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly ISnapshotRepository _snapshotRepository;

        
        public DistributionPlanService(
            IAssetsService assetService,
            IClaimedGasAmountRepository claimedGasAmountRepository,
            IDistributionPlanRepository distributionPlanRepository,
            ILogFactory logFactory,
            IMatchingEngineClient matchingEngineClient,
            ISnapshotRepository snapshotRepository,
            string gasAssetId)
        {
            _assetService = assetService;
            _claimedGasAmountRepository = claimedGasAmountRepository;
            _distributionPlanRepository = distributionPlanRepository;
            _gasAssetId = gasAssetId;
            _log = logFactory.CreateLog(this);
            _matchingEngineClient = matchingEngineClient;
            _snapshotRepository = snapshotRepository;
        }


        public async Task CreatePlanAsync(
            DateTime from,
            DateTime to)
        {
            _log.Info($"Creating distribution plan for {from:s} - {to:s}...");

            var claimedGasAmounts = await _claimedGasAmountRepository.GetAsync(from, to);

            _log.Info("Claimed GAS amount found", new
            {
                claimedGasAmounts
            });

            var snapshots = await _snapshotRepository.GetAsync(from, to);

            _log.Info($"{snapshots.Count} balance snapshots found");

            var scale = _assetService.AssetGet(_gasAssetId).Accuracy;

            _log.Info($"GAS scale {scale}");

            var distributionAmounts = DistributionPlanCalculator.CalculateAmounts(snapshots, claimedGasAmounts, scale).ToArray();

            _log.Info($"{distributionAmounts.Length} distribution amounts gotten");

            var distributionPlan = DistributionPlanAggregate.Create(to, distributionAmounts);

            _log.Info($"Distribution plan {distributionPlan.Id} created");

            await _distributionPlanRepository.SaveAsync(distributionPlan);
        }

        public async Task ExecutePlanAsync(
            Guid planId)
        {
            _log.Info("Distribution plan execution started...", new
            {
                planId
            });

            var distributionPlan = await _distributionPlanRepository.TryGetAsync(planId);

            if (distributionPlan != null)
            {
                _log.Info($"Plan contains {distributionPlan.Amounts} amounts to distribute");

                foreach (var distributionAmount in distributionPlan.Amounts)
                {
                    var result = await _matchingEngineClient.CashInOutAsync
                    (
                        id: distributionAmount.Id.ToString(),
                        clientId: distributionAmount.WalletId.ToString(),
                        assetId: _gasAssetId,
                        amount: ((double) distributionAmount.Value)
                    );
                    
                    // ReSharper disable once SwitchStatementMissingSomeCases
                    switch (result.Status)
                    {
                        case MeStatusCodes.Ok:
                            _log.Info
                            (
                                $"{distributionAmount.Value} of gas has been distributed to {distributionAmount.WalletId}.",
                                distributionAmount
                            );
                            break;
                        
                        case MeStatusCodes.Duplicate:
                            _log.Info
                            (
                                $"Distribution of {distributionAmount.Value} gas to {distributionAmount.WalletId} has been deduplicated by ME.",
                                distributionAmount
                            );
                            break;
                        
                        case MeStatusCodes.Runtime:
                            throw new Exception($"Distribution failed. ME status: {result.Status}. ME message: {result.Message}.");

                        default:
                            _log.Warning
                            (
                                $"Got unexpected response from ME. ME status: {result.Status}, ME message: {result.Message}.",
                                context: distributionAmount
                            );
                            break;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException($"Distribution plan [{planId}] has not been found.");
            }
        }

        public Task<bool> PlanExistsAsync(
            Guid planId)
        {
            return _distributionPlanRepository.PlanExistsAsync(planId);
        }

        public Task<DateTime?> TryGetLatestPlanTimestampAsync()
        {
            return _distributionPlanRepository.TryGetLatestTimestampAsync();
        }
    }
}
