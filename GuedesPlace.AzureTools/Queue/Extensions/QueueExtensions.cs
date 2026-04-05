using System.Text;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Newtonsoft.Json;

namespace GuedesPlace.AzureTools.Queue.Extensions;

public static class QueueExtensions
{
    public static T? DeserializeMessage<T>(this QueueMessage message)
    {
        string messageText = message.Body.ToString();
        byte[] data = Convert.FromBase64String(messageText);
        string decodedString = Encoding.UTF8.GetString(data);
        return JsonConvert.DeserializeObject<T>(decodedString);
    }
    public static string ExtractText(this QueueMessage message)
    {
        string messageText = message.Body.ToString();
        byte[] data = Convert.FromBase64String(messageText);
        return Encoding.UTF8.GetString(data);
    }
    public static async Task<SendReceipt> SendPayloadToQueueAsync(this QueueClient client, object payload, CancellationToken token = default)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
        return await client.SendMessageAsync(Convert.ToBase64String(plainTextBytes), null, null, token);
    }
    public static async Task<SendReceipt> SendPayloadToQueueDelayed(this QueueClient client, object payload, double delaySeconds, CancellationToken token = default)
    {
        var timeSpan = TimeSpan.FromSeconds(delaySeconds);
        string payloadString = JsonConvert.SerializeObject(payload);
        string b64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadString));
        return await client.SendMessageAsync(b64Payload, timeSpan, null, token);
    }
    public static async Task<SendReceipt> SendPayloadToQueueWithDefinedLifeTime(this QueueClient client, object payload, TimeSpan lifeTime, CancellationToken token = default)
    {
        string payloadString = JsonConvert.SerializeObject(payload);
        string b64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadString));
        return await client.SendMessageAsync(b64Payload, null, lifeTime, token);
    }
    public static async Task<SendReceipt> SendPayloadToQueueDelayedWithDefinedLifeTime(this QueueClient client, object payload, double delaySeconds, TimeSpan lifeTime, CancellationToken token = default)
    {
        var timeSpan = TimeSpan.FromSeconds(delaySeconds);
        string payloadString = JsonConvert.SerializeObject(payload);
        string b64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadString));
        return await client.SendMessageAsync(b64Payload, timeSpan, lifeTime, token);
    }
}