using System.Net;
using Autofac;
using JetBrains.Annotations;
using Lykke.Job.NeoGasDistributor.Settings;
using Lykke.Service.Assets.Client;
using Lykke.SettingsReader;

namespace Lykke.Job.NeoGasDistributor.Modules
{
    [UsedImplicitly]
    public class ClientsModule : Module
    {
        private readonly AppSettings _appSettings;
        
        
        public ClientsModule(
            IReloadingManager<AppSettings> appSettings)
        {
            _appSettings = appSettings.CurrentValue;
        }


        protected override void Load(
            ContainerBuilder builder)
        {
            builder
                .RegisterMeClient(GetIPEndPoint(_appSettings.MatchingEngineClient));

            builder
                .RegisterAssetsClient(_appSettings.AssetService);
        }
        
        private static IPEndPoint GetIPEndPoint(
            MatchingEngineClientSettings settings)
        {
            if (IPAddress.TryParse(settings.Host, out var ipAddress))
            {
                return new IPEndPoint(ipAddress, settings.Port);
            }

            var addresses = Dns.GetHostAddressesAsync(settings.Host).Result;
            
            return new IPEndPoint(addresses[0], settings.Port);
        }
    }
}
