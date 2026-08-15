namespace PaymentTrackerApi.Services
{
    /// <summary>
    /// Downloads an image from a URL (e.g. an AiSensy/WhatsApp media link) and
    /// returns its bytes + content type so they can be stored directly in the
    /// database rather than just keeping a link that could expire.
    /// </summary>
    public class ImageDownloader
    {
        private readonly HttpClient _httpClient;

        public ImageDownloader(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(byte[]? data, string? contentType)> TryDownloadAsync(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return (null, null);

            try
            {
                var response = await _httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return (null, null);

                var bytes = await response.Content.ReadAsByteArrayAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                return (bytes, contentType);
            }
            catch
            {
                // Image download failing shouldn't block saving the rest of the bill details.
                return (null, null);
            }
        }
    }
}