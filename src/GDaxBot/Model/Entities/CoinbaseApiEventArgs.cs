using CoinbasePro.Shared.Types;
using GDaxBot.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace GDaxBot.Model.Entities
{
    public class CoinbaseApiEventArgs : EventArgs
    {
        Usuario Usuario { get; set; }
        string Mensaje { get; set; }
    }
    public delegate void CoinbaseApiEventHandler(CoinbaseApiEventArgs e);
}
