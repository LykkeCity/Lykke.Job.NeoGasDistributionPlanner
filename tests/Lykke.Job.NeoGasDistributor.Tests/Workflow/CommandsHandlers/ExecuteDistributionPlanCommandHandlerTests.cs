using System;
using System.Threading.Tasks;
using FluentAssertions;
using Lykke.Cqrs;
using Lykke.Job.NeoGasDistributor.Domain.Services;
using Lykke.Job.NeoGasDistributor.Workflow.CommandHandlers;
using Lykke.Logs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NeoGasDistributor.Contract.Commands;

namespace Lykke.Job.NeoGasDistributor.Tests.Workflow.CommandsHandlers
{
    [TestClass]
    public class ExecuteDistributionPlanCommandHandlerTests
    {
        [TestMethod]
        public async Task Test_that_Handle_call_results_in_Ok_If_no_exception_thrown_and_plan_exists()
        {
            // Arrange
            var distributionPlanServiceMock = new Mock<IDistributionPlanService>();
            var eventPublisherMock = new Mock<IEventPublisher>();

            // Setup
            distributionPlanServiceMock
                .Setup(x => x.ExecutePlanAsync(It.IsAny<Guid>()))
                .Returns(Task.CompletedTask);
            
            distributionPlanServiceMock
                .Setup(x => x.PlanExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);
            
            // Act
            var handler = CreateHandler(distributionPlanServiceMock);

            var actualResult = await handler.Handle
            (
                new ExecuteDistributionPlanCommand(),
                eventPublisherMock.Object
            );

            // Assert
            var expectedResult = CommandHandlingResult.Ok();

            actualResult
                .Should()
                .BeEquivalentTo(expectedResult);
        }
        
        [TestMethod]
        public async Task Test_that_Handle_call_throws_exception_if_plan_does_not_exist()
        {
            // Arrange
            var distributionPlanServiceMock = new Mock<IDistributionPlanService>();
            var eventPublisherMock = new Mock<IEventPublisher>();

            // Setup
            distributionPlanServiceMock
                .Setup(x => x.ExecutePlanAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new InvalidOperationException());
            
            distributionPlanServiceMock
                .Setup(x => x.PlanExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(false);
            
            // Act
            var handler = CreateHandler(distributionPlanServiceMock);

            Func<Task<CommandHandlingResult>> action = () => handler.Handle
            (
                new ExecuteDistributionPlanCommand(),
                eventPublisherMock.Object
            );

            // Assert
            
            await action.Should()
                .ThrowAsync<InvalidOperationException>();
        }

        [TestMethod]
        public async Task Test_that_Handle_call_returns_Fail_with_30_seconds_delay_if_exception_thrown()
        {
            // Arrange
            var distributionPlanServiceMock = new Mock<IDistributionPlanService>();
            var eventPublisherMock = new Mock<IEventPublisher>();

            // Setup
            distributionPlanServiceMock
                .Setup(x => x.ExecutePlanAsync(It.IsAny<Guid>()))
                .ThrowsAsync(new Exception());
            
            distributionPlanServiceMock
                .Setup(x => x.PlanExistsAsync(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            // Act
            var handler = CreateHandler(distributionPlanServiceMock);
            
            var actualResult = await handler.Handle
            (
                new ExecuteDistributionPlanCommand(),
                eventPublisherMock.Object
            );
            
            // Assert
            var expectedResult = CommandHandlingResult.Fail(TimeSpan.FromSeconds(30));
            
            actualResult
                .Should()
                .BeEquivalentTo(expectedResult);
        }

        private static ExecuteDistributionPlanCommandHandler CreateHandler(
            IMock<IDistributionPlanService> distributionPlanServiceMock)
        {
            return new ExecuteDistributionPlanCommandHandler
            (
                distributionPlanServiceMock.Object,
                EmptyLogFactory.Instance
            );
        }
    }
}
