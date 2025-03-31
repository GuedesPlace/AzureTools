using System.Text;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;

namespace GuedesPlace.AzureTools.Extensions;

public static class QueueExtensions
{
    public static T? DeserializeMessage<T>(this QueueMessage message)
    {
        string messageText = message.Body.ToString();
        byte[] data = Convert.FromBase64String(messageText);
        string decodedString = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(decodedString);
    }
    public static async Task<SendReceipt> SendPayloadToQueueAsync(this QueueClient client, object payload)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
        return await client.SendMessageAsync(Convert.ToBase64String(plainTextBytes));
    }
    public static async Task<SendReceipt> SendPayloadToQueueDelayed(this QueueClient client, object payload, double delaySeconds)
    {
        var timeSpan = TimeSpan.FromSeconds(delaySeconds);
        string payloadString = JsonConvert.SerializeObject(payload);
        string b64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadString));
        return await client.SendMessageAsync(b64Payload, timeSpan);
    }
}