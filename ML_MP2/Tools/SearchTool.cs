using System.Text.Json;
using ML_MP2.Models;
using System.Net.Http;

namespace ML_MP2.Tools;

public class SearchTool
{
    private readonly HttpClient _httpClient = new HttpClient();
    private const int MaxRetries = 5;

    /// <summary>
    /// Searches for academic research papers based on a topic, year constraints, and minimum citation count.
    /// Uses caching to avoid repeated API calls and help mitigate rate limiting.
    /// </summary>
    public async Task<string> SearchResearchPapers(string query, int? minYear, int? maxYear, int minCitations = 0)
    {
        // Check cache first to avoid repeated API calls
        string cacheKey = SearchCache.GenerateKey(query, minYear, maxYear, minCitations);
        if (SearchCache.TryGetCached(cacheKey, out var cachedResult))
        {
            Console.WriteLine("  [Using cached result]");
            return cachedResult!;
        }

        string yearParam = "";
        if (minYear.HasValue || maxYear.HasValue)
        {
            string min = minYear?.ToString() ?? "";
            string max = maxYear?.ToString() ?? "";
            yearParam = $"&year={min}-{max}";
        }

        string apiUrl = $"https://api.semanticscholar.org/graph/v1/paper/search?query={Uri.EscapeDataString(query)}&fields=title,year,citationCount,abstract,url,authors&limit=50{yearParam}";
        
        int retryCount = 0;
        while (retryCount < MaxRetries)
        {
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                
                // Handle rate limiting with exponential backoff
                if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    int delayMs = (int)Math.Pow(2, retryCount) * 2000;  // Increased to 2000ms base
                    Console.WriteLine($"  [Rate limited. Waiting {delayMs}ms before retry {retryCount + 1}/5...]");
                    await Task.Delay(delayMs);
                    retryCount++;
                    continue;
                }
                
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var searchResponse = JsonSerializer.Deserialize<SemanticScholarSearchResponse>(jsonResponse);
                
                if (searchResponse != null && searchResponse.Data != null && searchResponse.Data.Count > 0)
                {
                    var filteredPapers = searchResponse.Data
                        .Where(p => p.CitationCount >= minCitations)
                        .Take(5)
                        .ToList();

                    if (filteredPapers.Count == 0)
                    {
                        var noResultsMsg = "No papers found matching the citation constraint.";
                        SearchCache.Cache(cacheKey, noResultsMsg);
                        return noResultsMsg;
                    }

                    var results = filteredPapers
                        .Select(p => 
                        {
                            string authorsStr = p.Authors != null && p.Authors.Count > 0 
                                ? string.Join(", ", p.Authors.Select(a => a.Name))
                                : "Unknown authors";
                            return $"Title: {p.Title}\nAuthors: {authorsStr}\nYear: {p.Year}\nCitations: {p.CitationCount}\nAbstract: {p.Abstract}\nURL: {p.Url}";
                        })
                        .ToList();
                    
                    var resultString = string.Join("\n\n---\n\n", results);
                    SearchCache.Cache(cacheKey, resultString);  // Cache successful results
                    return resultString;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  [Error: {ex.Message}]");
                var errorMsg = $"Error searching papers: {ex.Message}";
                return errorMsg;
            }
        }
        
        var timeoutMsg = "Max retries exceeded. Try again later.";
        SearchCache.Cache(cacheKey, timeoutMsg);  // Cache timeout for consistency
        return timeoutMsg;
    }
}

