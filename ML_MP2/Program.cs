using ML_MP2.Agents;
using ML_MP2.Evaluation;
using DotNetEnv;

// Load variables from the .env file
Env.Load();

// Verify API key is available
string? apiKey = Environment.GetEnvironmentVariable("MISTRAL_API_KEY");

if (string.IsNullOrEmpty(apiKey) || apiKey == "your-mistral-api-key-here")
{
    Console.WriteLine("❌ ERROR: MISTRAL_API_KEY not set in .env file");
    Console.WriteLine("Please add your Mistral API key to ML_MP2/.env");
    return;
}

Console.WriteLine("✓ API Key loaded\n");

// Menu to choose between agent and evaluation
Console.WriteLine("╔════════════════════════════════════════════════════════╗");
Console.WriteLine("║        Research Scout Agent - Main Menu               ║");
Console.WriteLine("╠════════════════════════════════════════════════════════╣");
Console.WriteLine("║ 1. Run Agent (Interactive demo with 3 test queries)  ║");
Console.WriteLine("║ 2. Run Evaluation Suite (15 comprehensive tests)     ║");
Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

Console.Write("Choose an option (1 or 2): ");
string? choice = Console.ReadLine();

if (choice == "1")
{
    Console.WriteLine("\nStarting Research Scout Agent Demo...\n");
    await AgentSetup.RunResearchScoutAsync();
}
else if (choice == "2")
{
    Console.WriteLine("\nStarting Evaluation Suite...");
    var evaluator = new EvaluationRunner();
    await evaluator.RunAllEvaluationsAsync();
}
else
{
    Console.WriteLine("Invalid choice. Exiting.");
    return;
}

// Only try to read key if console input is available
if (Console.IsInputRedirected)
{
    Console.WriteLine("\nProgram completed.");
}
else
{
    Console.WriteLine("\nPress any key to exit...");
    Console.ReadKey();
}

