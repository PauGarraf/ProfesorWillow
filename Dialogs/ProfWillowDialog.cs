using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.CognitiveServices.QnAMaker;
using Microsoft.Bot.Connector;

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
            #region Is Typing Activity

            var activity = context.Activity as Activity;
            Trace.TraceInformation($"Type={activity.Type} Text={activity.Text}");
            if (activity.Type == ActivityTypes.Message)
            {
                var connector = new ConnectorClient(new System.Uri(activity.ServiceUrl));
                var isTyping = activity.CreateReply("Nerdibot is thinking...");
                isTyping.Type = ActivityTypes.Typing;
                await connector.Conversations.ReplyToActivityAsync(isTyping);

                // DEMO: I've added this for demonstration purposes, so we have time to see the "Is Typing" integration in the UI. Else the bot is too quick for us :)
                Thread.Sleep(2500);
            }

            #endregion

            #region Handle incoming message

            var message = await argument;

            HttpClient client = new HttpClient();
            var chuckJoke = client.GetStringAsync("https://api.chucknorris.io/jokes/random").Result;

            var deserializedChuck = JsonConvert.DeserializeObject<dynamic>(chuckJoke);
            string chuckSays = ((dynamic)deserializedChuck).value.ToString();

            await context.PostAsync(GetRandomGreet(activity.From.Name) + Environment.NewLine + chuckSays);

            #endregion

            context.Wait(MessageReceivedAsync);
        }

        private string GetRandomGreet(string name)
        {
            List<string> greetings = new List<string>
            {
                $"Hey there {name}!",
                $"Okay {name}.",
                $"{name}, your request has been considered, and I approve. This time.",
                $"Okidoki {name}.",
                $"What's up my awesome pal, {name}."

            };
            Random r = new Random();
            int index = r.Next(greetings.Count);
            string randomString = greetings[index];

            return randomString;

        }
    }
    
}