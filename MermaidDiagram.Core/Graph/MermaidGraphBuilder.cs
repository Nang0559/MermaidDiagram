
using System;
using MermaidDiagram.Core.Models;

namespace MermaidDiagram.Core.Graph
{
    /// <summary>
    /// Chuyển MermaidDocument thành MermaidGraph.
    ///
    /// Builder chỉ chịu trách nhiệm xây dựng cấu trúc graph:
    ///
    ///     MermaidDocument
    ///          ↓
    ///     MermaidGraph
    ///
    /// Bao gồm:
    ///     - NodeKeys
    ///     - Adjacency
    ///     - ReverseAdjacency
    ///
    /// Không chịu trách nhiệm:
    ///     - tìm root
    ///     - tính layer
    ///     - tìm main path
    ///     - phân tích branch
    ///     - layout
    ///     - DevExpress
    ///     - WinForms
    /// </summary>
    public sealed class MermaidGraphBuilder
    {
        // ============================================================
        // PUBLIC API
        // ============================================================

        /// <summary>
        /// Build graph từ Mermaid document.
        /// </summary>
        public MermaidGraph Build(
            MermaidDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(
                    nameof(document));
            }

            MermaidGraph graph =
                new MermaidGraph();

            AddNodes(
                graph,
                document);

            AddEdges(
                graph,
                document);

            return graph;
        }

        // ============================================================
        // NODES
        // ============================================================

        /// <summary>
        /// Thêm toàn bộ node từ MermaidDocument
        /// vào MermaidGraph.
        /// </summary>
        private static void AddNodes(
            MermaidGraph graph,
            MermaidDocument document)
        {
            foreach (MermaidNode node
                     in document.Nodes)
            {
                if (node == null)
                {
                    throw new InvalidOperationException(
                        "MermaidDocument chứa node null.");
                }

                string nodeId =
                    node.Id?.Trim()
                    ?? string.Empty;

                if (nodeId.Length == 0)
                {
                    throw new InvalidOperationException(
                        "MermaidDocument chứa node " +
                        "không có Id.");
                }

                graph.AddNode(
                    nodeId);
            }
        }

        // ============================================================
        // EDGES
        // ============================================================

        /// <summary>
        /// Thêm toàn bộ edge từ MermaidDocument
        /// vào MermaidGraph.
        ///
        /// Mermaid:
        ///
        ///     Source --> Target
        ///
        /// Graph:
        ///
        ///     AddEdge(Source, Target)
        /// </summary>
        private static void AddEdges(
            MermaidGraph graph,
            MermaidDocument document)
        {
            foreach (MermaidEdge edge
                     in document.Edges)
            {
                if (edge == null)
                {
                    throw new InvalidOperationException(
                        "MermaidDocument chứa edge null.");
                }

                string source =
                    edge.Source?.Trim()
                    ?? string.Empty;

                string target =
                    edge.Target?.Trim()
                    ?? string.Empty;

                if (source.Length == 0)
                {
                    throw new InvalidOperationException(
                        "Mermaid edge có Source rỗng.");
                }

                if (target.Length == 0)
                {
                    throw new InvalidOperationException(
                        "Mermaid edge có Target rỗng.");
                }

                ValidateNodeExists(
                    graph,
                    source,
                    "Source");

                ValidateNodeExists(
                    graph,
                    target,
                    "Target");

                graph.AddEdge(
                    source,
                    target);
            }
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Bảo đảm endpoint của edge đã tồn tại
        /// trong graph.
        /// </summary>
        private static void ValidateNodeExists(
            MermaidGraph graph,
            string nodeId,
            string endpointName)
        {
            if (!graph.ContainsNode(
                    nodeId))
            {
                throw new InvalidOperationException(
                    $"Edge {endpointName} '{nodeId}' " +
                    "không tồn tại trong MermaidGraph.");
            }
        }
    }
}

