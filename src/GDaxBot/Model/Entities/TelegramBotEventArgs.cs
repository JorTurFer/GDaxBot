using CoinbasePro.Shared.Types;
using GDaxBot.Data;
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
        RatioTipo,
        ActivosDisponibles
    }

    public class TelegramBotEventArgs : EventArgs
    {
        Usuario Usuario { get; set; }
        public TelegramCommands Comando { get; set; }
        public ProductType Tipo { get; set; }
        public decimal Valor { get; set; }
    }

    public delegate void TelegramBotEventHandler(TelegramBotEventArgs e);
}
