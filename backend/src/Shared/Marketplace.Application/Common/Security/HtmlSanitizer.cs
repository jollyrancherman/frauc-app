using System.Text.RegularExpressions;
using Ganss.Xss;

namespace Marketplace.Application.Common.Security;

public interface IHtmlSanitizer
{
    string Sanitize(string? input);
    bool ContainsHtml(string? input);
}

public class HtmlSanitizerService : IHtmlSanitizer
{
    private readonly HtmlSanitizer _sanitizer;
    private static readonly Regex HtmlTagRegex = new(@"<[^>]+>", RegexOptions.Compiled);
    private static readonly Regex DangerousPatternRegex = new(
        @"(javascript:|on\w+\s*=|<script|<iframe|<object|<embed|<applet|<meta|<link|<style)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public HtmlSanitizerService()
    {
        _sanitizer = new HtmlSanitizer();
        
        // Configure allowed tags for descriptions
        _sanitizer.AllowedTags.Clear();
        _sanitizer.AllowedTags.Add("p");
        _sanitizer.AllowedTags.Add("br");
        _sanitizer.AllowedTags.Add("strong");
        _sanitizer.AllowedTags.Add("em");
        _sanitizer.AllowedTags.Add("ul");
        _sanitizer.AllowedTags.Add("ol");
        _sanitizer.AllowedTags.Add("li");
        
        // Remove all attributes except specific safe ones
        _sanitizer.AllowedAttributes.Clear();
        _sanitizer.AllowedAttributes.Add("class");
        
        // Remove all schemes except http/https
        _sanitizer.AllowedSchemes.Clear();
        _sanitizer.AllowedSchemes.Add("http");
        _sanitizer.AllowedSchemes.Add("https");
    }

    public string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // First pass: Remove dangerous patterns
        if (DangerousPatternRegex.IsMatch(input))
        {
            input = DangerousPatternRegex.Replace(input, string.Empty);
        }

        // Second pass: Use HtmlSanitizer library
        return _sanitizer.Sanitize(input);
    }

    public bool ContainsHtml(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return HtmlTagRegex.IsMatch(input) || DangerousPatternRegex.IsMatch(input);
    }
}