using System.Text.RegularExpressions;

namespace ML_MP2.Agents;

/// <summary>
/// Simple constraint extractor that parses natural language queries without API calls.
/// Uses regex patterns to find year constraints, citation counts, and topics.
/// </summary>
public class SimpleConstraintExtractor
{
    /// <summary>
    /// Extracts search constraints from a natural language query using pattern matching.
    /// </summary>
    public static ConstraintExtraction Extract(string userQuery)
    {
        var constraints = new ConstraintExtraction();

        // Extract year constraints
        ExtractYearConstraints(userQuery, constraints);

        // Extract citation constraints
        ExtractCitationConstraints(userQuery, constraints);

        // Extract topic (everything else)
        ExtractTopic(userQuery, constraints);

        return constraints;
    }

    /// <summary>
    /// Extracts year constraints like "after 2022", "before 2021", "between 2018 and 2020"
    /// </summary>
    private static void ExtractYearConstraints(string query, ConstraintExtraction constraints)
    {
        // Match "after YYYY" or "published after YYYY"
        var afterMatch = Regex.Match(query, @"after\s+(\d{4})", RegexOptions.IgnoreCase);
        if (afterMatch.Success && int.TryParse(afterMatch.Groups[1].Value, out int afterYear))
        {
            constraints.MinYear = afterYear + 1;  // "after 2022" means 2023 or later
        }

        // Match "before YYYY" or "published before YYYY"
        var beforeMatch = Regex.Match(query, @"before\s+(\d{4})", RegexOptions.IgnoreCase);
        if (beforeMatch.Success && int.TryParse(beforeMatch.Groups[1].Value, out int beforeYear))
        {
            constraints.MaxYear = beforeYear - 1;  // "before 2021" means 2020 or earlier
        }

        // Match "between YYYY and YYYY"
        var betweenMatch = Regex.Match(query, @"between\s+(\d{4})\s+and\s+(\d{4})", RegexOptions.IgnoreCase);
        if (betweenMatch.Success)
        {
            if (int.TryParse(betweenMatch.Groups[1].Value, out int startYear))
                constraints.MinYear = startYear;
            if (int.TryParse(betweenMatch.Groups[2].Value, out int endYear))
                constraints.MaxYear = endYear;
        }

        // Match "in YYYY" (exact year)
        var inMatch = Regex.Match(query, @"in\s+(\d{4})", RegexOptions.IgnoreCase);
        if (inMatch.Success && int.TryParse(inMatch.Groups[1].Value, out int exactYear))
        {
            constraints.MinYear = exactYear;
            constraints.MaxYear = exactYear;
        }

        // Match "recent" or "recent years" -> last 2 years
        if (query.Contains("recent", StringComparison.OrdinalIgnoreCase))
        {
            constraints.MinYear = DateTime.Now.Year - 1;
        }
    }

    /// <summary>
    /// Extracts citation constraints like "100+ citations", "with at least 500 citations"
    /// </summary>
    private static void ExtractCitationConstraints(string userQuery, ConstraintExtraction constraints)
    {
        // Match "at least NNNN citations"
        var atLeastMatch = Regex.Match(userQuery, @"at\s+least\s+(\d+)\s+citations?", RegexOptions.IgnoreCase);
        if (atLeastMatch.Success && int.TryParse(atLeastMatch.Groups[1].Value, out int citations))
        {
            constraints.MinCitations = citations;
            return;
        }

        // Match "NNNN+ citations" or "with NNNN citations"
        var numMatch = Regex.Match(userQuery, @"(?:with\s+)?(\d+)\+?\s+citations?", RegexOptions.IgnoreCase);
        if (numMatch.Success && int.TryParse(numMatch.Groups[1].Value, out int cites))
        {
            constraints.MinCitations = cites;
        }

        // Match "more than NNNN citations"
        var moreMatch = Regex.Match(userQuery, @"more\s+than\s+(\d+)\s+citations?", RegexOptions.IgnoreCase);
        if (moreMatch.Success && int.TryParse(moreMatch.Groups[1].Value, out int moreCites))
        {
            constraints.MinCitations = moreCites + 1;
        }
    }

    /// <summary>
    /// Extracts the topic by removing year/citation constraints and keeping key terms
    /// </summary>
    private static void ExtractTopic(string userQuery, ConstraintExtraction constraints)
    {
        // Remove common phrases to isolate the topic
        var topic = userQuery;

        // Remove year-related phrases
        topic = Regex.Replace(topic, @"published\s+(after|before|between)\s+\d{4}.*?(?=\.|$)", "", RegexOptions.IgnoreCase);
        topic = Regex.Replace(topic, @"(after|before|between)\s+\d{4}.*?(?=and|\s+with|\.)", "", RegexOptions.IgnoreCase);
        topic = Regex.Replace(topic, @"in\s+\d{4}", "", RegexOptions.IgnoreCase);

        // Remove citation-related phrases
        topic = Regex.Replace(topic, @"with\s+(?:at\s+least\s+)?\d+\+?\s+citations?", "", RegexOptions.IgnoreCase);
        topic = Regex.Replace(topic, @"(?:at\s+least|more\s+than)\s+\d+\s+citations?", "", RegexOptions.IgnoreCase);

        // Remove common query starters
        topic = Regex.Replace(topic, @"^(?:Find|Search|Look for|Get)\s+(?:a\s+)?(?:research\s+)?papers?\s+(?:about|on|for)\s+", "", RegexOptions.IgnoreCase);
        topic = Regex.Replace(topic, @"^(?:and|with)\s+", "", RegexOptions.IgnoreCase);

        // Remove trailing phrases like "Explain...", "Summarize...", etc.
        topic = Regex.Replace(topic, @"\s*(?:Explain|Summarize|Show|Discuss|Provide).*$", "", RegexOptions.IgnoreCase);

        // Clean up whitespace
        topic = Regex.Replace(topic, @"\s+", " ").Trim();
        
        // Remove trailing punctuation and common words
        topic = topic.TrimEnd('.', ',', ';', '?');
        topic = Regex.Replace(topic, @"\s+(and|or|with)$", "", RegexOptions.IgnoreCase);

        if (!string.IsNullOrWhiteSpace(topic))
        {
            constraints.Topic = topic;
        }
    }
}

