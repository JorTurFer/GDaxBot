using CoinbasePro.Shared.Types;
using GDaxBot.Coinbase;
using GDaxBot.Extensions;
using GDaxBot.Model.Entities;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GDaxBot.Coinbase.Model.Services.Telegram
{
    class TelegramBot : ITelegramBot
    {
        private readonly TelegramBotClient _bot;
        private readonly int _userID;

        // I’ve injected <em>secrets</em> into the constructor as setup in Program.cs
        public TelegramBot(IOptions<Settings> secrets)
        {
            _userID = secrets.Value.UserID;
            _bot = new TelegramBotClient(secrets.Value.TelegramBotKey);
            _bot.OnMessage += _bot_OnMessage;
            _bot.StartReceiving();
            SendMessage("Iniciando los servicios de monitorizacion");
        }

        public event TelegramBotEventHandler AcctionNeeded;

        public async void SendMessage(string Message)
        {
            await _bot.SendTextMessageAsync(_userID, Message);
        }

        private async void _bot_OnMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;

            if (message == null || message.Type != MessageType.Text) return;
            //Si el usuario no esta dado de alta, rechaza la conexion
            if (message.From.Id != _userID)
            {
                await _bot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Usuario no autorizado");
                return;
            }
            var entrada = message.Text.ToLower().Split(' ');
            StringBuilder sb;
            switch (entrada.First())
            {
                case "-help":
                    sb = new StringBuilder();
                    sb.AppendLine("Lista de Comandos:");
                    sb.AppendLine("\t\tUmbral get/set \"Activo\"");

                    await _bot.SendTextMessageAsync(
                        message.Chat.Id,
                        sb.ToString());
                    break;
                case "umbral":
                    UmbralCommand(entrada, message);
                    break;
                case "ratio":
                    RatioCommand(entrada, message);
                    break;
                case "alive":
                    await _bot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Estoy vivo");
                    break;
                default:
                    await _bot.SendTextMessageAsync(
                                   message.Chat.Id,
                                   "Envia una orden, si tienes dudas, envia \"-help\" para pedir ayuda");
                    break;
            }
        }

        private async void UmbralCommand(string[] entrada, Message message)
        {
            if (entrada.Length == 1 || entrada[1] == "-help")
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Lista de Subcomandos \"Umbral\":");
                sb.AppendLine("\tGet \"Activo\"->Obtiene el valor de activacion de la alerta para el activo indicado");
                sb.AppendLine("\tSet \"Activo\" \"porcentaje\"->Modifica el valor de activaciond e la alerta para el activo indicado");
                await _bot.SendTextMessageAsync(
                message.Chat.Id,
                sb.ToString());
                return;
            }
            if (entrada[1] == "get")
            {
                try
                {
                    if (Enum.TryParse(typeof(ProductType), entrada[2].FirstLetterCapital() + "Eur", out object tipo))
                    {
                        AcctionNeeded?.Invoke(new TelegramBotEventArgs { Comando = TelegramCommands.UmbralGet, Tipo = (ProductType)tipo });
                    }
                    else
                        throw new ArgumentException();
                }
                catch
                {
                    await _bot.SendTextMessageAsync(
                           message.Chat.Id,
                           "Envia una orden válida, si tienes dudas, envia \"umbral -help\" para pedir ayuda");
                }
            }
            else if (entrada[1] == "set")
            {
                try
                {
                    if (Enum.TryParse(typeof(ProductType), entrada[2].FirstLetterCapital() + "Eur", out object tipo))
                    {
                        if (decimal.TryParse(entrada[3], out decimal valor))
                        {
                            AcctionNeeded?.Invoke(new TelegramBotEventArgs { Comando = TelegramCommands.UmbralSet, Tipo = (ProductType)tipo, Valor = valor });
                        }
                        else
                            throw new ArgumentException();
                    }
                    else
                        throw new ArgumentException();
                }
                catch
                {
                    await _bot.SendTextMessageAsync(
                           message.Chat.Id,
                           "Envia una orden válida, si tienes dudas, envia \"umbral -help\" para pedir ayuda");
                }
            }
            else
                await _bot.SendTextMessageAsync(
                           message.Chat.Id,
                           "Envia una orden válida, si tienes dudas, envia \"umbral -help\" para pedir ayuda");
        }
        private async void RatioCommand(string[] entrada, Message message)
        {
            if (entrada.Length == 1 || entrada[1] == "-help")
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Lista de Subcomandos \"Ratio\":");
                sb.AppendLine("\tRatio All/\"Activo\"->Obtiene el valor de los activos o del activo seleccionado");
                await _bot.SendTextMessageAsync(
                message.Chat.Id,
                sb.ToString());
                return;
            }
            if (entrada[1] == "get")
            {
                try
                {
                    if (Enum.TryParse(typeof(ProductType), entrada[2].FirstLetterCapital() + "Eur", out object tipo))
                    {
                        AcctionNeeded?.Invoke(new TelegramBotEventArgs { Comando = TelegramCommands.UmbralGet, Tipo = (ProductType)tipo });
                    }
                    else
                        throw new ArgumentException();
                }
                catch
                {
                    await _bot.SendTextMessageAsync(
                           message.Chat.Id,
                           "Envia una orden válida, si tienes dudas, envia \"-help\" para pedir ayuda");
                }
            }
            else if (entrada[1] == "set")
            {
                try
                {
                    if (Enum.TryParse(typeof(ProductType), entrada[2].FirstLetterCapital() + "Eur", out object tipo))
                    {
                        if (decimal.TryParse(entrada[3], out decimal valor))
                        {
                            AcctionNeeded?.Invoke(new TelegramBotEventArgs { Comando = TelegramCommands.UmbralSet, Tipo = (ProductType)tipo, Valor = valor });
                        }
                        else
                            throw new ArgumentException();
                    }
                    else
                        throw new ArgumentException();
                }
                catch
                {
                    await _bot.SendTextMessageAsync(
                           message.Chat.Id,
                           "Envia una orden válida, si tienes dudas, envia \"-help\" para pedir ayuda");
                }
            }
        }
    }
}
