using DeepArtConsoleClient.Settings;

namespace DeepArtConsoleClient.DeepArtAPI
{
    class DeepArtService : IDisposable
    {
        private readonly AppSettings _settings;
        private readonly Lazy<HttpClient> _httpClient;
        private readonly Lazy<GetClient> _getClient;
        private readonly Lazy<MediaClient> _mediaClient;
        private readonly Lazy<OperationClient> _operationClient;

        public DeepArtService(AppSettings settings)
        {
            _settings = settings;

            _httpClient = new Lazy<HttpClient>(() =>
            {
                // TODO use HttpClientFactory
                var http = new HttpClient();
                if (settings.HttpTimeout != null)
                {
                    http.Timeout = settings.HttpTimeout.Value;
                }
                return http;
            });

            _getClient = new Lazy<GetClient>(() => new GetClient(_httpClient.Value) { BaseUrl = _settings.ApiUrl!.ToString() });
            _mediaClient = new Lazy<MediaClient>(() => new MediaClient(_httpClient.Value) { BaseUrl = _settings.ApiUrl!.ToString() });
            _operationClient = new Lazy<OperationClient>(() => new OperationClient(_httpClient.Value) { BaseUrl = _settings.ApiUrl!.ToString() });
        }

        public async Task<ICollection<StyleInfo>> GetEffects(CancellationToken cancellation)
        {
            var response = await _getClient.Value.EffectsAsync(cancellation).ConfigureAwait(false);
            return response.Data;
        }

        public async Task<Guid?> AddMedia(FileParameter file, CancellationToken cancellation)
        {
            var location = await _mediaClient.Value.AddAsync(file, cancellation).ConfigureAwait(false);
            return GetGuid(location);
        }

        public async Task<Guid?> StartOperation(SubmitOperationParameters operation, CancellationToken cancellation)
        {
            var location = await _operationClient.Value.SubmitAsync(operation, cancellation).ConfigureAwait(false);
            return GetGuid(location);
        }

        public async Task<byte[]> GetOperationResult(Guid operationId, CancellationToken cancellation)
        {
            return await _operationClient.Value.CheckAsync(operationId, cancellation).ConfigureAwait(false);
        }

        private Guid? GetGuid(string location)
        {
            if (location == null) return null;

            var idx = location.LastIndexOf("/");
            var guid = idx >= 0 ? location.Substring(idx + 1) : location;

            return Guid.TryParse(guid, out var result) ? result : null;
        }

        public void Dispose()
        {
            if (_httpClient.IsValueCreated)
            {
                _httpClient.Value?.Dispose();
            }
        }
    }
}
