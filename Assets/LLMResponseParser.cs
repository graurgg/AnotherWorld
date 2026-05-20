using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class LLMResponseParser
{
    private static readonly Regex KeyPattern =
        new Regex(@"<KEY\s+(\d+)>", RegexOptions.IgnoreCase | RegexOptions.Multiline);

    private const string WarningTag = "<WARNING>";

    public struct ParseResult
    {
        public string cleanedText;
        public int[]  detectedKeys;
        public bool   hasWarning;
    }

    public static ParseResult Parse(string raw)
    {
        var result = new ParseResult();

        if (string.IsNullOrEmpty(raw))
        {
            result.detectedKeys = new int[0];
            result.cleanedText  = string.Empty;
            return result;
        }

        // Warning check — tag must appear at the very start of the response
        string trimmed = raw.TrimStart();
        result.hasWarning = trimmed.StartsWith(WarningTag, System.StringComparison.OrdinalIgnoreCase);
        if (result.hasWarning)
            raw = trimmed.Substring(WarningTag.Length).TrimStart('\n', '\r', ' ');

        // Collect all key IDs present in the response
        var keys = new List<int>();
        foreach (Match m in KeyPattern.Matches(raw))
            if (int.TryParse(m.Groups[1].Value, out int id))
                keys.Add(id);

        result.detectedKeys = keys.ToArray();

        // Return the response with all key tags stripped
        result.cleanedText = KeyPattern.Replace(raw, "").Trim();

        return result;
    }
}
