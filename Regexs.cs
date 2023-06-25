using System;
using System.Runtime.CompilerServices;

namespace TDC_Extractor
{	public static class Regexs
	{
        // Each of the below are Regular Expressions to match the corressponding tag/flag/etc...

        // ***TAGS***

        // [DC] DOS Conversion"
        public const string DC = @"\[DC\]";
        // [h1, h2, ...] Hacked version 1, 2, etc
        public const string HN = @"\[h\d+\]";
        // [a1, a2, ...] Alternate version 1, 2, etc
        public const string AN = @"\[a\d+\]";
        // [f1, f2, ...] Fixed version 1, 2, etc
        public const string FN = @"\[f\d+\]";
        // [o1, o2, ...] Overdump version 1, 2, etc
        public const string ON = @"\[o\d+\]";
        // [b1, b2, ...] Bad version 1, 2, etc
        public const string BN = @"\[b\d+\]";
        // [SW] Shareware
        public const string SW = @"(\[SW\])";
        // [SW] Shareware, [SWR] Registered Shareware, [FW] Freeware
        public const string SWR_FW = @"(\[SWR\])|(\[FW\])";
        // [!] Known good dump
        public const string GOOD = @"\[\!\]";
        // [.] Mostly good dump.
        public const string MOSTLY_GOOD = @"\[\.\]";
        // [tr xx] Translated into any language
        public const string TRANSLATED = @"\[tr\s\w{2}\]";
        // [tr xx] Translated into other language
        //public const string OTHER = @"\[tr\sXX\]";

        // ***VERSION***
        // Matches v1.x and/or r1, etc
        public const string VERSION = @"[vV]\d+\.?\d+[a-zA-Z]?|[rR]\d+[a-zA-Z]?";

        // ***FLAGS***
        // Installer
        public const string INSTALLER = @"\(Installer\)";
        // Demo
        public const string DEMO = @"\(Demo\)";
        // CGA
        public const string CGA = @"\(CGA\)";
        // PCjr
        public const string PCJR = @"\(PCjr\)";
        // Tandy
        public const string TANDY = @"\(Tandy\)";
        // EGA
        public const string EGA = @"\(EGA\)";
        // VGA
        public const string VGA = @"\(VGA\)";
        // Custom Flag
        public const string CUSTOM_FLAG = @"\([a-zA-Z\s\-\,\.]*(?i)custom[a-zA-Z\s\-\,\.]*(?-i)\)";

        // ***LANGUAGES***
        // English
        public const string ENGLISH = @"\(En\)";
        // Japenese
        //public const string JAPANESE = @"\(Jp\)";
        // TO DO: Other...
        // BUG: Matches too many other things!
        //public const string OTHER = @"\[\w{2}\]";

        // ***PUBLISHER***
        // Matches publisher within round brackets
        public const string PUBLISHER = @"\([a-zA-Z\s\-\,\.]*(?i)publisher[a-zA-Z\s\-\,\.]*(?-i)\)";

        // ***GENRE***
        // Matches genre within square brackets
        public const string GENRE = @"\[[a-zA-Z\s\-,'.()]*genre[a-zA-Z\s\-,'.()]*\]";

        // ***OTHER***
        // These are punctuation chars we do not want in the final game title IF it is shortened, either by truncation or suggestion
        public const string EXCLUDED_SYMBOLS = @"[\s_\-'.?!#&()\[\]$,]";
        // Game title with no meta data
        public const string NAME_W_O_META = @"^[^[\]()]*?(?=\s*(?:\[|\(|v\d+(?:\.\d+)*|r\d+))";
        // Regex to match the trucated number at the end of a game name
        public const string TRUNCATED_NUMBER = @"~(\d+)$";
    }
}
