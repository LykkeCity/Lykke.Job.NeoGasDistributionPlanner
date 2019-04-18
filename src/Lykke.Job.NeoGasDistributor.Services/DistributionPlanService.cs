using System;
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
using Lykke.Service.BlockchainApi.Client;

namespace Lykke.Job.NeoGasDistributor.Services
{
    [UsedImplicitly]
    public class DistributionPlanService : IDistributionPlanService
    {
        private readonly IBlockchainApiClient _blockchainApiClient;
        private readonly IClaimedGasAmountRepository _claimedGasAmountRepository;
        private readonly IDistributionPlanRepository _distributionPlanRepository;
        private readonly string _gasAssetId;
        private readonly ILog _log;
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly ISnapshotRepository _snapshotRepository;

        
        public DistributionPlanService(
            IBlockchainApiClient blockchainApiClient,
            IClaimedGasAmountRepository claimedGasAmountRepository,
            IDistributionPlanRepository distributionPlanRepository,
            ILogFactory logFactory,
            IMatchingEngineClient matchingEngineClient,
            ISnapshotRepository snapshotRepository,
            string gasAssetId)
        {
            _blockchainApiClient = blockchainApiClient;
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
            var snapshots = await _snapshotRepository.GetAsync(from, to);
            var claimedGasAmounts = await _claimedGasAmountRepository.GetAsync(from, to);
            var scale = (await _blockchainApiClient.GetAssetAsync(_gasAssetId)).Accuracy;
            var distributionAmounts = DistributionPlanCalculator.CalculateAmounts(snapshots, claimedGasAmounts, scale);

            var distributionPlan = DistributionPlanAggregate.Create(to, distributionAmounts);

            await _distributionPlanRepository.SaveAsync(distributionPlan);
        }

        public async Task ExecutePlanAsync(
            Guid planId)
        {
            var distributionPlan = await _distributionPlanRepository.TryGetAsync(planId);

            if (distributionPlan != null)
            {
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
