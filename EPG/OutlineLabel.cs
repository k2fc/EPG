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
        private float dropShadowDistance;
        private float borderSize;
        private Color borderColor;

        private PointF point;
        private RectangleF offsetrect;
        private SizeF drawSize;
        private Pen drawPen;
        private GraphicsPath drawPath;
        private GraphicsPath shadowPath;
        private SolidBrush forecolorBrush;
        private SolidBrush shadowBrush;

        public OutlineLabel()
        {
            this.borderSize = 1.5f;
            this.borderColor = Color.Black;
            this.dropShadowDistance = 2;
            this.drawPath = new GraphicsPath();
            this.shadowPath = new GraphicsPath();
            this.drawPen = new Pen(new SolidBrush(this.borderColor), borderSize);
            this.forecolorBrush = new SolidBrush(this.ForeColor);
            this.shadowBrush = new SolidBrush(this.borderColor);
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
        [Browsable(false)]
        [Category("Appearance")]
        [Description("The border's color")]
        public Color BorderColor
        {
            get { return this.borderColor;}
            set
            {
                this.borderColor = value;

                if (this.BorderSize != 0)
                    this.drawPen.Color = value;
                this.shadowBrush.Color = value;
                this.Invalidate();
            }
        }
        public float DropShadowDistance
        {
            get { return this.dropShadowDistance; }
            set
            {
                this.dropShadowDistance = value;
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
            
            if (this.AutoSize)
            {
                this.point.X = this.Padding.Left;
                this.point.Y = this.Padding.Top;
                this.drawSize = e.Graphics.MeasureString(this.Text, this.Font, new PointF(), sf);
                this.Size = new Size((int)(this.drawSize.Width + this.Padding.Left + this.Padding.Right), (int)(this.drawSize.Height + this.Padding.Top + this.Padding.Bottom));
            }
            else
            {
                var testSize = new SizeF()
                {
                    Width = this.Size.Width,
                    Height = 1000
                };
                this.drawSize = e.Graphics.MeasureString(this.Text, this.Font, testSize);
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
            }
            var innersize = new SizeF()
            {
                Width = this.Width - (this.Padding.Left + this.Padding.Right),
                Height = this.Height - (this.Padding.Top + this.Padding.Bottom)
            };
            var rect = new RectangleF(point, innersize);
            float fontSize = e.Graphics.DpiY * this.Font.SizeInPoints / 72;

            this.drawPath.Reset();
            this.drawPath.AddString(this.Text, this.Font.FontFamily, (int)this.Font.Style, fontSize, rect, sf);


            //RenderDropshadowText(e.Graphics, this.Text, this.Font, this.ForeColor,this.BorderColor, 255, point);
            offsetrect = new RectangleF()
            {
                Location = new PointF(point.X + this.dropShadowDistance, point.Y + this.dropShadowDistance),
                Size = innersize
            };
            this.shadowPath.Reset();
            this.shadowPath.AddString(this.Text, this.Font.FontFamily, (int)this.Font.Style, fontSize, offsetrect, sf);

            e.Graphics.FillPath(this.shadowBrush, this.shadowPath);
            e.Graphics.FillPath(this.forecolorBrush, this.drawPath);
            e.Graphics.DrawPath(this.drawPen, this.drawPath);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.forecolorBrush != null) this.forecolorBrush.Dispose();
                if (this.shadowBrush != null) this.shadowBrush.Dispose();
                if (this.drawPath != null) this.drawPath.Dispose();
                if (this.shadowPath != null) this.shadowPath.Dispose();
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
