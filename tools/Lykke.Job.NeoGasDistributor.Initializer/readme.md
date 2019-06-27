To run this tool:

1. Place rabbitMessages ME log files somwhere in the file system. rabbitMessages ME log files consitst of lines like:

```
25-06 03:47:22:999 INFO [message] lykke.spot.matching.engine.out.events : {"balanceUpdates":[{"walletId":"707fffff-d3a8-aaaa-8ec4-eeeeeeeeeeee","assetId":"CHF","oldBalance":"474.9962","newBalance":"474.9962","oldReserved":"0","newReserved":"474.9958"}],"orders":[{"orderType":"LIMIT","id":"eeeeeee4-fffe-4aaa-940c-dcccccccccc3","externalId":"aaccccc6-98ce-4a53-bdd8-daaaaaeeeee6","assetPairId":"EURCHF","walletId":"707fffff-d3a8-aaaa-8ec4-eeeeeeeeeeee","side":"BUY","volume":"429.53","remainingVolume":"429.53","price":"1.10585","status":"PLACED","statusDate":"2019-06-25T03:47:22.998+0000","createdAt":"2019-06-25T03:47:22.997+0000","registered":"2019-06-25T03:47:22.998+0000","fees":[{"type":"NO_FEE","sourceWalletId":"707fffff-d3a8-aaaa-8ec4-eeeeeeeeeeee","targetWalletId":"8aaaaaaa-0ee1-4263-9484-cccccccccccc","assetsIds":[],"index":0}],"trades":[]}],"header":{"messageType":"ORDER","sequenceNumber":372681950,"messageId":"aaaaaaaa-9cce-4a53-bf68-ddddddddddd6","requestId":"aaaaaaa6-9aae-4a53-bf68-dcccccccccc6","version":"1","timestamp":"2019-06-25T03:47:22.998+0000","eventType":"LIMIT_ORDER"}}
```

2. Build the tool: ```dotnet build```
3. Run the tool from the csproj dir: ```dotnet run -- --logs <rabbitMessages-log-files-dir> --settings <Lykke.Job.NeoGasDistributor-settings-url>``` or 
from the bin dir: ```dotnet Lykke.Job.NeoGasDistributor.Initializer.dll --logs <rabbitMessages-log-files-dir> --settings <Lykke.Job.NeoGasDistributor-settings-url>```
