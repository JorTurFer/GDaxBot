using System;
using System.Collections.Generic;
using System.Text;

namespace GDaxBot
{
    public class Settings
    {
        public string TelegramBotKey { get; set; }
        public int UserID { get; set; }

        public string CoinbaseKey { get; set; }
        public string CoinbaseSecret { get; set; }
        public string CoinbasePassword { get; set; }
        public int DiasAlmacenados { get; set; }
        public decimal UmbralDisparo { get; set; }
        public int MuestrasMinuto { get; set; }
    }
}
