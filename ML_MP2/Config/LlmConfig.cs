namespace ML_MP2.Config;

public static class LlmConfig
{
    public static string GetMistralApiKey()
    {
        var apiKey = Environment.GetEnvironmentVariable("MISTRAL_API_KEY");
        
        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("MISTRAL_API_KEY environment variable is not set.");
        }

        return apiKey;
    }

    public static string GetMistralModel()
    {
        return "open-mistral-nemo";
    }
}

