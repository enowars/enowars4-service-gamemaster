namespace GamemasterChecker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using EnoCore.Checker;
    using Microsoft.Extensions.DependencyInjection;

    public class GamemasterCheckerInitializer : ICheckerInitializer
    {
        public int FlagsPerRound => 3;

        public int NoisesPerRound => 0;

        public int HavocsPerRound => 1;

        public string ServiceName => "Gamemaster";

        public void Initialize(IServiceCollection collection)
        {
            collection.AddSingleton(typeof(GamemasterCheckerDatabase));
            collection.AddHttpClient<GamemasterClient>("Foo")
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new HttpClientHandler()
                    {
                        UseCookies = false,
                        AllowAutoRedirect = false,
                    };
                });
            collection.AddTransient(typeof(GamemasterSignalRClient));
            collection.AddTransient(typeof(GamemasterClient));
        }
    }
}
