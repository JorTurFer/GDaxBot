using CoinbasePro.Shared.Types;
using GDaxBot.Model.Entities;
using System.Collections.Generic;

namespace GDaxBot.Coinbase.Model.Services.Coinbase
{
    public interface ICoinbaseService
    {
        void CheckProducts();
        event CoinbaseApiEventHandler AcctionNeeded;
        decimal GetUmbralUp(ProductType tipo);
        decimal GetUmbralDown(ProductType tipo);
        void SetUmbral(ProductType tipo,decimal umbral);
        string GetRatio(ProductType tipo);
        string GetRatio();
        decimal SetMarcador(ProductType tipo);

        IEnumerable<ProductType> GetActivosDisponibles();
    }
}
