using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Lykke.Job.NeoGasDistributor
{
    public class MELogReader
    {
        private readonly string _logsFolderPath;
        
        public MELogReader(
            string logsFolderPath)
        {
            _logsFolderPath = logsFolderPath;
        }
        
        public IEnumerable<MELogRecord> GetRecords()
        {
            var logFiles = Directory.GetFiles(_logsFolderPath, "balanceUpdates-*.json");

            foreach (var logFile in logFiles)
            {
                var logContent = File.ReadAllText(logFile);
                var log = JsonConvert.DeserializeObject<MELog>(logContent, new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                });

                foreach (var logRecord in log.BalanceUpdates)
                {
                    yield return logRecord;
                }
            }
        }
    }
}
