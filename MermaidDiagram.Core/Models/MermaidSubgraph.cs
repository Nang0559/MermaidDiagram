using MermaidDiagram.Core.Models;

public sealed class MermaidSubgraph
{
    // ============================================================
    // IDENTITY
    // ============================================================

    public string Key
    {
        get;
    }

    public string Title
    {
        get;
    }

    // ============================================================
    // HIERARCHY
    // ============================================================

    public int Depth
    {
        get;
        set;
    }

    public string? ParentKey
    {
        get;
        set;
    }

    // ============================================================
    // MEMBERS
    // ============================================================

    public List<string> NodeKeys
    {
        get;
    }

    public List<string> ChildSubgraphKeys
    {
        get;
    }

    // ============================================================
    // STYLE
    // ============================================================

    public MermaidSubgraphStyle Style
    {
        get;
        set;
    }

    // ============================================================
    // CONSTRUCTOR
    // ============================================================

    public MermaidSubgraph(
        string key,
        string title)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(
                "Subgraph key cannot be empty.",
                nameof(key));
        }

        Key = key;

        Title =
            title ?? string.Empty;

        ParentKey =
            null;

        NodeKeys =
            new List<string>();

        ChildSubgraphKeys =
            new List<string>();

        Style =
            new MermaidSubgraphStyle();
    }
}