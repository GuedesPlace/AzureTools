using System.Text;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Newtonsoft.Json;

namespace GuedesPlace.AzureTools.Blob.Extensions;

public static class BlobExtensions {

    public static async Task<Response<BlobContentInfo>> StoreAsJsonAsync(this BlobContainerClient blobContainerClientClient, string filePathAndName, object payload) {
        var blobClient = blobContainerClientClient.GetBlobClient($"{filePathAndName}.json");
        byte[] byteArray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
        MemoryStream stream = new MemoryStream(byteArray);

        var uploadOptions = new BlobUploadOptions
        {
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = "application/json"
            }
        };
        return await blobClient.UploadAsync(stream, uploadOptions);
    }
    public static async Task<T?> RetreiveFromStoreAsync<T>(this BlobContainerClient blobContainerClient, string filePathAndName) {
        var blobClient = blobContainerClient.GetBlobClient($"{filePathAndName}.json");
        BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
        string blobContents = downloadResult.Content.ToString();
        return JsonConvert.DeserializeObject<T>(blobContents);
    }
}