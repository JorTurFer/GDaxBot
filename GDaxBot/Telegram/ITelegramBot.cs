using System;
using System.Collections.Generic;
using System.Text;

namespace GDaxBot.Telegram
{
    interface ITelegramBot
    {
        void SendMessage(string Message);
    }
}
