
namespace MermaidDiagram.Core.Layout.Bounds
{
    /// <summary>
    /// Bounding rectangle dùng chung trong Layout layer.
    ///
    /// MermaidBounds chỉ chứa geometry:
    ///
    ///     X
    ///     Y
    ///     Width
    ///     Height
    ///
    /// Không chứa:
    ///     - Graph analysis
    ///     - Layout algorithm
    ///     - Collision logic
    ///     - Edge routing
    ///     - DevExpress
    /// </summary>
    public sealed class MermaidBounds
    {
        // ============================================================
        // GEOMETRY
        // ============================================================

        public float X
        {
            get;
        }

        public float Y
        {
            get;
        }

        public float Width
        {
            get;
        }

        public float Height
        {
            get;
        }

        // ============================================================
        // DERIVED GEOMETRY
        // ============================================================

        public float Right
        {
            get
            {
                return X + Width;
            }
        }

        public float Bottom
        {
            get
            {
                return Y + Height;
            }
        }

        public float CenterX
        {
            get
            {
                return X +
                       Width / 2f;
            }
        }

        public float CenterY
        {
            get
            {
                return Y +
                       Height / 2f;
            }
        }

        // ============================================================
        // VALIDATION
        // ============================================================

        /// <summary>
        /// Cho biết bounds có geometry hợp lệ hay không.
        ///
        /// Bounds hợp lệ khi:
        ///
        ///     - không NaN
        ///     - không Infinity
        ///     - Width > 0
        ///     - Height > 0
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (float.IsNaN(X) ||
                    float.IsNaN(Y) ||
                    float.IsNaN(Width) ||
                    float.IsNaN(Height))
                {
                    return false;
                }

                if (float.IsInfinity(X) ||
                    float.IsInfinity(Y) ||
                    float.IsInfinity(Width) ||
                    float.IsInfinity(Height))
                {
                    return false;
                }

                if (Width <= 0f ||
                    Height <= 0f)
                {
                    return false;
                }

                return true;
            }
        }

        // ============================================================
        // EMPTY
        // ============================================================

        /// <summary>
        /// Bounds rỗng.
        ///
        /// Dùng khi chưa có child nào có geometry hợp lệ.
        /// </summary>
        public static MermaidBounds Empty
        {
            get
            {
                return new MermaidBounds(
                    0f,
                    0f,
                    0f,
                    0f);
            }
        }

        // ============================================================
        // CONSTRUCTOR
        // ============================================================

        public MermaidBounds(
            float x,
            float y,
            float width,
            float height)
        {
            X =
                x;

            Y =
                y;

            Width =
                width;

            Height =
                height;
        }
    }
}

