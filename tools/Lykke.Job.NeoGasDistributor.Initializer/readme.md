To run this tool:

1. Place rabbitMessages ME log files somwhere in the file system
2. Build the tool: ```dotnet build```
3. Run the tool from the csproj dir: ```dotnet run -- --logs <rabbitMessages-log-files-dir> --settings <Lykke.Job.NeoGasDistributor-settings-url>``` or 
from the bin dir: ```dotnet Lykke.Job.NeoGasDistributor.Initializer.dll --logs <rabbitMessages-log-files-dir> --settings <Lykke.Job.NeoGasDistributor-settings-url>```
