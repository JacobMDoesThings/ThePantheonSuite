namespace ThePantheonSuite.ZeusOrchestrator;

public static class Extensions
{
    public static string ReplacePlaceholder(this string input, string value, string key="{user}")
    {
        return input.Replace(key, value);
    }
}