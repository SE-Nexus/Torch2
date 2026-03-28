using System.Text.Json.Serialization;
using System.Text;

namespace Torch2WebUI.Services.ModServices
{
    public interface ISteamService
    {
        Task<Dictionary<string, SteamPublishedFileDetails>> GetPublishedFileDetailsAsync(params string[] publishedFileIds);
    }

    public class SteamService : ISteamService
    {
        private readonly HttpClient _httpClient;
        private const string SteamAPIUrl = "https://api.steampowered.com/ISteamRemoteStorage/GetPublishedFileDetails/v1/";

        public SteamService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Dictionary<string, SteamPublishedFileDetails>> GetPublishedFileDetailsAsync(params string[] publishedFileIds)
        {
            if (publishedFileIds == null || publishedFileIds.Length == 0)
                return new Dictionary<string, SteamPublishedFileDetails>();

            System.Diagnostics.Debug.WriteLine($"SteamService: Fetching {publishedFileIds.Length} mod details from Steam API");

            // Batch requests - Steam API allows up to 100 per request
            var results = new Dictionary<string, SteamPublishedFileDetails>();
            var batches = publishedFileIds.Chunk(100);

            foreach (var batch in batches)
            {
                using (var content = new FormUrlEncodedContent(BuildRequestContent(batch)))
                {
                    try
                    {
                        var response = await _httpClient.PostAsync(SteamAPIUrl, content);
                        response.EnsureSuccessStatusCode();

                        var responseContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"SteamService: Received response for batch of {batch.Length} mods");

                        var steamResponse = System.Text.Json.JsonSerializer.Deserialize<SteamApiResponse>(responseContent);

                        if (steamResponse?.Response?.PublishedFileDetails != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"SteamService: Response contains {steamResponse.Response.PublishedFileDetails.Count} file details");

                            foreach (var details in steamResponse.Response.PublishedFileDetails)
                            {
                                if (!results.ContainsKey(details.PublishedFileId))
                                {
                                    results[details.PublishedFileId] = details;
                                    System.Diagnostics.Debug.WriteLine($"SteamService: Added {details.PublishedFileId} - {details.Title}");
                                }
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"SteamService: Response is null or empty");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log error but don't throw - allow partial success
                        System.Diagnostics.Debug.WriteLine($"Error fetching Steam details: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine($"SteamService: Returning {results.Count} mod details total");
            return results;
        }

        private List<KeyValuePair<string, string>> BuildRequestContent(string[] fileIds)
        {
            var content = new List<KeyValuePair<string, string>>
            {
                new("itemcount", fileIds.Length.ToString())
            };

            for (int i = 0; i < fileIds.Length; i++)
            {
                content.Add(new($"publishedfileids[{i}]", fileIds[i]));
            }

            return content;
        }
    }

    public class SteamApiResponse
    {
        [JsonPropertyName("response")]
        public SteamResponse Response { get; set; } = new();
    }

    public class SteamResponse
    {
        [JsonPropertyName("result")]
        public int Result { get; set; }

        [JsonPropertyName("resultcount")]
        public int ResultCount { get; set; }

        [JsonPropertyName("publishedfiledetails")]
        public List<SteamPublishedFileDetails> PublishedFileDetails { get; set; } = new();
    }

    public class SteamPublishedFileDetails
    {
        [JsonPropertyName("publishedfileid")]
        public string PublishedFileId { get; set; } = string.Empty;

        [JsonPropertyName("result")]
        public int Result { get; set; }

        [JsonPropertyName("file_size")]
        public string FileSize { get; set; } = string.Empty;

        [JsonPropertyName("preview_url")]
        public string PreviewUrl { get; set; } = string.Empty;

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("time_created")]
        public long TimeCreated { get; set; }

        [JsonPropertyName("time_updated")]
        public long TimeUpdated { get; set; }

        [JsonPropertyName("subscriptions")]
        public int Subscriptions { get; set; }

        [JsonPropertyName("favorited")]
        public int Favorites { get; set; }

        [JsonPropertyName("views")]
        public int Views { get; set; }

        [JsonPropertyName("tags")]
        public List<SteamTag> Tags { get; set; } = new();
    }

    public class SteamTag
    {
        [JsonPropertyName("tag")]
        public string Tag { get; set; } = string.Empty;
    }
}
