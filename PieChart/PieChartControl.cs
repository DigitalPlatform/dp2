using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace System.Drawing.PieChart {
	/// <summary>
	/// Summary description for PieChartControl.
	/// </summary>
    public class PieChartControl : System.Windows.Forms.Panel {
        /// <summary>
        ///   Initializes the <c>PieChartControl</c>.
        /// </summary>
        public PieChartControl() : base() {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.DoubleBuffer, true);
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            m_toolTip = new ToolTip();
        }

        /// <summary>
        ///   Sets the left margin for the chart.
        /// </summary>
        public float LeftMargin {
            set { 
                Debug.Assert(value >= 0);
                m_leftMargin = value; 
                Invalidate();
            }
        }

        /// <summary>
        ///   Sets the right margin for the chart.
        /// </summary>
        public float RightMargin {
            set { 
                Debug.Assert(value >= 0);
                m_rightMargin = value; 
                Invalidate();
            }
        }

        /// <summary>
        ///   Sets the top margin for the chart.
        /// </summary>
        public float TopMargin {
            set { 
                Debug.Assert(value >= 0);
                m_topMargin = value;
                Invalidate();
            }
        }

        /// <summary>
        ///   Sets the bottom margin for the chart.
        /// </summary>
        public float BottomMargin {
            set { 
                Debug.Assert(value >= 0);
                m_bottomMargin = value; 
                Invalidate();
            }
        }

        /// <summary>
        ///   Sets the indicator if chart should fit the bounding rectangle
        ///   exactly.
        /// </summary>
        public bool FitChart {
            set { 
                m_fitChart = value; 
                Invalidate();
            }
        }

        /// <summary>
        ///   Sets values to be represented by the chart.
        /// </summary>
        public decimal[] Values {
            set { 
                m_values = value;
                Invalidate();
            }
        }

        /// <summary>
        ///   Sets colors to be used for rendering pie slices.
        /// </summary>
        public Color[] Colors {
            set { 
                m_colors = value; 
                Invalidate();
            }
        }

        /// <summary>
        ///   Sets values for slice displacements.
        /// </summary>
        public float[] SliceRelativeDisplacements {
            set { 
                m_relativeSliceDisplacements = value; 
                Invalidate();
            }
        }

        /// <summary>
        ///   Gets or sets tooltip texts.
        /// </summary>
        public string[] ToolTips {
            set { m_toolTipTexts = value; }
            get { return m_toolTipTexts; }
        }

        /// <summary>
        ///   Sets texts appearing by each pie slice.
        /// </summary>
        public string[] Texts {
            set { m_texts = value; }
        }

        /// <summary>
        ///   Sets pie slice reative height.
        /// </summary>
        public float SliceRelativeHeight {
            set { 
                m_sliceRelativeHeight = value; 
                Invalidate();
            }
        }

        /// <summary>
        ///   Sets the shadow style.
        /// </summary>
        public ShadowStyle ShadowStyle {
            set { 
                m_shadowStyle = value; 
                Invalidate();
            }
        }

        /// <summary>
        ///  Sets the edge color type.
        /// </summary>
        public EdgeColorType EdgeColorType {
            set { 
                m_edgeColorType = value; 
                Invalidate();
            }
        }

        /// <summary>
        ///   Sets the edge lines width.
        /// </summary>
        public float EdgeLineWidth {
            set { 
                m_edgeLineWidth = value; 
                Invalidate();
            }
        }

        /// <summary>
        ///   Sets the initial angle from which pies are drawn.
        /// </summary>
        public float InitialAngle {
            set { 
                m_initialAngle = value; 
                Invalidate();
            }
        }

        /// <summary>
        ///   Handles <c>OnPaint</c> event.
        /// </summary>
        /// <param name="args">
        ///   <c>PaintEventArgs</c> object.
        /// </param>
        protected override void OnPaint(PaintEventArgs args) {
            base.OnPaint(args);
            if (HasAnyValue) {
                DoDraw(args.Graphics);
            }
        }

        /// <summary>
        ///   Sets values for the chart and draws them.
        /// </summary>
        /// <param name="graphics">
        ///   Graphics object used for drawing.
        /// </param>
        protected void DoDraw(Graphics graphics) {
            if (m_values != null && m_values.Length > 0) {
                graphics.SmoothingMode = SmoothingMode.AntiAlias;
                float width = ClientSize.Width - m_leftMargin - m_rightMargin;
                float height = ClientSize.Height - m_topMargin - m_bottomMargin;
                // if the width or height if <=0 an exception would be thrown -> exit method..
                if (width <= 0 || height <= 0)
                    return;
                if (m_pieChart != null)
                    m_pieChart.Dispose();
                if (m_colors != null && m_colors.Length > 0)
                    m_pieChart = new PieChart3D(m_leftMargin, m_topMargin, width, height, m_values, m_colors, m_sliceRelativeHeight, m_texts); 
                else
                    m_pieChart = new PieChart3D(m_leftMargin, m_topMargin, width, height, m_values, m_sliceRelativeHeight, m_texts); 
                m_pieChart.FitToBoundingRectangle = m_fitChart;
                m_pieChart.InitialAngle = m_initialAngle;
                m_pieChart.SliceRelativeDisplacements = m_relativeSliceDisplacements;
                m_pieChart.EdgeColorType = m_edgeColorType;
                m_pieChart.EdgeLineWidth = m_edgeLineWidth;
                m_pieChart.ShadowStyle = m_shadowStyle;
                m_pieChart.HighlightedIndex = m_highlightedIndex;
                m_pieChart.Draw(graphics);
                m_pieChart.Font = this.Font;
                m_pieChart.ForeColor = this.ForeColor;
                m_pieChart.PlaceTexts(graphics);
            }
        }

        /// <summary>
        ///   Handles <c>MouseEnter</c> event to activate the tooltip.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseEnter(System.EventArgs e) {
            base.OnMouseEnter(e);
            m_defaultToolTipAutoPopDelay = m_toolTip.AutoPopDelay;
            m_toolTip.AutoPopDelay = Int16.MaxValue;
        }

        /// <summary>
        ///   Handles <c>MouseLeave</c> event to disable tooltip.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseLeave(System.EventArgs e) {
            base.OnMouseLeave(e);
            m_toolTip.RemoveAll();
            m_toolTip.AutoPopDelay = m_defaultToolTipAutoPopDelay;
            m_highlightedIndex = -1;
            Refresh();
        }

        /// <summary>
        ///   Handles <c>MouseMove</c> event to display tooltip for the pie
        ///   slice under pointer and to display slice in highlighted color.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e) {
            base.OnMouseMove(e);
            if (m_pieChart != null && m_values != null && m_values.Length > 0) {
                int index = m_pieChart.FindPieSliceUnderPoint(new PointF(e.X, e.Y));
                if (index != m_highlightedIndex) {
                    m_highlightedIndex = index;
                    Refresh();
                }
                if (m_highlightedIndex != -1) {
                    if (m_toolTipTexts == null || m_toolTipTexts.Length <= m_highlightedIndex || m_toolTipTexts[m_highlightedIndex].Length == 0)
                        m_toolTip.SetToolTip(this, m_values[m_highlightedIndex].ToString());
                    else
                        m_toolTip.SetToolTip(this, m_toolTipTexts[m_highlightedIndex]);
                }
                else {
                    m_toolTip.RemoveAll();
                }
            }
        }

        /// <summary>
        ///   Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (!m_disposed) {
                try {
                    if (disposing) {
                        if (m_pieChart != null) // 2012/10/3
                            m_pieChart.Dispose();
                        m_toolTip.Dispose();
                    }
                    m_disposed = true;
                }
                finally {
                    base.Dispose(disposing);
                }
            }
        }

        /// <summary>
        ///   Gets a flag indicating if at least one value is nonzero.
        /// </summary>
        private bool HasAnyValue {
            get {
                if (m_values == null)
                    return false;
                foreach (decimal angle in m_values) {
                    if (angle != 0) {
                        return true;
                    }
                }
                return false;
            }
        }

        private PieChart3D      m_pieChart = null;
        private float           m_leftMargin;
        private float           m_topMargin;
        private float           m_rightMargin;
        private float           m_bottomMargin;
        private bool            m_fitChart = false;

        private decimal[]       m_values = null;
        private Color[]         m_colors = null;
        private float           m_sliceRelativeHeight;
        private float[]         m_relativeSliceDisplacements = new float[] { 0F };
        private string[]        m_texts = null;
        private string[]        m_toolTipTexts = null;
        private ShadowStyle     m_shadowStyle = ShadowStyle.GradualShadow;
        private EdgeColorType   m_edgeColorType = EdgeColorType.SystemColor;
        private float           m_edgeLineWidth = 1F;
        private float           m_initialAngle;
        private int             m_highlightedIndex = -1;
        private ToolTip         m_toolTip = null;
        /// <summary>
        ///   Default AutoPopDelay of the ToolTip control.
        /// </summary>
        private int             m_defaultToolTipAutoPopDelay;
        /// <summary>
        ///   Flag indicating that object has been disposed.
        /// </summary>
        private bool            m_disposed = false;
	}
}