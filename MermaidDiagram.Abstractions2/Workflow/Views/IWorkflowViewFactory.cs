
using MermaidDiagram.Abstractions.Workflow.Runtime;
using System;

namespace MermaidDiagram.Abstractions.Workflow.Views
{
    /// <summary>
    /// Factory tạo Workflow View từ một WorkflowViewDefinition.
    ///
    /// Abstractions chỉ định nghĩa contract.
    /// Implementation cụ thể nằm ở Core hoặc tầng UI/integration.
    /// </summary>
    public interface IWorkflowViewFactory
    {
        /// <summary>
        /// Đăng ký một loại Workflow View.
        ///
        /// viewKey phải trùng với
        /// WorkflowViewDefinition.ViewKey.
        /// </summary>
        void Register(
            string viewKey,
            Func<WorkflowContext, IWorkflowView> factory);

        /// <summary>
        /// Đăng ký hoặc thay thế một Workflow View.
        /// </summary>
        void RegisterOrReplace(
            string viewKey,
            Func<WorkflowContext, IWorkflowView> factory);

        /// <summary>
        /// Tạo Workflow View theo definition.
        /// </summary>
        IWorkflowView Create(
            WorkflowViewDefinition definition,
            WorkflowContext context);

        /// <summary>
        /// Kiểm tra ViewKey đã được đăng ký hay chưa.
        /// </summary>
        bool Contains(
            string viewKey);
    }
}

