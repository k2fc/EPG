﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EPG
{
    internal class Box : Panel
    {
        public int BorderSize { get; set; } = 5;
        public Box() : base()
        {
            base.BorderStyle = BorderStyle.None;
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            ControlPaint.DrawBorder(e.Graphics, this.ClientRectangle, Color.White, BorderSize, ButtonBorderStyle.Outset, Color.White, BorderSize, ButtonBorderStyle.Outset, Color.Black, BorderSize, ButtonBorderStyle.Outset,
                Color.Black, BorderSize, ButtonBorderStyle.Outset);
            //base.OnPaint(e);
        }
    }
}