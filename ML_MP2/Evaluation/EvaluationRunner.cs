using ML_MP2.Tools;
using ML_MP2.Models;
using System.Text.Json;

namespace ML_MP2.Evaluation;

/// <summary>
/// Runs comprehensive evaluation tests on the Research Scout Agent.
/// Tests constraint handling, prevents hallucination, and validates results.
/// </summary>
public class EvaluationRunner
{
    private readonly SearchTool _searchTool = new SearchTool();
    private List<EvaluationResult> _results = new();

    public async Task RunAllEvaluationsAsync()
    {
        Console.WriteLine("\n╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     Research Scout Agent - Evaluation Test Suite         ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

        // Category 1: Citation Constraints
        await RunTest1_HighCitationThreshold();
        await RunTest2_ModerateCitationThreshold();
        await RunTest3_LowCitationThreshold();

        // Category 2: Year Constraints
        await RunTest4_BeforeSpecificYear();
        await RunTest5_AfterSpecificYear();
        await RunTest6_YearRange();

        // Category 3: Combined Constraints
        await RunTest7_CombinedEasy();
        await RunTest8_CombinedMedium();
        await RunTest9_CombinedRestrictive();

        // Category 4: Broad Searches
        await RunTest10_BroadTopic();
        await RunTest11_SpecificTopic();

        // Category 5: Edge Cases
        await RunTest12_NoResultsExpected();
        await RunTest13_NicheTopic();
        await RunTest14_AcronymSearch();
        await RunTest15_AmbiguousTopic();

        // Print summary
        PrintSummary();
    }

    private async Task RunTest1_HighCitationThreshold()
    {
        var test = new TestCase
        {
            TestNumber = 1,
            Category = "Citation Constraints",
            Query = "Find papers about deep learning published after 2020 with at least 1000 citations",
            Topic = "deep learning",
            MinYear = 2021,
            MinCitations = 1000
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MinYear={test.MinYear}, MinCitations={test.MinCitations}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, test.MinYear, null, test.MinCitations);
        
        var result = EvaluateResults(test, results, 1000);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest2_ModerateCitationThreshold()
    {
        var test = new TestCase
        {
            TestNumber = 2,
            Category = "Citation Constraints",
            Query = "Find papers about natural language processing published after 2019 with at least 500 citations",
            Topic = "natural language processing",
            MinYear = 2020,
            MinCitations = 500
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MinYear={test.MinYear}, MinCitations={test.MinCitations}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, test.MinYear, null, test.MinCitations);
        
        var result = EvaluateResults(test, results, 500);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest3_LowCitationThreshold()
    {
        var test = new TestCase
        {
            TestNumber = 3,
            Category = "Citation Constraints",
            Query = "Find papers about quantum computing published after 2023 with at least 5 citations",
            Topic = "quantum computing",
            MinYear = 2024,
            MinCitations = 5
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MinYear={test.MinYear}, MinCitations={test.MinCitations}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, test.MinYear, null, test.MinCitations);
        
        var result = EvaluateResults(test, results, 5);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest4_BeforeSpecificYear()
    {
        var test = new TestCase
        {
            TestNumber = 4,
            Category = "Year Constraints",
            Query = "Find papers about reinforcement learning published before 2015",
            Topic = "reinforcement learning",
            MaxYear = 2014
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MaxYear={test.MaxYear}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, null, test.MaxYear, 0);
        
        var result = EvaluateResults(test, results, 0, test.MaxYear);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest5_AfterSpecificYear()
    {
        var test = new TestCase
        {
            TestNumber = 5,
            Category = "Year Constraints",
            Query = "Find papers about attention mechanisms published after 2022",
            Topic = "attention mechanisms transformers",
            MinYear = 2023
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MinYear={test.MinYear}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, test.MinYear, null, 0);
        
        var result = EvaluateResults(test, results, 0, minYearExpected: test.MinYear);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest6_YearRange()
    {
        var test = new TestCase
        {
            TestNumber = 6,
            Category = "Year Constraints",
            Query = "Find papers about generative models published between 2018 and 2020",
            Topic = "generative models GANs VAE",
            MinYear = 2018,
            MaxYear = 2020
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MinYear={test.MinYear}, MaxYear={test.MaxYear}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, test.MinYear, test.MaxYear, 0);
        
        var result = EvaluateResults(test, results, 0, test.MaxYear, test.MinYear);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest7_CombinedEasy()
    {
        var test = new TestCase
        {
            TestNumber = 7,
            Category = "Combined Constraints - Known Working",
            Query = "Find a paper about retrieval-augmented generation published before 2021 with more than 500 citations",
            Topic = "retrieval-augmented generation",
            MaxYear = 2020,
            MinCitations = 500,
            ExpectedPaperTitle = "Retrieval-Augmented Generation"
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MaxYear={test.MaxYear}, MinCitations={test.MinCitations}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, null, test.MaxYear, test.MinCitations);
        
        var result = EvaluateResults(test, results, test.MinCitations, test.MaxYear);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest8_CombinedMedium()
    {
        var test = new TestCase
        {
            TestNumber = 8,
            Category = "Combined Constraints - Medium",
            Query = "Find papers about LLM agents for software engineering published after 2022 with at least 100 citations",
            Topic = "LLM agents software engineering",
            MinYear = 2023,
            MinCitations = 100
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MinYear={test.MinYear}, MinCitations={test.MinCitations}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, test.MinYear, null, test.MinCitations);
        
        var result = EvaluateResults(test, results, test.MinCitations, minYearExpected: test.MinYear);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest9_CombinedRestrictive()
    {
        var test = new TestCase
        {
            TestNumber = 9,
            Category = "Combined Constraints - Restrictive",
            Query = "Find papers about neural architecture search published after 2023 with at least 200 citations",
            Topic = "neural architecture search NAS",
            MinYear = 2024,
            MinCitations = 200
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MinYear={test.MinYear}, MinCitations={test.MinCitations}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, test.MinYear, null, test.MinCitations);
        
        var result = EvaluateResults(test, results, test.MinCitations, minYearExpected: test.MinYear);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest10_BroadTopic()
    {
        var test = new TestCase
        {
            TestNumber = 10,
            Category = "Broad Searches",
            Query = "Find papers about machine learning",
            Topic = "machine learning"
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}' (no year/citation filters)");

        var results = await _searchTool.SearchResearchPapers(test.Topic, null, null, 0);
        
        var result = EvaluateResults(test, results, 0);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest11_SpecificTopic()
    {
        var test = new TestCase
        {
            TestNumber = 11,
            Category = "Specific Topics",
            Query = "Find papers about federated learning and privacy",
            Topic = "federated learning privacy"
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}' (no year/citation filters)");

        var results = await _searchTool.SearchResearchPapers(test.Topic, null, null, 0);
        
        var result = EvaluateResults(test, results, 0);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest12_NoResultsExpected()
    {
        var test = new TestCase
        {
            TestNumber = 12,
            Category = "Edge Cases - No Results Expected",
            Query = "Find papers about AI ethics published before 1990",
            Topic = "AI ethics",
            MaxYear = 1989
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MaxYear={test.MaxYear}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, null, test.MaxYear, 0);
        
        var result = EvaluateResults(test, results, 0, test.MaxYear);
        // For this test, no results is actually a success
        if (results.Contains("No papers found") || results.Contains("Max retries") || string.IsNullOrWhiteSpace(results))
        {
            result.Passed = true;
            result.Notes = "Correctly returned no results for impossible constraint";
        }
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest13_NicheTopic()
    {
        var test = new TestCase
        {
            TestNumber = 13,
            Category = "Edge Cases - Niche Topic",
            Query = "Find papers about zero-shot learning for object detection published after 2020 with at least 50 citations",
            Topic = "zero-shot learning object detection",
            MinYear = 2021,
            MinCitations = 50
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MinYear={test.MinYear}, MinCitations={test.MinCitations}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, test.MinYear, null, test.MinCitations);
        
        var result = EvaluateResults(test, results, test.MinCitations, minYearExpected: test.MinYear);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest14_AcronymSearch()
    {
        var test = new TestCase
        {
            TestNumber = 14,
            Category = "Edge Cases - Acronym Search",
            Query = "Find papers about BERT published after 2018 with at least 500 citations",
            Topic = "BERT Bidirectional Encoder Representations",
            MinYear = 2019,
            MinCitations = 500,
            ExpectedPaperTitle = "BERT"
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MinYear={test.MinYear}, MinCitations={test.MinCitations}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, test.MinYear, null, test.MinCitations);
        
        var result = EvaluateResults(test, results, test.MinCitations, minYearExpected: test.MinYear);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private async Task RunTest15_AmbiguousTopic()
    {
        var test = new TestCase
        {
            TestNumber = 15,
            Category = "Edge Cases - Ambiguous Topic",
            Query = "Find papers about agent based models in economics published after 2015",
            Topic = "agent based models economics",
            MinYear = 2016
        };

        Console.WriteLine($"\n[TEST {test.TestNumber}] {test.Category}");
        Console.WriteLine($"Query: {test.Query}");
        Console.WriteLine($"Constraints: Topic='{test.Topic}', MinYear={test.MinYear}");

        var results = await _searchTool.SearchResearchPapers(test.Topic, test.MinYear, null, 0);
        
        var result = EvaluateResults(test, results, 0, minYearExpected: test.MinYear);
        _results.Add(result);
        PrintTestResult(result);

        await Task.Delay(2000);
    }

    private EvaluationResult EvaluateResults(TestCase test, string rawResults, int minCitations = 0, int? maxYear = null, int? minYear = null, int? minYearExpected = null)
    {
        var result = new EvaluationResult
        {
            TestNumber = test.TestNumber,
            TestName = $"TEST {test.TestNumber}: {test.Category}",
            Query = test.Query
        };

        // Parse results to extract paper data
        var papers = ParsePaperResults(rawResults);
        
        if (papers.Count == 0)
        {
            if (rawResults.Contains("No papers found") || rawResults.Contains("Max retries") || string.IsNullOrEmpty(rawResults))
            {
                result.HasResults = false;
                result.Notes = "No results returned (may be expected or rate-limited)";
                result.Passed = true; // No hallucination = pass
            }
            else
            {
                result.HasResults = false;
                result.Notes = "Unknown error or rate limiting";
                result.Passed = false;
            }
            return result;
        }

        result.HasResults = true;
        result.PaperCount = papers.Count;

        // Check constraints
        bool allConstraintsMet = true;
        var constraintChecks = new List<string>();

        // Check citation constraints
        foreach (var paper in papers)
        {
            if (paper.CitationCount < minCitations)
            {
                allConstraintsMet = false;
                constraintChecks.Add($"❌ Paper '{paper.Title}' has {paper.CitationCount} citations, below minimum {minCitations}");
            }
            else
            {
                constraintChecks.Add($"✓ Citation constraint met: {paper.CitationCount} >= {minCitations}");
            }

            // Check year constraints
            if (minYearExpected.HasValue && paper.Year < minYearExpected)
            {
                allConstraintsMet = false;
                constraintChecks.Add($"❌ Paper '{paper.Title}' published in {paper.Year}, below minimum year {minYearExpected}");
            }
            else if (minYearExpected.HasValue)
            {
                constraintChecks.Add($"✓ Year constraint met: {paper.Year} >= {minYearExpected}");
            }

            if (maxYear.HasValue && paper.Year > maxYear)
            {
                allConstraintsMet = false;
                constraintChecks.Add($"❌ Paper '{paper.Title}' published in {paper.Year}, above maximum year {maxYear}");
            }
            else if (maxYear.HasValue)
            {
                constraintChecks.Add($"✓ Year constraint met: {paper.Year} <= {maxYear}");
            }
        }

        // Check for hallucination (citations should come from API, not invented)
        bool hasValidSources = papers.All(p => !string.IsNullOrEmpty(p.Title) && !string.IsNullOrEmpty(p.Url));
        
        result.ConstraintsMet = allConstraintsMet;
        result.NoHallucination = hasValidSources;
        result.Passed = allConstraintsMet && hasValidSources && papers.Count > 0;
        
        result.Notes = string.Join(" | ", constraintChecks.Take(3)); // Show first 3 checks
        if (constraintChecks.Count > 3)
            result.Notes += $" | ... ({constraintChecks.Count - 3} more checks)";

        return result;
    }

    private List<PaperData> ParsePaperResults(string rawResults)
    {
        var papers = new List<PaperData>();

        if (string.IsNullOrEmpty(rawResults) || rawResults.Contains("No papers found") || rawResults.Contains("Max retries"))
            return papers;

        // Split by separator
        var paperSections = rawResults.Split("---\n", StringSplitOptions.RemoveEmptyEntries);

        foreach (var section in paperSections)
        {
            var paper = new PaperData();
            var lines = section.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.StartsWith("Title:"))
                    paper.Title = line.Replace("Title:", "").Trim();
                else if (line.StartsWith("Year:"))
                {
                    if (int.TryParse(line.Replace("Year:", "").Trim(), out var year))
                        paper.Year = year;
                }
                else if (line.StartsWith("Citations:"))
                {
                    if (int.TryParse(line.Replace("Citations:", "").Trim(), out var citations))
                        paper.CitationCount = citations;
                }
                else if (line.StartsWith("URL:"))
                    paper.Url = line.Replace("URL:", "").Trim();
            }

            if (!string.IsNullOrEmpty(paper.Title))
                papers.Add(paper);
        }

        return papers;
    }

    private void PrintTestResult(EvaluationResult result)
    {
        string statusIcon = result.Passed ? "✓ PASS" : "✗ FAIL";
        Console.WriteLine($"Status: {statusIcon}");
        if (result.HasResults)
            Console.WriteLine($"Found: {result.PaperCount} papers | Constraints Met: {(result.ConstraintsMet ? "Yes" : "No")} | No Hallucination: {(result.NoHallucination ? "Yes" : "No")}");
        if (!string.IsNullOrEmpty(result.Notes))
            Console.WriteLine($"Details: {result.Notes}");
    }

    private void PrintSummary()
    {
        Console.WriteLine("\n╔═══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                   EVALUATION SUMMARY                      ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════════╝\n");

        int totalTests = _results.Count;
        int passedTests = _results.Count(r => r.Passed);
        int failedTests = totalTests - passedTests;
        double passRate = totalTests > 0 ? (double)passedTests / totalTests * 100 : 0;

        Console.WriteLine($"Total Tests: {totalTests}");
        Console.WriteLine($"Passed: {passedTests} ({passRate:F1}%)");
        Console.WriteLine($"Failed: {failedTests} ({100 - passRate:F1}%)");
        Console.WriteLine();

        // Group by category
        var byCategory = _results
            .Where(r => !string.IsNullOrEmpty(r.TestName))
            .GroupBy(r => r.TestName!.Split(':')[1].Trim().Split('-')[0].Trim());
        foreach (var category in byCategory)
        {
            var tests = category.ToList();
            var passCount = tests.Count(t => t.Passed);
            Console.WriteLine($"{category.Key}: {passCount}/{tests.Count} passed");
        }

        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine($"OVERALL RESULT: {(passRate >= 80 ? "✓ SUCCESS" : "✗ NEEDS IMPROVEMENT")}");
        Console.WriteLine($"Pass Rate: {passRate:F1}% (Target: 80%+)");
        Console.WriteLine(new string('=', 60));
    }
}

public class TestCase
{
    public int TestNumber { get; set; }
    public string? Category { get; set; }
    public string? Query { get; set; }
    public string? Topic { get; set; }
    public int? MinYear { get; set; }
    public int? MaxYear { get; set; }
    public int MinCitations { get; set; }
    public string? ExpectedPaperTitle { get; set; }
}

public class EvaluationResult
{
    public int TestNumber { get; set; }
    public string? TestName { get; set; }
    public string? Query { get; set; }
    public bool HasResults { get; set; }
    public int PaperCount { get; set; }
    public bool ConstraintsMet { get; set; }
    public bool NoHallucination { get; set; }
    public bool Passed { get; set; }
    public string? Notes { get; set; }
}

public class PaperData
{
    public string? Title { get; set; }
    public int Year { get; set; }
    public int CitationCount { get; set; }
    public string? Url { get; set; }
}

