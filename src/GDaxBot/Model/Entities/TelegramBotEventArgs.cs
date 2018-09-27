using CoinbasePro.Shared.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace GDaxBot.Model.Entities
{
    public enum TelegramCommands
    {
        UmbralGet,
        UmbralSet,
        MarcadorSetTipo,
        RatioAll,
        RatioTipo
    }

    public class TelegramBotEventArgs : EventArgs
    {
        public TelegramCommands Comando { get; set; }
        public ProductType Tipo { get; set; }
        public decimal Valor { get; set; }
    }

    public delegate void TelegramBotEventHandler(TelegramBotEventArgs e);
}
