using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cronos;
using FluentAssertions;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using Lykke.Job.NeoGasDistributor.Jobs;
using Lykke.Job.NeoGasDistributor.Tests.Utils;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lykke.Job.NeoGasDistributor.Tests.Jobs
{
    [TestClass]
    public class CreateDistributionPlanJobTests
    {
        private const string CreateDistributionPlanCronString  = "0 3 2 * *";
        private const string CreateDistributionPlanDelayString = "1.00:00";
        
        [TestMethod]
        public async Task Test_that_distribution_plan_created_for_period_after_previous_distribution_plan()
        {
            // Arrange
            var balanceServiceMock = new Mock<IBalanceService>();
            var createDistributionPlanCron = CronExpression.Parse(CreateDistributionPlanCronString);
            var createDistributionPlanDelay = TimeSpan.Parse(CreateDistributionPlanDelayString);
            var dateTimeProvider = new FakeDateTimeProvider();
            var distributionPlanServiceMock = new Mock<IDistributionPlanService>();
            var latestPlanTimestamp = ParseUtc("2020-01-01T03:00:00Z");
            var actualPlans = new List<(DateTime, DateTime)>();
            
            // Setup
            dateTimeProvider.UtcNow = ParseUtc("2020-02-02T05:00:00Z");
            
            distributionPlanServiceMock
                .Setup(x => x.TryGetLatestPlanTimestampAsync())
                .ReturnsAsync(latestPlanTimestamp);

            distributionPlanServiceMock
                .Setup(x => x.CreatePlanAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((from, to) =>
                {
                    actualPlans.Add((from, to));
                })
                .Returns(Task.CompletedTask);
            
            // Act
            var job = new CreateDistributionPlanJob
            (
                balanceServiceMock.Object,
                createDistributionPlanCron,
                createDistributionPlanDelay,
                dateTimeProvider,
                distributionPlanServiceMock.Object
            );

            await job.ExecuteAsync();

            // Assert
            var expectedPlans = new List<(DateTime, DateTime)>
            {
                (latestPlanTimestamp, ParseUtc("2020-02-01T03:00:00Z"))
            };

            actualPlans
                .Should()
                .BeEquivalentTo(expectedPlans);
        }

        [TestMethod]
        public async Task Test_that_distribution_plan_created_on_first_execution()
        {
            // Arrange
            var balanceServiceMock = new Mock<IBalanceService>();
            var createDistributionPlanCron = CronExpression.Parse(CreateDistributionPlanCronString);
            var createDistributionPlanDelay = TimeSpan.Parse(CreateDistributionPlanDelayString);
            var dateTimeProvider = new FakeDateTimeProvider();
            var distributionPlanServiceMock = new Mock<IDistributionPlanService>();
            var firstBalanceUpdateTimestamp = ParseUtc("2020-01-01T02:45:00Z");
            var actualPlans = new List<(DateTime, DateTime)>();
            
            // Setup
            dateTimeProvider.UtcNow = ParseUtc("2020-02-02T05:00:00Z");

            balanceServiceMock
                .Setup(x => x.TryGetFirstBalanceUpdateTimestampAsync())
                .ReturnsAsync(firstBalanceUpdateTimestamp);
            
            distributionPlanServiceMock
                .Setup(x => x.TryGetLatestPlanTimestampAsync())
                .ReturnsAsync(default(DateTime?));

            distributionPlanServiceMock
                .Setup(x => x.CreatePlanAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((from, to) =>
                {
                    actualPlans.Add((from, to));
                })
                .Returns(Task.CompletedTask);
            
            // Act
            var job = new CreateDistributionPlanJob
            (
                balanceServiceMock.Object,
                createDistributionPlanCron,
                createDistributionPlanDelay,
                dateTimeProvider,
                distributionPlanServiceMock.Object
            );

            await job.ExecuteAsync();

            // Assert
            var expectedPlans = new List<(DateTime, DateTime)>
            {
                (firstBalanceUpdateTimestamp, ParseUtc("2020-02-01T03:00:00Z"))
            };

            actualPlans
                .Should()
                .BeEquivalentTo(expectedPlans);
        }

        private static DateTime ParseUtc(
            string dateTime)
        {
            return DateTime.Parse(dateTime).ToUniversalTime();
        }
    }
}
