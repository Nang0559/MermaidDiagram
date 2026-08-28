using System.Text.RegularExpressions;

namespace MermaidDiagram.Core.Parsing
{
    public static class MermaidRegex
    {
        // ============================================================
        // SUBGRAPH
        // ============================================================

        public static readonly Regex SubgraphRegex =
            new Regex(
                @"^\s*subgraph\s+" +
                @"(?:" +
                    @"(?<id>[A-Za-z_][A-Za-z0-9_-]*)\s*" +
                    @"\[\s*""(?<title>.*?)""\s*\]" +
                "|" +
                    @"(?<plain>.+?)" +
                @")\s*$",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase);

        // ============================================================
        // NODE
        // ============================================================

        public static readonly Regex NodeRegex =
            new Regex(
                @"(?<id>[A-Za-z_][A-Za-z0-9_-]*)" +
                @"(?:" +
                    @"\(\(""(?<circle>.*?)""\)\)" +
                    "|" +
                    @"\{""(?<decision>.*?)""\}" +
                    "|" +
                    @"\[""(?<rect>.*?)""\]" +
                    "|" +
                    @"\(""(?<round>.*?)""\)" +
                    "|" +
                    @"\((?<roundRaw>.*?)\)" +
                    "|" +
                    @"\[(?<rectRaw>.*?)\]" +
                    "|" +
                    @"\{(?<decisionRaw>.*?)\}" +
                ")",
                RegexOptions.Compiled |
                RegexOptions.Singleline);

        // ============================================================
        // EDGE
        // ============================================================

        public static readonly Regex EdgeRegex =
     new Regex(
         @"(?<src>[A-Za-z_][A-Za-z0-9_-]*)\s*" +
         @"(?<op>-.->|==>|-->|---|-.-)\s*" +
         @"(?:\|(?<label>.*?)\|\s*)?" +
         @"(?<dst>[A-Za-z_][A-Za-z0-9_-]*)",
         RegexOptions.Compiled |
         RegexOptions.Singleline);

        // ============================================================
        // STYLE
        // ============================================================

        public static readonly Regex StyleRegex =
            new Regex(
                @"^\s*style\s+" +
                @"(?<id>[A-Za-z_][A-Za-z0-9_-]*)\s+" +
                @"(?<props>.+?)\s*$",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase);

        // ============================================================
        // CLASS DEF
        // ============================================================

        public static readonly Regex ClassDefRegex =
            new Regex(
                @"^\s*classDef\s+" +
                @"(?<name>[A-Za-z_][A-Za-z0-9_-]*)\s+" +
                @"(?<props>.+?)\s*$",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase);

        // ============================================================
        // CLASS APPLY
        // ============================================================

        public static readonly Regex ClassApplyRegex =
            new Regex(
                @"^\s*class\s+" +
                @"(?<ids>[A-Za-z0-9_,\-\s]+)\s+" +
                @"(?<name>[A-Za-z_][A-Za-z0-9_-]*)\s*$",
                RegexOptions.Compiled |
                RegexOptions.IgnoreCase);

        // ============================================================
        // FORM NAME
        // ============================================================

        public static readonly Regex FormNameRegex =
            new Regex(
                @"\bForm[A-Za-z0-9_]+\b",
                RegexOptions.Compiled);
    }
}