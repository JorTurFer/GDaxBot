using CoinbasePro.Shared.Types;
using GDaxBot.Coinbase;
using GDaxBot.Data;
using GDaxBot.Extensions;
using GDaxBot.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        private GDaxBotDbContext context;
        private readonly string botPassword;
        // I’ve injected <em>secrets</em> into the constructor as setup in Program.cs
        public TelegramBot(IConfiguration config, GDaxBotDbContext context)
        {
            this.context = context;
            botPassword = config.GetValue<string>("Settings:TelegramBotPassword");
            _bot = new TelegramBotClient(config.GetValue<string>("Settings:TelegramBotKey"));
            _bot.OnMessage += _bot_OnMessage;
            foreach (var sesion in context.Sesiones.Include(x => x.Usuario))
                SendMessage(sesion.IdTelegram, $"{sesion.Usuario.Nombre} , acabamos de reiniciar los servicios");
            _bot.StartReceiving();
        }

        public event TelegramBotEventHandler AcctionNeeded;

        public async void SendMessage(long ChatID, string Message)
        {
            await _bot.SendTextMessageAsync(ChatID, Message);
        }

        private async void _bot_OnMessage(object sender, MessageEventArgs e)
        {
            var message = e.Message;

            if (message == null || message.Type != MessageType.Text) return;

            var entrada = message.Text.ToLower().Split(' ');
            StringBuilder sb;
            switch (entrada.First())
            {
                case "-user":
                    if (entrada[2] == botPassword)
                    {
                        Sesion session = new Sesion();
                        Usuario usuario = await context.Usuarios.Where(x => x.Nombre.ToLower() == entrada[1].ToLower()).FirstOrDefaultAsync();
                        if (usuario == null) //Si no existe, creo el usuario y los ajustes
                        {
                            usuario = new Usuario();
                            usuario.Nombre = entrada[1];
                            context.Add(usuario);
                            foreach (var producto in context.Productos)
                            {
                                AjustesProducto ajuste = new AjustesProducto();
                                ajuste.IdProducto = producto.IdProducto;
                                ajuste.IdUsuario = usuario.IdUsuario;
                                ajuste.UmbralInferior = -5;
                                ajuste.UmbralSuperior = 5;
                                ajuste.ValorMarcado = context.Registros.Where(x => x.IdProducto == producto.IdProducto)
                                                                        .OrderByDescending(x => x.Fecha)
                                                                        .First().Valor;
                                context.Add(ajuste);
                            }
                        }
                        session.IdTelegram = message.Chat.Id;
                        session.IdUsuario = usuario.IdUsuario;
                        context.Add(session);
                        await context.SaveChangesAsync();
                        await _bot.SendTextMessageAsync(
                                message.Chat.Id,
                                "Se ha añadido la sesion a tu cuenta");
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(
                        message.Chat.Id,
                        "Contraseña incorrecta");
                    }
                    break;
                case "-help":
                    sb = new StringBuilder();
                    sb.AppendLine("Lista de Comandos:");
                    sb.AppendLine("\t\tUmbral get/set \"Activo\"");
                    sb.AppendLine("\t\tRatio All/\"Activo\"");
                    sb.AppendLine("\t\tMarcador \"Activo\"");
                    sb.AppendLine("\t\tActivos");

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
                case "marcador":
                    MarcadorCommand(entrada, message);
                    break;
                case "activos":
                    AcctionNeeded?.Invoke(new TelegramBotEventArgs { Comando = TelegramCommands.ActivosDisponibles });
                    break;
                default:
                    if (context.Sesiones.Where(x => x.IdTelegram == message.Chat.Id).Count() == 0)
                    {
                        await _bot.SendTextMessageAsync(
                                message.Chat.Id,
                                "Introduce la contraseña y tu usuario mediante el comando -user \"Usuario\" ContraseñaDelBot");
                        return;
                    }
                    else
                    {
                        await _bot.SendTextMessageAsync(
                                       message.Chat.Id,
                                       "Envia una orden, si tienes dudas, envia \"-help\" para pedir ayuda");
                    }
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
                    var ajustes = await context.AjustesProductos.Where(x => x.Usuario.Sesiones.Any(y => y.IdTelegram == message.Chat.Id)
                                                            && x.Producto.Nombre.ToLower() == entrada[2]).Include(x=>x.Producto).FirstAsync();

                    await _bot.SendTextMessageAsync(
                           message.Chat.Id,
                           $"Los umbrales de notificacion de {ajustes.Producto.Nombre} son {ajustes.UmbralInferior.ToString("0.00")}% y {ajustes.UmbralSuperior.ToString("0.00")}%");
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
            if (entrada[1] == "all")
            {
                AcctionNeeded?.Invoke(new TelegramBotEventArgs { Comando = TelegramCommands.RatioAll });
            }
            else
            {
                try
                {
                    if (Enum.TryParse(typeof(ProductType), entrada[1].FirstLetterCapital() + "Eur", out object tipo))
                    {
                        AcctionNeeded?.Invoke(new TelegramBotEventArgs { Comando = TelegramCommands.RatioTipo, Tipo = (ProductType)tipo });
                    }
                    else
                        throw new ArgumentException();
                }
                catch
                {
                    await _bot.SendTextMessageAsync(
                           message.Chat.Id,
                           "Envia una orden válida, si tienes dudas, envia \"Ratio -help\" para pedir ayuda");
                }
            }
        }
        private async void MarcadorCommand(string[] entrada, Message message)
        {
            if (entrada.Length == 1 || entrada[1] == "-help")
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Lista de Subcomandos \"Marcador\":");
                sb.AppendLine("\tMarcador \"Activo\"->Indica el calor sobre el que operan los porcentajes personalizados");
                await _bot.SendTextMessageAsync(
                message.Chat.Id,
                sb.ToString());
                return;
            }
            try
            {
                if (Enum.TryParse(typeof(ProductType), entrada[1].FirstLetterCapital() + "Eur", out object tipo))
                {
                    AcctionNeeded?.Invoke(new TelegramBotEventArgs { Comando = TelegramCommands.MarcadorSetTipo, Tipo = (ProductType)tipo });
                }
                else
                    throw new ArgumentException();
            }
            catch
            {
                await _bot.SendTextMessageAsync(
                       message.Chat.Id,
                       "Envia una orden válida, si tienes dudas, envia \"Marcador -help\" para pedir ayuda");
            }

        }
    }
}
