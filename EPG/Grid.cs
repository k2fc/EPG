using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EPG
{
    internal class Grid : Panel
    {
        public bool hasTimePanels = true;
        public Panel newCurrentTimeSlot;
        public Panel newPlus30TimeSlot;
        public Panel newPlus60TimeSlot;
        public Grid() : base()
        {
            this.DoubleBuffered = true;
        }
    }
}
