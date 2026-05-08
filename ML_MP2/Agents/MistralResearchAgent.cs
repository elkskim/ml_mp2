using ML_MP2.Tools;
using ML_MP2.Config;
using System.Text.Json;

namespace ML_MP2.Agents;

/// <summary>
/// AutoGen-based Research Scout Agent using Mistral AI to parse natural language
/// queries and call the SearchTool to find research papers with constraint filtering.
/// </summary>
public class MistralResearchAgent
{
    private readonly SearchTool _searchTool;
    private readonly string _mistralApiKey;
    private readonly string _mistralModel;

    public MistralResearchAgent()
    {
        _searchTool = new SearchTool();
        _mistralApiKey = LlmConfig.GetMistralApiKey();
        _mistralModel = LlmConfig.GetMistralModel();
    }

    /// <summary>
    /// Processes a natural language research query and returns a structured recommendation.
    /// Uses Mistral AI to extract constraints, then calls SearchTool to fetch papers.
    /// </summary>
    public async Task<ResearchRecommendation> ProcessQueryAsync(string userQuery)
    {
        // Step 1: Extract constraints using simple pattern matching (fast and reliable)
        Console.WriteLine("[AGENT] Analyzing query with constraint extraction...");
        var constraints = SimpleConstraintExtractor.Extract(userQuery);

        // Step 2: Call SearchTool with extracted constraints
        Console.WriteLine($"[AGENT] Extracted constraints:");
        Console.WriteLine($"  • Topic: {constraints.Topic}");
        Console.WriteLine($"  • Min Year: {constraints.MinYear}");
        Console.WriteLine($"  • Max Year: {constraints.MaxYear}");
        Console.WriteLine($"  • Min Citations: {constraints.MinCitations}");

        Console.WriteLine("\n[TOOL] Calling SearchTool...");
        string searchResults = await _searchTool.SearchResearchPapers(
            constraints.Topic,
            constraints.MinYear,
            constraints.MaxYear,
            constraints.MinCitations
        );

        // If we got rate-limited or no results, try a broader search with fewer constraints
        if ((searchResults.Contains("No papers found") || searchResults.Contains("Max retries exceeded")) && 
            (constraints.MinYear.HasValue || constraints.MaxYear.HasValue || constraints.MinCitations > 0))
        {
            Console.WriteLine("\n[TOOL] Broadening search (rate limited, trying fewer constraints)...");
            searchResults = await _searchTool.SearchResearchPapers(
                constraints.Topic,
                null,  // Remove year constraint
                null,
                Math.Max(0, constraints.MinCitations - 50)  // Lower citation requirement by 50
            );
        }

        // Add delay to avoid rate limiting on subsequent queries
        await Task.Delay(3000);

        // Step 3: Parse search results
        var papers = ParseSearchResults(searchResults);

        if (papers.Count == 0)
        {
            return new ResearchRecommendation
            {
                Success = false,
                Error = "No papers found matching the constraints",
                UserQuery = userQuery
            };
        }

        // Step 4: Use Mistral to generate explanation for the best result
        Console.WriteLine("\n[AGENT] Generating recommendation with Mistral AI...");
        var recommendation = await GenerateRecommendationWithMistralAsync(
            userQuery,
            constraints,
            papers[0]
        );

        return recommendation;
    }

    /// <summary>
    /// Fallback method to extract topic from query if needed.
    /// </summary>
    private string ExtractTopicFromQueryFallback(string query)
    {
        // Simple heuristic: return a substring that might be the topic
        var words = query.Split(' ');
        return string.Join(" ", words.Take(Math.Min(4, words.Length)));
    }

    /// <summary>
    /// Uses Mistral AI to generate a short explanation of why the paper is relevant.
    /// </summary>
    private async Task<ResearchRecommendation> GenerateRecommendationWithMistralAsync(
        string userQuery,
        ConstraintExtraction constraints,
        Paper paper)
    {
        try
        {
            var systemPrompt = @"You are a research recommendation agent. Generate a brief, evidence-based explanation 
of why a specific paper matches a research query. Be concise (2-3 sentences). 
Focus on how the paper's citations, year, and title relate to the user's request.
Always cite the source (Semantic Scholar) for all numbers.";

            string userMessage = $@"User query: {userQuery}

Paper found:
- Title: {paper.Title}
- Year: {paper.Year}
- Citations: {paper.CitationCount} (from Semantic Scholar)
- URL: {paper.Url}
- Abstract: {paper.Abstract}

Why is this paper relevant to the user's query?";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_mistralApiKey}");

                var requestBody = new
                {
                    model = _mistralModel,
                    messages = new object[]
                    {
                        new { role = "system", content = systemPrompt },
                        new { role = "user", content = userMessage }
                    },
                    temperature = 0.5
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("https://api.mistral.ai/v1/chat/completions", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[WARNING] Mistral API error: {response.StatusCode}");
                    // Return basic recommendation without LLM generation
                    return BasicRecommendation(userQuery, constraints, paper);
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                using (JsonDocument doc = JsonDocument.Parse(responseJson))
                {
                    var root = doc.RootElement;
                    var explanation = root
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "";

                    return new ResearchRecommendation
                    {
                        Success = true,
                        UserQuery = userQuery,
                        Paper = paper,
                        Explanation = explanation,
                        CitationSource = "Semantic Scholar API"
                    };
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[WARNING] Failed to generate explanation: {ex.Message}");
            return BasicRecommendation(userQuery, constraints, paper);
        }
    }

    /// <summary>
    /// Generates a basic recommendation without LLM if needed.
    /// </summary>
    private ResearchRecommendation BasicRecommendation(
        string userQuery,
        ConstraintExtraction constraints,
        Paper paper)
    {
        var explanation = $"This paper matches your criteria: published in {paper.Year} " +
            $"with {paper.CitationCount} citations (from Semantic Scholar). " +
            $"The abstract is: {paper.Abstract}";

        return new ResearchRecommendation
        {
            Success = true,
            UserQuery = userQuery,
            Paper = paper,
            Explanation = explanation,
            CitationSource = "Semantic Scholar API"
        };
    }

    /// <summary>
    /// Parses search results from SearchTool into structured Paper objects.
    /// </summary>
    private List<Paper> ParseSearchResults(string results)
    {
        var papers = new List<Paper>();

        if (results.Contains("Error") || results.Contains("Max retries exceeded"))
            return papers;

        var entries = results.Split(new[] { "\n\n---\n\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var entry in entries)
        {
            try
            {
                var lines = entry.Split('\n');
                var paper = new Paper();

                foreach (var line in lines)
                {
                    if (line.StartsWith("Title:"))
                        paper.Title = line.Replace("Title:", "").Trim();
                    else if (line.StartsWith("Authors:"))
                        paper.Authors = line.Replace("Authors:", "").Trim();
                    else if (line.StartsWith("Year:"))
                    {
                        if (int.TryParse(line.Replace("Year:", "").Trim(), out int year))
                            paper.Year = year;
                    }
                    else if (line.StartsWith("Citations:"))
                    {
                        if (int.TryParse(line.Replace("Citations:", "").Trim(), out int cites))
                            paper.CitationCount = cites;
                    }
                    else if (line.StartsWith("URL:"))
                        paper.Url = line.Replace("URL:", "").Trim();
                    else if (line.StartsWith("Abstract:"))
                        paper.Abstract = line.Replace("Abstract:", "").Trim();
                }

                if (!string.IsNullOrEmpty(paper.Title))
                    papers.Add(paper);
            }
            catch { /* skip malformed entries */ }
        }

        return papers;
    }
}

/// <summary>
/// Extracted constraints from a natural language query
/// </summary>
public class ConstraintExtraction
{
    public string Topic { get; set; } = "machine learning";
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public int MinCitations { get; set; } = 0;
}

/// <summary>
/// Structured paper data
/// </summary>
public class Paper
{
    public string Title { get; set; } = "";
    public string Authors { get; set; } = "Unknown authors";
    public int Year { get; set; }
    public int CitationCount { get; set; }
    public string Url { get; set; } = "";
    public string Abstract { get; set; } = "";
}

/// <summary>
/// Final recommendation returned to the user
/// </summary>
public class ResearchRecommendation
{
    public bool Success { get; set; }
    public string UserQuery { get; set; } = "";
    public Paper? Paper { get; set; }
    public string Explanation { get; set; } = "";
    public string CitationSource { get; set; } = "";
    public string? Error { get; set; }
}

