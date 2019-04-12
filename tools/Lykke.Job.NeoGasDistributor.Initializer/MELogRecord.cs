using System;

namespace Lykke.Job.NeoGasDistributor
{
    public class MELogRecord
    {
        public Guid Asset { get; set; }
        
        public DateTime Date { get; set; }
        
        public Guid Id { get; set; }
        
        public decimal NewBalance { get; set; }
    }
}
