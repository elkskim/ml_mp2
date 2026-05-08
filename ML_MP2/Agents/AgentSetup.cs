namespace ML_MP2.Agents;

/// <summary>
/// Research Scout Agent setup using MistralResearchAgent
/// Orchestrates the agent to handle natural language queries and return recommendations
/// </summary>
public static class AgentSetup
{
    public static async Task RunResearchScoutAsync()
    {
        // Initialize the Mistral-powered agent
        var agent = new MistralResearchAgent();
        
        Console.WriteLine("╔════════════════════════════════════════════════════╗");
        Console.WriteLine("║    Research Scout Agent - Powered by Mistral AI   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

        // Test queries from the assignment
        var testQueries = new[]
        {
            "Find a research paper about LLM agents for software engineering that was published after 2022 and has at least 100 citations. Explain why the paper is relevant and provide the source of the citation count.",
            "Find a paper about retrieval-augmented generation published before 2021 with more than 500 citations. Summarize its contribution in 5-7 sentences.",
            "Find a recent paper about AI agents using tools and explain whether it would be useful for someone building autonomous software agents."
        };

        foreach (var userQuery in testQueries)
        {
            Console.WriteLine("\n" + new string('=', 70));
            Console.WriteLine("USER QUERY:");
            Console.WriteLine(userQuery);
            Console.WriteLine(new string('=', 70) + "\n");

            try
            {
                var recommendation = await agent.ProcessQueryAsync(userQuery);

                if (recommendation.Success && recommendation.Paper != null)
                {
                    Console.WriteLine("\n[✓ RECOMMENDATION FOUND]");
                    Console.WriteLine($"Title: {recommendation.Paper.Title}");
                    Console.WriteLine($"Authors: {recommendation.Paper.Authors}");
                    Console.WriteLine($"Year: {recommendation.Paper.Year}");
                    Console.WriteLine($"Citations: {recommendation.Paper.CitationCount} (source: {recommendation.CitationSource})");
                    Console.WriteLine($"URL: {recommendation.Paper.Url}");
                    Console.WriteLine($"\nExplanation:\n{recommendation.Explanation}");
                }
                else
                {
                    Console.WriteLine($"\n[✗ ERROR] {recommendation.Error ?? "Unknown error"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[✗ EXCEPTION] {ex.Message}");
                Console.WriteLine($"Stack: {ex.StackTrace}");
            }

            // Rate limiting between queries
            Console.WriteLine("\n[Waiting 2 seconds before next query...]");
            await Task.Delay(2000);
        }

        Console.WriteLine("\n" + new string('=', 70));
        Console.WriteLine("✓ Agent demo complete");
        Console.WriteLine(new string('=', 70));
    }
}

