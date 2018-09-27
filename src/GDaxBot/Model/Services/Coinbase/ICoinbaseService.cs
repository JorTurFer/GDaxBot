using CoinbasePro.Shared.Types;
using GDaxBot.Model.Entities;

namespace GDaxBot.Coinbase.Model.Services.Coinbase
{
    public interface ICoinbaseService
    {
        void CheckProducts();
        event CoinbaseApiEventHandler AcctionNeeded;
        decimal GetUmbral(ProductType tipo);
        void SetUmbral(ProductType tipo,decimal umbral);
    }
}
