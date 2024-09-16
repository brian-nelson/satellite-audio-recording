namespace SAR.Libraries.Fountain.Constants
{
    public static class FountainTemplates
    {
        public const string UNIVERSAL_LINE_BREAKS_TEMPLATE = "\n";

        public const string SCENE_HEADER_TEMPLATE = "\n<Scene Heading>$1</Scene Heading>";
        public const string ACTION_TEMPLATE = "<Action>$1</Action>$2";
        public const string MULTI_LINE_ACTION_TEMPLATE = "\n<Action>$2</Action>";
        public const string CHARACTER_CUE_TEMPLATE = "<Character>$1</Character>\n";
        public const string DIALOGUE_TEMPLATE = "$1<Dialogue>$4</Dialogue>";
        public const string PARENTHETICAL_TEMPLATE = "<Parenthetical>$2</Parenthetical>\n";
        public const string TRANSITION_TEMPLATE = "\n<Transition>$1</Transition>";
        public const string FORCED_TRANSITION_TEMPLATE = "\n<Transition>$1</Transition>";
        public const string FALSE_TRANSITION_TEMPLATE = "\n<Action>$1</Action>";
        public const string PAGE_BREAK_TEMPLATE = "\n<Page Break></Page Break>\n";
        public const string CLEANUP_TEMPLATE = @"";
        public const string FIRST_LINE_ACTION_TEMPLATE = "<Action>$1</Action>\n";
        public const string SECTION_HEADER_TEMPLATE = "<Section Heading>$1</Section Heading>";

        public const string BLOCK_COMMENT_TEMPLATE = "\n<Boneyard>$1</Boneyard>\n";
        public const string BRACKET_COMMENT_TEMPLATE = "\n<Comment>$1</Comment>\n";
        public const string SYNOPSIS_TEMPLATE = "\n<Synopsis>$1</Synopsis>\n";

        public const string NEWLINE_REPLACEMENT = @"@@@@@";
        public const string NEWLINE_RESTORE = "\n";

        public const string OPEN_PARENTHIESIS_REPLACEMENT = "OOOOO";
        public const string CLOSE_PARENTHIESIS_REPLACEMENT = "CCCCC";
    }
}
