using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace FileInfoBot
{
    public static class BotCallbackFunction
    {
        [FunctionName("BotCallbackFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

            var botClient = new TelegramBotClient(botToken);

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var update = JsonConvert.DeserializeObject<Update>(requestBody);

            try
            {
                if (update.Message != null)
                {
                    if (update.Message.Chat.Type == ChatType.Private)
                    {
                        var stringBuilder = new StringBuilder();

                        if (update.Message.Photo != null && update.Message.Photo[0] != null)
                        {
                            stringBuilder.AppendLine("Type: Photo");
                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine($"File ID: {update.Message.Photo[0].FileId}");
                            stringBuilder.AppendLine($"File size: {update.Message.Photo[0].FileSize}");
                            stringBuilder.AppendLine($"Width: {update.Message.Photo[0].Width} px");
                            stringBuilder.AppendLine($"Height: {update.Message.Photo[0].Height} px");
                        }

                        await botClient.SendTextMessageAsync(update.Message.Chat.Id, stringBuilder.ToString(), replyToMessageId: update.Message.MessageId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Error processing update");
            }

            return new OkResult();
        }
    }
}
