using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Common.Log;
using Lykke.Common.Log;
using Newtonsoft.Json;

namespace Lykke.Job.NeoGasDistributor
{
    public class MeLogReader
    {
        private readonly string _logsFolderPath;
        private readonly ILog _log;

        public MeLogReader(
            ILogFactory logFactory,
            string logsFolderPath)
        {
            _log = logFactory.CreateLog(this);
            _logsFolderPath = logsFolderPath;
        }
        
        public IEnumerable<MeLogEntry> GetBalanceUpdates()
        {
            _log.Info("Reading 'rabbitMessages*.log' log files...");

            var logFiles = Directory.GetFiles(_logsFolderPath, "rabbitMessages*.log")
                .OrderBy(x => x)
                .ToArray();

            _log.Info($"{logFiles.Length} files have been found.");

            var fileNumber = 0;

            foreach (var logFile in logFiles)
            {
                _log.Info($"Reading file {logFile} ({++fileNumber} of {logFiles.Length})...");

                var file = File.OpenRead(logFile);
                using (var reader = new StreamReader(file, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 16777216, leaveOpen: false))
                {
                    var readLinesCount = 0;

                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();

                        ++readLinesCount;

                        if (readLinesCount % 10000 == 0)
                        {
                            _log.Info($"{readLinesCount} lines have been read in the current file");
                        }

                        if (line == null || line.Length < 91)
                        {
                            _log.Warning("Invalid line. Skipped", context: line);
                            continue;
                        }

                        //"25-06 00:00:05:693 INFO [message] lykke.spot.matching.engine.out.events : {"
                        if (line.Substring(19, 4) != "INFO")
                        {
                            _log.Warning("Not INFO line. Skipped", context: line);
                            continue;
                        }

                        if (line.Substring(34, 37) != "lykke.spot.matching.engine.out.events")
                        {
                            _log.Warning("Not lykke.spot.matching.engine.out.events line. Skipped", context: line);
                            continue;
                        }

                        if (line.Substring(76, 14) != "balanceUpdates")
                        {
                            _log.Warning("Not balanceUpdates line. Skipped", context: line);
                            continue;
                        }

                        var json = line.Substring(74);

                        MeLogEntry entry;

                        try
                        {
                            entry = JsonConvert.DeserializeObject<MeLogEntry>(json);
                        }
                        catch (Exception ex)
                        {
                            _log.Warning(
                                "Failed to parse entry as a json. Skipped",
                                context: new
                                {
                                    Line = line, 
                                    Json = json
                                },
                                exception: ex);
                            continue;
                        }

                        yield return entry;
                    }
                }
            }
        }
    }
}
