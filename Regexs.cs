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

        // Regex to match any meta data
        // This includes: Version, Flags, Languages, Tags
        public const string VARIENT_META = @"(v[0-9.]+|r\d+)|(\([^\)]*\))|(\[[^\]]*\])|(\[\.\])|(\[\!\])";
        // Regex to remove brackets and . (dot) from above metadata match
        public const string SHORT_META = @"[\[\]\(\)]|\.0+";

        public const string YEAR = @"\((198\d|199\d|20[0-9]\d)\)";
    }
}
