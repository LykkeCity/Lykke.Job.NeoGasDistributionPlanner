using System;
using System.Threading.Tasks;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using Lykke.Job.NeoGasDistributor.Workflow.Projections;
using Lykke.Service.Balances.Client.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lykke.Job.NeoGasDistributor.Tests.Workflow.Projections
{
    [TestClass]
    public class BalanceUpdateRegistrationProjectionTests
    {
        [TestMethod]
        public async Task Test_that_events_with_AssetId_different_from_neo_asset_id_are_ignored()
        {
            // Arrange
            var balanceServiceMock = new Mock<IBalanceService>();
            var neoAssetId = $"{Guid.NewGuid()}";
            
            // Act
            var projection = new BalanceUpdateRegistrationProjection(balanceServiceMock.Object, neoAssetId);

            await projection.Handle(CreateBalanceUpdatedEvent($"{Guid.NewGuid()}"));
            
            // Assert

            balanceServiceMock
                .Verify
                (
                    x => x.RegisterBalanceUpdateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<decimal>()),
                    Times.Never
                );
        }
        
        [TestMethod]
        public async Task Test_that_events_with_AssetId_equal_to_neo_asset_id_are_not_ignored()
        {
            // Arrange
            var balanceServiceMock = new Mock<IBalanceService>();
            var neoAssetId = $"{Guid.NewGuid()}";
            
            // Act
            var projection = new BalanceUpdateRegistrationProjection(balanceServiceMock.Object, neoAssetId);

            await projection.Handle(CreateBalanceUpdatedEvent(neoAssetId));
            
            // Assert

            balanceServiceMock
                .Verify
                (
                    x => x.RegisterBalanceUpdateAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<decimal>()),
                    Times.Once
                );
        }

        private static BalanceUpdatedEvent CreateBalanceUpdatedEvent(
            string assetId)
        {
            return new BalanceUpdatedEvent
            {
                AssetId = assetId,
                Balance = "42.0",
                WalletId = $"{Guid.NewGuid()}"
            };
        }
    }
}
