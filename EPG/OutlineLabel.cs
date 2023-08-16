using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace EPG
{
    internal class OutlineLabel : Label
    {
        private float borderSize;
        private Color borderColor;

        private PointF point;
        private SizeF drawSize;
        private Pen drawPen;
        private GraphicsPath drawPath;
        private SolidBrush forecolorBrush;

        public OutlineLabel()
        {
            this.borderSize = 1.5f;
            this.borderColor = Color.Black;
            this.drawPath = new GraphicsPath();
            this.drawPen = new Pen(new SolidBrush(this.borderColor), borderSize);
            this.forecolorBrush = new SolidBrush(this.ForeColor);
            this.Invalidate();
        }
        [Browsable(false)]
        [Category("Appearance")]
        [Description("The border's thickness")]
        [DefaultValue(1f)]
        public float BorderSize
        {
            get { return this.borderSize; }
            set
            {
                this.borderSize = value;
                if (value == 0)
                {
                    this.drawPen.Color = Color.Transparent;
                }
                else
                {
                    this.drawPen.Color = this.borderColor;
                    this.drawPen.Width = value;
                }
                this.OnTextChanged(EventArgs.Empty);
            }
        }
        public Color BorderColor
        {
            get { return this.borderColor;}
            set
            {
                this.borderColor = value;

                if (this.BorderSize != 0)
                    this.drawPen.Color = value;

                this.Invalidate();
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (this.Text.Length == 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
            StringFormat sf = new StringFormat();
            this.drawSize = e.Graphics.MeasureString(this.Text, this.Font, new PointF(), sf);

            if (this.AutoSize)
            {
                this.point.X = this.Padding.Left;
                this.point.Y = this.Padding.Top;
            }
            else
            {
                if (this.TextAlign == ContentAlignment.TopLeft ||
                    this.TextAlign == ContentAlignment.MiddleLeft ||
                    this.TextAlign == ContentAlignment.BottomLeft)
                    this.point.X = this.Padding.Left;
                else if (this.TextAlign == ContentAlignment.TopRight ||
                    this.TextAlign == ContentAlignment.MiddleRight ||
                    this.TextAlign == ContentAlignment.BottomRight)
                    this.point.X = this.Width - (this.Padding.Right + this.drawSize.Width);
                else point.X = (this.Width - this.drawSize.Width) / 2;

                if (this.TextAlign == ContentAlignment.TopLeft ||
                    this.TextAlign == ContentAlignment.TopCenter ||
                    this.TextAlign == ContentAlignment.TopRight)
                    this.point.Y = this.Padding.Top;
                else if (this.TextAlign == ContentAlignment.BottomLeft ||
                    this.TextAlign == ContentAlignment.BottomCenter ||
                    this.TextAlign == ContentAlignment.BottomRight)
                    this.point.Y = this.Height - (this.Padding.Bottom + this.drawSize.Height);
                else point.Y = (this.Height - this.drawSize.Height) / 2;

                float fontSize = e.Graphics.DpiY * this.Font.SizeInPoints / 72;

                this.drawPath.Reset();
                this.drawPath.AddString(this.Text, this.Font.FontFamily, (int)this.Font.Style, fontSize, point, sf);

                e.Graphics.FillPath(this.forecolorBrush, this.drawPath);
                e.Graphics.DrawPath(this.drawPen, this.drawPath);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.forecolorBrush != null) this.forecolorBrush.Dispose();
                if (this.drawPath != null) this.drawPath.Dispose();
                if (this.drawPen != null) this.drawPen.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);
            this.Invalidate();
        }
        protected override void OnTextAlignChanged(EventArgs e)
        {
            base.OnTextAlignChanged(e);
            this.Invalidate();
        }
        protected override void OnForeColorChanged(EventArgs e)
        {
            this.forecolorBrush.Color = base.ForeColor;
            base.OnForeColorChanged(e);
            this.Invalidate();
        }
    }
}
