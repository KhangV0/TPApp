using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace TPApp.Infrastructure.AI
{
    /// <summary>
    /// OpenAI implementation of the embedding service using text-embedding-3-small model.
    /// Includes retry logic and comprehensive error handling.
    /// </summary>
    public class OpenAIEmbeddingService : IEmbeddingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenAIEmbeddingService> _logger;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly int _maxRetries;

        public OpenAIEmbeddingService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<OpenAIEmbeddingService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _apiKey = configuration["OpenAI:ApiKey"] 
                ?? throw new InvalidOperationException("OpenAI:ApiKey is not configured in appsettings.json");
            
            _model = configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-3-small";
            _maxRetries = int.TryParse(configuration["OpenAI:MaxRetries"], out var retries) ? retries : 3;

            _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Text cannot be null or empty", nameof(text));
            }

            var attempt = 0;
            Exception? lastException = null;

            while (attempt < _maxRetries)
            {
                attempt++;
                try
                {
                    _logger.LogDebug("Generating embedding (attempt {Attempt}/{MaxRetries})", attempt, _maxRetries);

                    var requestBody = new
                    {
                        input = text,
                        model = _model
                    };

                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    var response = await _httpClient.PostAsync("embeddings", content, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
                        var result = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseJson);

                        if (result?.Data == null || result.Data.Length == 0)
                        {
                            throw new InvalidOperationException("OpenAI API returned empty embedding data");
                        }

                        var embedding = result.Data[0].Embedding;
                        
                        if (embedding == null || embedding.Length == 0)
                        {
                            throw new InvalidOperationException("OpenAI API returned null or empty embedding vector");
                        }

                        _logger.LogDebug("Successfully generated embedding with {Dimensions} dimensions", embedding.Length);
                        return embedding;
                    }

                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("OpenAI API returned error: {StatusCode} - {Error}", 
                        response.StatusCode, errorContent);

                    if ((int)response.StatusCode >= 500)
                    {
                        lastException = new HttpRequestException(
                            $"OpenAI API server error: {response.StatusCode} - {errorContent}");
                        
                        if (attempt < _maxRetries)
                        {
                            var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                            _logger.LogInformation("Retrying after {Delay} seconds...", delay.TotalSeconds);
                            await Task.Delay(delay, cancellationToken);
                            continue;
                        }
                    }
                    else
                    {
                        throw new HttpRequestException(
                            $"OpenAI API client error: {response.StatusCode} - {errorContent}");
                    }
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    lastException = ex;
                    _logger.LogError(ex, "Error generating embedding (attempt {Attempt}/{MaxRetries})", 
                        attempt, _maxRetries);

                    if (attempt < _maxRetries)
                    {
                        var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                        _logger.LogInformation("Retrying after {Delay} seconds...", delay.TotalSeconds);
                        await Task.Delay(delay, cancellationToken);
                    }
                }
            }

            throw new InvalidOperationException(
                $"Failed to generate embedding after {_maxRetries} attempts", lastException);
        }

        private class OpenAIEmbeddingResponse
        {
            [System.Text.Json.Serialization.JsonPropertyName("data")]
            public EmbeddingData[]? Data { get; set; }
        }

        private class EmbeddingData
        {
            [System.Text.Json.Serialization.JsonPropertyName("embedding")]
            public float[]? Embedding { get; set; }
        }
    }
}
