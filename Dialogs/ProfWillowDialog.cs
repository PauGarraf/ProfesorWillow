using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

using ProfWillow.Entities;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Globalization;

namespace ProfWillow
{
    [Serializable]
    public class ProfWillowDialog :  IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            /* Wait until the first message is received from the conversation and call MessageReceviedAsync 
            *  to process that message. */
            context.Wait(this.MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            TelegramMessage telegramMessage = message.GetChannelData<TelegramMessage>();
            if (telegramMessage.Parameters != null && telegramMessage.Parameters.Latitute.HasValue == true)
            {
                float lat = telegramMessage.Parameters.Latitute.Value;
                float lon = telegramMessage.Parameters.Longitute.Value;
                await context.PostAsync($"Latitud: {lat}, Longitud: {lon}");
            }
            else if (message.Text.ToLower().StartsWith("/registrarmision"))
            {
                Quest q = ExtraerQuest(message.Text.ToLower());
                if (q != null)
                {
                    List<Quest> quests;
                    DateTime date;
                    DateTime today = message.Timestamp.Value.Date;

                    context.ConversationData.TryGetValue("QuestDate", out date);

                    if (date == null || date.Date != today)
                    {
                        context.ConversationData.SetValue("QuestList", new List<Quest>());
                        context.ConversationData.SetValue("QuestDate", today);
                    }
                    if (!context.ConversationData.TryGetValue("QuestList", out quests))
                    {
                        quests = new List<Quest>();
                    }
                    quests.Add(q);
                    quests = quests.OrderBy(o => o.Description).ToList();
                    context.ConversationData.SetValue("QuestList", quests);
                    await context.PostAsync($"Se ha registrado la misión de {q.Description} en {q.Location}");
                }
                else
                {
                    await context.PostAsync($"Lo siento, no te he entendido. Para registrar una misión escribe /registrarmision de <nombre de la mision> en <localización>");
                }
            }
            else if (message.Text.ToLower().StartsWith("/listamisiones"))
            {
                List<Quest> quests;
                DateTime date;
                DateTime today = message.Timestamp.Value.Date;
                string title = $"**** MISIONES {today.ToString("dd/MM/yyyy")} **** \n\n\n";
                string r = "";

                context.ConversationData.TryGetValue("QuestDate", out date);
                if (date == null || date.Date != today)
                {
                    context.ConversationData.SetValue("QuestList", new List<Quest>());
                    context.ConversationData.SetValue("QuestDate", today);
                }

                if (context.ConversationData.TryGetValue("QuestList", out quests))
                {
                    IEnumerable<IGrouping<string, string>> questsGouped = quests.GroupBy(q => q.Description, q => q.Location);
                    foreach (IGrouping<string, string> questGroup in questsGouped)
                    {
                        r += $"** MISIÓN {questGroup.Key.ToUpper()} **\n\n\n";
                        foreach (string location in questGroup)
                        {
                            r += $"- {location}\n\n";
                        }
                    }
                }
                if (string.IsNullOrEmpty(r)) r = "No hay misiones registradas.";
                await context.PostAsync(title + r);
            }
            else
            {
                await context.PostAsync($"Echo: {message.Text}");
            }
            

            context.Wait(MessageReceivedAsync);
        }

        public Quest ExtraerQuest(string message)
        {
            string input = message.Substring(16);

            int descPos = input.IndexOf(" de ");
            int locPos = input.IndexOf(" en ");

            if (descPos < 0 || locPos < 0) return null;

            string desc = "";
            string loc = "";
            if (descPos < locPos)
            {
                loc = input.Substring(locPos + 4);
                desc = input.Substring(descPos + 4, input.Length - loc.Length - 8);
            }
            else
            {
                desc = input.Substring(descPos + 4);
                loc = input.Substring(locPos + 4, input.Length - desc.Length - 8);
            }

            if (!string.IsNullOrEmpty(desc) && !string.IsNullOrEmpty(loc))
            {
                return new Quest()
                {
                    Description = desc.Trim(),
                    Location = loc.Trim()
                };
            }

            return null;
        }

    }
    
}