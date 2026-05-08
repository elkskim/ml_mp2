using System.Text.Json.Serialization;

namespace ML_MP2.Models;

public class SemanticScholarSearchResponse
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("offset")]
    public int Offset { get; set; }

    [JsonPropertyName("data")]
    public List<Paper>? Data { get; set; }
}

public class Paper
{
    [JsonPropertyName("paperId")]
    public string? PaperId { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("year")]
    public int? Year { get; set; }

    [JsonPropertyName("citationCount")]
    public int? CitationCount { get; set; }

    [JsonPropertyName("abstract")]
    public string? Abstract { get; set; }
    
    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("authors")]
    public List<Author>? Authors { get; set; }
}

public class Author
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("authorId")]
    public string? AuthorId { get; set; }
}
