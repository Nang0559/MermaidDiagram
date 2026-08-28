using System;
using System.Collections.Generic;
using MermaidDiagram.Abstractions.Workflow.Actions;

namespace MermaidDiagram.Core.Workflow.Actions
{
    /// <summary>
    /// Factory tạo Workflow Action theo ActionKey.
    ///
    /// Factory không biết business logic cụ thể.
    ///
    /// Trách nhiệm:
    ///     - Register Action factory.
    ///     - Resolve Action theo ActionKey.
    ///     - Đảm bảo Action được tạo đúng contract.
    ///
    /// Không phụ thuộc:
    ///     - DevExpress
    ///     - WinForms
    ///     - MAUI
    ///     - Business Service cụ thể
    /// </summary>
    public sealed class WorkflowActionFactory
        : IWorkflowActionFactory
    {
        // ============================================================
        // REGISTRY
        // ============================================================

        private readonly Dictionary<
            string,
            Func<IWorkflowAction>>
            _factories =
                new Dictionary<
                    string,
                    Func<IWorkflowAction>>(
                        StringComparer.OrdinalIgnoreCase);

        // ============================================================
        // REGISTER
        // ============================================================

        /// <summary>
        /// Đăng ký một Action factory.
        ///
        /// Mỗi ActionKey chỉ được đăng ký một lần.
        /// </summary>
        public void Register(
            string actionKey,
            Func<IWorkflowAction> factory)
        {
            if (string.IsNullOrWhiteSpace(actionKey))
            {
                throw new ArgumentException(
                    "ActionKey không được rỗng.",
                    nameof(actionKey));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(
                    nameof(factory));
            }

            if (_factories.ContainsKey(actionKey))
            {
                throw new InvalidOperationException(
                    $"WorkflowAction '{actionKey}' đã được đăng ký.");
            }

            _factories.Add(
                actionKey,
                factory);
        }

        // ============================================================
        // CREATE
        // ============================================================

        /// <summary>
        /// Tạo Action theo ActionKey.
        /// </summary>
        public IWorkflowAction Create(
            string actionKey)
        {
            if (string.IsNullOrWhiteSpace(actionKey))
            {
                throw new ArgumentException(
                    "ActionKey không được rỗng.",
                    nameof(actionKey));
            }

            if (!_factories.TryGetValue(
                    actionKey,
                    out Func<IWorkflowAction> factory))
            {
                throw new KeyNotFoundException(
                    $"Không tìm thấy WorkflowAction '{actionKey}'.");
            }

            IWorkflowAction action =
                factory();

            if (action == null)
            {
                throw new InvalidOperationException(
                    $"Factory của WorkflowAction '{actionKey}' " +
                    "trả về null.");
            }

            if (string.IsNullOrWhiteSpace(
                    action.ActionKey))
            {
                throw new InvalidOperationException(
                    $"WorkflowAction '{actionKey}' " +
                    "không cung cấp ActionKey.");
            }

            if (!string.Equals(
                    action.ActionKey,
                    actionKey,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    $"WorkflowAction factory đăng ký key " +
                    $"'{actionKey}' nhưng Action thực tế có key " +
                    $"'{action.ActionKey}'.");
            }

            return action;
        }

        // ============================================================
        // EXISTS
        // ============================================================

        /// <summary>
        /// Kiểm tra ActionKey đã được đăng ký hay chưa.
        /// </summary>
        public bool Contains(
            string actionKey)
        {
            if (string.IsNullOrWhiteSpace(actionKey))
            {
                return false;
            }

            return _factories.ContainsKey(
                actionKey);
        }
    }
}