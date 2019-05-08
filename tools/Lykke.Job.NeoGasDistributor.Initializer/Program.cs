using Autofac;
using CommandLine;
using Lykke.Job.NeoGasDistributor.Settings;
using Lykke.SettingsReader;

namespace Lykke.Job.NeoGasDistributor
{
    internal static class Program
    {
        private static void Main(
            string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(Run);
        }

        private static void Run(
            Options options)
        {
            var settings = new SettingsServiceReloadingManager<AppSettings>
            (
                options.NeoGasDistributorSettingsUrl,
                opt => { }
            );
            
            var containerBuilder = new ContainerBuilder();
            var diModule = new DIModule(options.BalanceLogsFolderPath, settings);

            containerBuilder.RegisterModule(diModule);

            using (var container = containerBuilder.Build())
            {
                var balanceImporter = container.Resolve<BalanceUpdateImporter>();

                balanceImporter
                    .ImportBalancesAsync()
                    .Wait();
            }
        }
    }
}
