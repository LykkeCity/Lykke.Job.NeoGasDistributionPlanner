using System;
using JetBrains.Annotations;
using ProtoBuf;

namespace NeoGasDistributor.Contract.Commands
{
    [PublicAPI]
    [ProtoContract]
    public class ExecuteDistributionPlanCommand
    {
        [ProtoMember(1)]
        public Guid PlanId { get; set; }
    }
}
