using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SAR.Libraries.Fountain.Constants;
using SAR.Libraries.Fountain.Objects;

namespace SAR.Libraries.Fountain.Parser
{
    /// <summary>
    /// Based on https://github.com/nyousefi/Fountain
    /// </summary>
    public static class FountainParser
    {

        public static List<Element> Parse(string script)
        {
            var output = new List<Element>();

            var normalizedLineEndings = Regex.Replace(
                script,
                FountainRegEx.UNIVERSAL_LINE_BREAKS_PATTERN,
                FountainTemplates.UNIVERSAL_LINE_BREAKS_TEMPLATE);

            //Process the title element
            var title = GetTitle(normalizedLineEndings);
            var titleElement = ProcessTitle(title);
            if (titleElement != null)
            {
                output.Add(titleElement);
            }
            
            //Process the remaining elements
            var body = GetBody(normalizedLineEndings);
            output.AddRange(ProcessBody(body));

            return output;
        }

        private static TitleElement ProcessTitle(string title)
        {
            if (!string.IsNullOrWhiteSpace(title))
            {
                TitleElement titleElement = new TitleElement(title);
                return titleElement;
            }

            return null;
        }

        private static IEnumerable<Element> ProcessBody(string body)
        {
            var output = new List<Element>();
            var working = body;

            // 1st pass - Block comments
            // The regexes aren't smart enough (yet) to deal with newlines in the
            // comments, so we need to convert them before processing.
            var blockCommentMatches = Regex.Matches(working, FountainRegEx.BLOCK_COMMENT_PATTERN);
            foreach (Match blockCommentMatch in blockCommentMatches)
            {
                var original = blockCommentMatch.Value;
                var newValue = original.Replace("\n", FountainTemplates.NEWLINE_REPLACEMENT);

                working = working.Replace(original, newValue);
            }

            var bracketCommentMatches = Regex.Matches(working, FountainRegEx.BRACKET_COMMENT_PATTERN);
            foreach (Match bracketCommentMatch in bracketCommentMatches)
            {
                var original = bracketCommentMatch.Value;
                var newValue = original.Replace("\n", FountainTemplates.NEWLINE_REPLACEMENT);

                working = working.Replace(original, newValue);
            }

            // Sanitize < and > chars for conversion to the markup
            working = working.Replace("&lt;", "<");
            working = working.Replace("&gt;", ">");

            working = Regex.Replace(working,
                FountainRegEx.BLOCK_COMMENT_PATTERN,
                FountainTemplates.BLOCK_COMMENT_TEMPLATE);

            working = Regex.Replace(working,
                FountainRegEx.BRACKET_COMMENT_PATTERN,
                FountainTemplates.BLOCK_COMMENT_TEMPLATE);

            working = Regex.Replace(working,
                FountainRegEx.SYNOPSIS_PATTERN,
                FountainTemplates.SYNOPSIS_TEMPLATE);

            working = Regex.Replace(working,
                FountainRegEx.PAGE_BREAK_PATTERN,
                FountainTemplates.PAGE_BREAK_TEMPLATE);

            working = Regex.Replace(working,
                FountainRegEx.FALSE_TRANSITION_PATTERN,
                FountainTemplates.FALSE_TRANSITION_TEMPLATE);

            working = Regex.Replace(working,
                FountainRegEx.FORCED_TRANSITION_PATTERN,
                FountainTemplates.FORCED_TRANSITION_TEMPLATE);

            working = Regex.Replace(working,
                FountainRegEx.SCENE_HEADER_PATTERN,
                FountainTemplates.SCENE_HEADER_TEMPLATE);

            var matches = Regex.Matches(working, FountainRegEx.SCENE_HEADING_TAG_MATCH);
            foreach (Match match in matches)
            {
                var oldValue = match.Value;
                var newValue = oldValue
                    .Replace("(", FountainTemplates.OPEN_PARENTHIESIS_REPLACEMENT)
                    .Replace(")", FountainTemplates.CLOSE_PARENTHIESIS_REPLACEMENT);

                working = working.Replace(oldValue, newValue);
            }

            working = Regex.Replace(working,
                FountainRegEx.FIRST_LINE_ACTION_PATTERN,
                FountainTemplates.FIRST_LINE_ACTION_TEMPLATE);

            working = Regex.Replace(working,
                FountainRegEx.TRANSITION_PATTERN,
                FountainTemplates.TRANSITION_TEMPLATE);

            working = Regex.Replace(working,
                FountainRegEx.CHARACTER_CUE_PATTERN,
                FountainTemplates.CHARACTER_CUE_TEMPLATE);

            matches = Regex.Matches(working, FountainRegEx.CHARACTER_TAG_MATCH);
            foreach (Match match in matches)
            {
                var oldValue = match.Value;
                var newValue = oldValue
                    .Replace("(", FountainTemplates.OPEN_PARENTHIESIS_REPLACEMENT)
                    .Replace(")", FountainTemplates.CLOSE_PARENTHIESIS_REPLACEMENT);

                working = working.Replace(oldValue, newValue);
            }

            working = Regex.Replace(working,
                FountainRegEx.PARENTHETICAL_PATTERN,
                FountainTemplates.PARENTHETICAL_TEMPLATE);

            working = Regex.Replace(working,
                FountainRegEx.DIALOGUE_PATTERN,
                FountainTemplates.DIALOGUE_TEMPLATE);

            working = Regex.Replace(working,
                FountainRegEx.SECTION_HEADER_PATTERN,
                FountainTemplates.SECTION_HEADER_TEMPLATE);

            working = Regex.Replace(working,
                FountainRegEx.ACTION_PATTERN,
                FountainTemplates.ACTION_TEMPLATE);

            working = Regex.Replace(working,
                FountainRegEx.CLEANUP_PATTERN,
                FountainTemplates.CLEANUP_TEMPLATE);

            working = working.Replace(FountainTemplates.OPEN_PARENTHIESIS_REPLACEMENT, "(");
            working = working.Replace(FountainTemplates.CLOSE_PARENTHIESIS_REPLACEMENT, ")");

            matches = Regex.Matches(working, FountainRegEx.TAG_MATCHES);

            foreach (Match match in matches)
            {
                string name = match.Groups["name"].Value;
                string value = match.Groups["value"].Value;

                Element element = null;

                switch (name)
                {
                    case "Scene Heading":
                        element = new SceneElement(value);
                        break;
                    case "Action":
                        element = new ActionElement(value);
                        break;
                    case "Character":
                        element = new CharacterElement(value);
                        break;
                    case "Dialogue":
                        element = new DialogueElement(value);
                        break;
                    case "Parenthetical":
                        element = new ParentheticalElement(value);
                        break;
                    case "Transition":
                        element = new TransitionElement(value);
                        break;
                    case "Page Break":
                        element = new PageBreakElement(value);
                        break;
                    case "Section Heading":
                        element = new SectionHeadingElement(value);
                        break;
                    case "Boneyard":
                        element = new BoneyardElement(value);
                        break;
                    case "Comment":
                        element = new CommentElement(value);
                        break;
                    case "Synopsis":
                        element = new SynopsisElement(value);
                        break;
                    default:
                        throw new NotSupportedException($"Unknown type - {name}");
                }

                output.Add(element);
            }

            return output;
        }

        private static string GetBody(string script)
        {
            int position = script.IndexOf("\n\n");

            if (position >= 0)
            {
                var output = script.Substring(position + 2);
                return output;
            }

            return script;
        }

        private static string GetTitle(string script)
        {
            int position = script.IndexOf("\n\n");

            if (position >= 0)
            {
                var title = script.Substring(0, position + 2);

                var match = Regex.Match(title, FountainRegEx.TITLE_PAGE_PATTERN);

                if (match.Groups.Count>0)
                {
                    return title;
                }
            }

            return "";
        }
    }
}
