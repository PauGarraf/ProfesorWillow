using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Connector;

using ProfWillow.Entities;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace ProfWillow
{
    [Serializable]
    public class RootDialog :  IDialog<object>
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


            if (message.Text.ToLower().StartsWith("encontrada quest") || message.Text.ToLower().StartsWith("quest encontrada"))
            {
                Quest q = ExtraerQuest(message.Text.ToLower());
                if (q != null)
                {
                    List<Quest> quests; 
                    if (!context.ConversationData.TryGetValue("QuestList", out quests))
                    {
                        quests = new List<Quest>();
                    }
                    quests.Add(q);
                    quests = quests.OrderBy(o => o.Description).ToList();
                    context.ConversationData.SetValue("QuestList", quests);
                    await context.PostAsync($"Se ha registrado la quest de {q.Description} en {q.Location}");
                }
                else
                {
                    await context.PostAsync($"Lo siento, no te he entendido.");
                }
            }
            else if (message.Text.ToLower().Equals("lista de quests"))
            {
                List<Quest> quests;
                string r = "";

                if (context.ConversationData.TryGetValue("QuestList", out quests))
                {
                    foreach (Quest q in quests)
                    {
                        r += $"- {q.Description} en {q.Location} \n";
                    }
                }
                if (string.IsNullOrEmpty(r)) r = "No hay quests registradas.";
                await context.PostAsync(r);
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
                desc = input.Substring(descPos + 4, locPos).Trim();
                loc = input.Substring(locPos + 4).Trim();
            }
            else
            {
                loc = input.Substring(locPos + 4, descPos).Trim();
                desc = input.Substring(descPos + 4).Trim();
            }

            if (!string.IsNullOrEmpty(desc) && !string.IsNullOrEmpty(loc))
            {
                return new Quest()
                {
                    Description = desc,
                    Location = loc
                };
            }

            return null;
        }

    }
    
}