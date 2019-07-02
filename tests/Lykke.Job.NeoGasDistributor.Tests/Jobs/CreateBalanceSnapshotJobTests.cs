using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cronos;
using FluentAssertions;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using Lykke.Job.NeoGasDistributor.Jobs;
using Lykke.Job.NeoGasDistributor.Tests.Utils;
using Lykke.Logs;
using Lykke.Logs.Loggers.LykkeConsole;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Lykke.Job.NeoGasDistributor.Tests.Jobs
{
    [TestClass]
    public class CreateBalanceSnapshotJobTests
    {
        private const string CreateBalanceSnapshotCronString  = "5,20,35,50 * * * *";
        private const string CreateBalanceSnapshotDelayString = "00:05:00";
        
        [TestMethod]
        public async Task Test_that_all_missed_after_previous_execution_snapshots_created()
        {
            // Arrange
            var balanceServiceMock = new Mock<IBalanceService>();
            var createBalanceSnapshotCron = CronExpression.Parse(CreateBalanceSnapshotCronString);
            var createBalanceSnapshotDelay = TimeSpan.Parse(CreateBalanceSnapshotDelayString);
            var dateTimeProvider = new FakeDateTimeProvider();
            var actualSnapshots = new List<(DateTime, DateTime)>();
            var latestSnapshotTimestamp = ParseUtc("2020-01-01T03:00:00Z");
            
            // Setup
            dateTimeProvider.UtcNow = ParseUtc("2020-01-01T04:05:00Z");

            balanceServiceMock
                .Setup(x => x.TryGetLatestSnapshotTimestampAsync())
                .ReturnsAsync(latestSnapshotTimestamp);

            balanceServiceMock
                .Setup(x => x.CreateSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((from, to) =>
                {
                    actualSnapshots.Add((from, to));
                })
                .Returns(Task.CompletedTask);

            // Act
            var job = new CreateBalanceSnapshotJob
            (
                LogFactory.Create().AddUnbufferedConsole(),
                balanceServiceMock.Object,
                createBalanceSnapshotCron,
                createBalanceSnapshotDelay,
                dateTimeProvider
            );

            await job.ExecuteAsync();

            // Assert
            var expectedSnapshots = new List<(DateTime, DateTime)>
            {
                (latestSnapshotTimestamp, ParseUtc("2020-01-01T03:15:00Z")),
                (ParseUtc("2020-01-01T03:15:00Z"), ParseUtc("2020-01-01T03:30:00Z")),
                (ParseUtc("2020-01-01T03:30:00Z"), ParseUtc("2020-01-01T03:45:00Z")),
                (ParseUtc("2020-01-01T03:45:00Z"), ParseUtc("2020-01-01T04:00:00Z"))
            };
            
            actualSnapshots
                .Should()
                .BeEquivalentTo(expectedSnapshots);
        }
        
        [TestMethod]
        public async Task Test_that_all_missed_snapshots_created_on_first_execution()
        {
            // Arrange
            var balanceServiceMock = new Mock<IBalanceService>();
            var createBalanceSnapshotCron = CronExpression.Parse(CreateBalanceSnapshotCronString);
            var createBalanceSnapshotDelay = TimeSpan.Parse(CreateBalanceSnapshotDelayString);
            var dateTimeProvider = new FakeDateTimeProvider();
            var firstBalanceUpdateTimestamp = ParseUtc("2020-01-01T02:42:00Z");
            var actualSnapshots = new List<(DateTime, DateTime)>();


            // Setup
            dateTimeProvider.UtcNow = ParseUtc("2020-01-01T04:05:00Z");
            
            balanceServiceMock
                .Setup(x => x.TryGetLatestSnapshotTimestampAsync())
                .ReturnsAsync(default(DateTime?));
            
            balanceServiceMock
                .Setup(x => x.TryGetFirstBalanceUpdateTimestampAsync())
                .ReturnsAsync(firstBalanceUpdateTimestamp);
            
            balanceServiceMock
                .Setup(x => x.CreateSnapshotAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>()))
                .Callback<DateTime, DateTime>((from, to) =>
                {
                    actualSnapshots.Add((from, to));
                })
                .Returns(Task.CompletedTask);
            
            
            // Act
            var job = new CreateBalanceSnapshotJob
            (
                LogFactory.Create().AddUnbufferedConsole(),
                balanceServiceMock.Object,
                createBalanceSnapshotCron,
                createBalanceSnapshotDelay,
                dateTimeProvider
            );

            await job.ExecuteAsync();
            
            
            // Assert
            var expectedSnapshots = new List<(DateTime, DateTime)>
            {
                (firstBalanceUpdateTimestamp, ParseUtc("2020-01-01T02:45:00Z")),
                (ParseUtc("2020-01-01T02:45:00Z"), ParseUtc("2020-01-01T03:00:00Z")),
                (ParseUtc("2020-01-01T03:00:00Z"), ParseUtc("2020-01-01T03:15:00Z")),
                (ParseUtc("2020-01-01T03:15:00Z"), ParseUtc("2020-01-01T03:30:00Z")),
                (ParseUtc("2020-01-01T03:30:00Z"), ParseUtc("2020-01-01T03:45:00Z")),
                (ParseUtc("2020-01-01T03:45:00Z"), ParseUtc("2020-01-01T04:00:00Z"))
            };

            actualSnapshots
                .Should()
                .BeEquivalentTo(expectedSnapshots);
        }

        private static DateTime ParseUtc(
            string dateTime)
        {
            return DateTime.Parse(dateTime).ToUniversalTime();
        }
    }
}
