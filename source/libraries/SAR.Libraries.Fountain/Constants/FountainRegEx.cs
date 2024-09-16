namespace SAR.Libraries.Fountain.Constants
{
    public static class FountainRegEx
    {
        public const string UNIVERSAL_LINE_BREAKS_PATTERN = @"\r\n|\r|\n";

        public const string SCENE_HEADER_PATTERN = @"(?<=\n)(([iI][nN][tT]|[eE][xX][tT]|[^\w][eE][sS][tT]|\.[^\.\n]+|[iI]\.?\/[eE]\.?)([^\n]+))\n";
        public const string ACTION_PATTERN = @"([^<>]*?)(\n{2}|\n<)";
        public const string MULTI_LINE_ACTION_PATTERN = @"\n{2}(([^a-z\n:]+?[\.\?,\s!\*_]*?)\n{2}){1,2}";
        public const string CHARACTER_CUE_PATTERN = @"(?<=\n)([ \t]*[^<>a-z\s\/\n][^<>a-z:!\?\n]*[^<>a-z\(!\?:,\n\.][ \t]?)\n{1}(?!\n)";
        public const string DIALOGUE_PATTERN = @"(<(Character|Parenthetical)>[^<>\n]+<\/(Character|Parenthetical)>)([^<>]*?)(?=\n{2}|\n{1}<Parenthetical>)";
        public const string PARENTHETICAL_PATTERN = @"(\(([^<>]*?)\)[\s]?)";
        public const string TRANSITION_PATTERN = @"\n([\*_]*([^<>\na-z]*TO:|FADE TO BLACK\.|FADE OUT\.|CUT TO BLACK\.)[\*_]*)\n";
        public const string FORCED_TRANSITION_PATTERN = @"\n((&gt;|>)\s*[^<>\n]+)\n";
        public const string FALSE_TRANSITION_PATTERN = @"\n((&gt;|>)\s*[^<>\n]+(&lt;\s*))\n";
        public const string PAGE_BREAK_PATTERN = @"(?<=\n)(\s*[\=\-_]{3,8}\s*)\n{1}";
        public const string CLEANUP_PATTERN = @"<Action>\s*<\/Action>";
        public const string FIRST_LINE_ACTION_PATTERN = @"^\n\n([^<>\n#]*?)\n";
        
        public const string SECTION_HEADER_PATTERN = @"^((#+)(\s*[^\n<>]*))\n?";

        public const string BLOCK_COMMENT_PATTERN = @"\n\/\*([^<>]+?)\*\/\n";
        public const string BRACKET_COMMENT_PATTERN = @"\n\[{2}([^<>]+?)\]{2}\n";
        public const string SYNOPSIS_PATTERN = @"\n={1}([^<>=][^<>]+?)\n";

        public const string TAG_MATCHES = @"<(?<name>[a-zA-Z\s]+)>(?<value>[^<>]*)<\/[a-zA-Z\s]+>";

        public const string CHARACTER_TAG_MATCH = @"<Character>(?<value>[^<>]*)<\/Character>";
        public const string SCENE_HEADING_TAG_MATCH = @"<Scene Heading>(?<value>[^<>]*)<\/Scene Heading>";

        public const string TITLE_PAGE_PATTERN = @"^([^\n]+:(([ \t]*|\n)[^\n]+\n)+)+\n";
        public const string INLINE_DIRECTIVE_PATTERN = @"^([\w\s&]+):\s*([^\s][\w&,\.\?!:\(\)\/\s\-©\*_]+)$";
        public const string MULTI_LINE_DIRECTIVE_PATTERN = @"^([\w\s&]+):\s*$";
        public const string MULTI_LINE_DATA_PATTERN = @"^([ ]{2,8}|\t)([^<>]+)$";

        public const string DUAL_DIALOGUE_PATTERN = @"";
        public const string CENTERED_TEXT_PATTERN = @"";

        
    }
}
