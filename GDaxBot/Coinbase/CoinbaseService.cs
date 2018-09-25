using CoinbasePro.Network.Authentication;
using CoinbasePro.Shared.Types;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GDaxBot.Coinbase
{
    class CoinbaseService : ICoinbaseService
    {
        public CoinbaseService(IOptions<Settings> secrets)
        {
            test(secrets);
        }
        async void test(IOptions<Settings> secrets)
        {
            //create an authenticator with your apiKey, apiSecret and passphrase
            var authenticator = new Authenticator(secrets.Value.CoinbaseKey, secrets.Value.CoinbaseSecret, secrets.Value.CoinbasePassword);

            //create the CoinbasePro client
            var coinbaseProClient = new CoinbasePro.CoinbaseProClient(authenticator);

            //use one of the services 
            var allAccounts = await coinbaseProClient.AccountsService.GetAllAccountsAsync();

            foreach (var a in allAccounts)
                Console.WriteLine($"{a.Currency}->{a.Balance}");
            List<int> productos = new List<int>();
            productos.Add(1); //BtcEur
            productos.Add(4); //EthUsd
            productos.Add(7); //LtcEur
            productos.Add(10); //BchEur
            productos.Add(13); //EtcEur

            foreach (var product in productos)
            {
                var res = await coinbaseProClient.ProductsService.GetProductTickerAsync((ProductType)product);
                Console.WriteLine($"{Enum.GetName(typeof(ProductType),product)}->{res.Price}");
            }
            var res2 = await coinbaseProClient.CurrenciesService.GetAllCurrenciesAsync();

            var ase = res2;

            var res3 = await coinbaseProClient.ProductsService.GetAllProductsAsync();

            var b = res3;

        }
    }
}
