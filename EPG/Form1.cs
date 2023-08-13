//Copyright(C) 2019 Dennis Graiani
//
//This program is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.
//
//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
//GNU General Public License for more details.
//
//You should have received a copy of the GNU General Public License
//along with this program.If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Xml;
using System.IO;
using DomainObjects;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace EPG
{
    public partial class Form1 : Form
    {
        private DateTime curTime = new DateTime();
        private DateTime currentTimeSlot = new DateTime();
        private DateTime nextTimeSlot = new DateTime();
        private DateTime secondTimeSlot = new DateTime();
        private Panel gridTimeCurrent = new Panel();
        private Panel gridTimePlus30 = new Panel();
        private Panel gridTimePlus60 = new Panel();
        private Panel topRow = new Panel();
        private readonly Panel topMask = new Panel();
        private readonly Panel clockPanel = new Panel();
        private readonly Label clock = new Label();
        private readonly Label title = new Label();
        private IEnumerable<TVStation> stations;
        private List<Grid> grids = new List<Grid>();
        private string runReference;
        private Mutex cancelMutex;
        private int gridMargin = 25;
        private int gridVerticalStart = 240;
        private int rowHeight = 34;
        private int textMargin = 3;
        private int lookahead = 0;
        private Color background;
        private Color timeBackground;
        private Color timeForeground;
        private Color gridBackground;
        private Color gridForeground;
        private Color endBackground;
        private Color endForeground;
        private Font font = new Font("Consolas",14);
        private String titletext = "";
        private String endText = "";
        private int gridBottom = 0;
        private int speed = 1;
        private int listingInterval = 0;
        private Panel pauseatbox;
        private Grid pauseatgrid;
        private int pauselength = 2;
        private DateTime pauseuntil;
        private bool paused = false;
        private bool doneInit = false;
        private bool dataloaded = false;
        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;
            GetSettings();

            //create clock
            topRow.Top = gridVerticalStart;
            topRow.Left = gridMargin;
            topRow.Width = this.Width - (gridMargin * 2);
            topRow.Height = rowHeight;
            topRow.BorderStyle = BorderStyle.FixedSingle;
            clockPanel.Top = 0;
            clockPanel.Left = 0;
            clockPanel.Width = (this.Width - (gridMargin*2)) / 4;
            clockPanel.Height = rowHeight;
            clock.Width = clockPanel.Width - (textMargin * 2);
            clock.Left = textMargin;
            clock.Height = clockPanel.Height;
            clock.Top = 0;
            clock.TextAlign = ContentAlignment.MiddleRight;
            clock.Font = font;
            clock.ForeColor = gridForeground;
            clockPanel.Visible = true;
            clockPanel.BackColor = timeBackground;
            clockPanel.BorderStyle = BorderStyle.Fixed3D;
            clockPanel.Controls.Add(clock);
            topRow.Controls.Add(clockPanel);
            this.Controls.Add(topRow);

            //create the mask that hides the times as they get pushed off
            topMask.BackColor = background;
            topMask.Top = 0;
            topMask.Height = topRow.Top;
            topMask.Left = topRow.Left;
            topMask.Width = this.Width - (gridMargin * 2);
            topMask.Controls.Add(title);
            this.Controls.Add(topMask);

            formResize(null, null);
            
            Task.Run(() => GetGuideData());

            title.Top = 0;
            title.Left = 0;
            title.Width = this.Width;
            title.Height = this.Height;
            title.Text = "Loading Data\r\nPlease Wait...";
            title.TextAlign = ContentAlignment.MiddleCenter;

            timer1.Start();
            
        }
        Panel nextCurrent = null;
        Panel nextP30 = null;
        Panel nextP60 = null;
        private Grid nextGrid;
        private void timer1_Tick(object sender, EventArgs e)
        {
            curTime = DateTime.Now;
            clock.Text = curTime.ToString("h:mm:ss");
            List<Grid> murdergrids = grids.Where(x => x.Bottom <= topRow.Top).ToList();
            foreach (Grid murdergrid in murdergrids)
            {
                foreach (Control control in murdergrid.Controls)
                    control.Dispose();
                murdergrid.Controls.Clear();
                murdergrid.Dispose();
            }
            grids = grids.Where(x => x.Bottom >= topRow.Top).ToList();
            if (paused) 
            {
                if (curTime > pauseuntil)
                    paused = false;
                bool foundnextpause = false;
                foreach (Grid grid in grids)
                {
                    
                    if (grid.Bottom > this.Height && grid.Top < clockPanel.Bottom)
                    {
                        foreach (Panel box in grid.Controls)
                        {
                        
                            if (box.Bottom + grid.Top > this.Height && box.Top + grid.Top > clockPanel.Bottom)
                            {
                                if (!foundnextpause)
                                {
                                    pauseatgrid = grid;
                                    pauseatbox = box;
                                    foundnextpause = true;
                                }
                            }
                        }
                        if (!foundnextpause)
                        {
                            pauseatgrid = grid;
                            pauseatbox = (Panel)grid.Controls[grid.Controls.Count - 1];
                        }
                    }
                    else if (grid.Top < this.Height && !foundnextpause)
                    {
                        foreach (Panel box in grid.Controls)
                        {

                            if (box.Bottom + grid.Top > this.Height && box.Top + grid.Top > clockPanel.Bottom)
                            {
                                if (!foundnextpause)
                                {
                                    pauseatgrid = grid;
                                    pauseatbox = box;
                                    foundnextpause = true;
                                }
                            }
                        }
                        if (!foundnextpause)
                        {
                            pauseatgrid = grid;
                            pauseatbox = (Panel)grid.Controls[grid.Controls.Count -1]; 
                        }
                    }
                }

                if (gridBottom <= this.Height * 2)
                {
                    generateGrid();

                }



            }
         
            else {

                gridBottom -= speed;
                foreach (Grid grid in grids)
                {
                    grid.Top -= speed;
                    
                    
                    if (grid.newCurrentTimeSlot != null)
                    {
                        if (grid.newCurrentTimeSlot.Top + grid.Top < topRow.Bottom)
                        {
                            if (nextGrid != grid)
                            {
                                nextGrid = grid;
                                nextCurrent = new Panel()
                                {
                                    Width = grid.newCurrentTimeSlot.Width,
                                    Height = grid.newCurrentTimeSlot.Height,
                                    Left = clockPanel.Right,
                                    Top = topRow.Height,
                                    BackColor = timeBackground,
                                    BorderStyle = BorderStyle.Fixed3D
                                };
                                topRow.Controls.Add(nextCurrent);
                                nextP30 = new Panel()
                                {
                                    Width = grid.newPlus30TimeSlot.Width,
                                    Height = grid.newPlus30TimeSlot.Height,
                                    Left = nextCurrent.Right,
                                    Top = topRow.Height,
                                    BackColor = timeBackground,
                                    BorderStyle = BorderStyle.Fixed3D
                                };
                                topRow.Controls.Add(nextP30);
                                nextP60 = new Panel()
                                {
                                    Width = grid.newPlus60TimeSlot.Width,
                                    Height = grid.newPlus60TimeSlot.Height,
                                    Left = nextP30.Right,
                                    Top = topRow.Height,
                                    BackColor = timeBackground,
                                    BorderStyle = BorderStyle.Fixed3D
                                };
                                topRow.Controls.Add(nextP60);
                                var ncLabel = grid.newCurrentTimeSlot.Controls[0] as Label;
                                var np30Label = grid.newPlus30TimeSlot.Controls[0] as Label;
                                var np60Label = grid.newPlus60TimeSlot.Controls[0] as Label;
                                nextCurrent.Controls.Add(new Label()
                                {
                                    Top = 0,
                                    Left = textMargin,
                                    Height = nextCurrent.Height,
                                    Width = nextCurrent.Width - (textMargin * 2),
                                    Text = ncLabel.Text,
                                    ForeColor = timeForeground,
                                    BackColor = timeBackground,
                                    Font = ncLabel.Font,
                                    TextAlign = ContentAlignment.MiddleCenter
                                });
                                nextP30.Controls.Add(new Label()
                                {
                                    Top = 0,
                                    Left = textMargin,
                                    Height = nextP30.Height,
                                    Width = nextP30.Width - (textMargin * 2),
                                    Text = np30Label.Text,
                                    ForeColor = timeForeground,
                                    BackColor = timeBackground,
                                    Font = np30Label.Font,
                                    TextAlign = ContentAlignment.MiddleCenter
                                });
                                nextP60.Controls.Add(new Label()
                                {
                                    Top = 0,
                                    Left = textMargin,
                                    Height = nextP60.Height,
                                    Width = nextP60.Width - (textMargin * 2),
                                    Text = np60Label.Text,
                                    ForeColor = timeForeground,
                                    BackColor = timeBackground,
                                    Font = np60Label.Font,
                                    TextAlign = ContentAlignment.MiddleCenter
                                });
                            }
                            gridTimeCurrent.Top -= speed;
                            gridTimePlus30.Top -= speed;
                            gridTimePlus60.Top -= speed;
                            if (nextCurrent != null)
                            {
                                nextCurrent.Top -= speed;
                                nextP30.Top -= speed;
                                nextP60.Top -= speed;
                            }
                            topRow.BringToFront();
                            topMask.BringToFront();
                        }

                        if (grid.newCurrentTimeSlot.Bottom + grid.Top <= topRow.Bottom)
                        {
                            gridTimeCurrent.Dispose();
                            gridTimePlus30.Dispose();
                            gridTimePlus30.Dispose();
                            topRow.Controls.Remove(gridTimeCurrent);
                            topRow.Controls.Remove(gridTimePlus30);
                            topRow.Controls.Remove(gridTimePlus60);
                            gridTimeCurrent = nextCurrent;
                            gridTimePlus30 = nextP30;
                            gridTimePlus60 = nextP60;
                            grid.Controls.Remove(grid.newCurrentTimeSlot);
                            grid.Controls.Remove(grid.newPlus30TimeSlot);
                            grid.Controls.Remove(grid.newPlus60TimeSlot);
                            grid.newCurrentTimeSlot = null;
                            grid.newPlus30TimeSlot = null;
                            grid.newPlus60TimeSlot = null;
                            gridTimeCurrent.Top = clockPanel.Top;
                            gridTimePlus30.Top = clockPanel.Top;
                            gridTimePlus60.Top = clockPanel.Top;
                            grid.SendToBack();
                            gridTimeCurrent.Left = clockPanel.Width;
                            gridTimePlus30.Left = gridTimeCurrent.Right;
                            gridTimePlus60.Left = gridTimePlus30.Right;
                            //topRow.Controls.Add(gridTimeCurrent);
                            //topRow.Controls.Add(gridTimePlus30);
                            //topRow.Controls.Add(gridTimePlus60);
                            //gridTimeCurrent.BringToFront();
                            //gridTimePlus30.BringToFront();
                            //gridTimePlus60.BringToFront();
                            

                            pauseatbox = (Panel)grid.Controls[3];
                        }

                    }

                }

                if (doneInit && dataloaded)
                {
                    if (grids.Count < 1)
                    {
                        generateGrid();
                    }
                    

                    if (pauseatgrid != null && pauseatbox != null)
                    {
                        if (pauseatbox.Top + pauseatgrid.Top <= topRow.Bottom &&
                            pauseatbox.Top + pauseatgrid.Top >= topRow.Top)
                        {

                            pauseuntil = curTime.AddSeconds(pauselength);
                            paused = true;
                            GetSettings();

                        }
                    }
                    
                }
                
                
                
            }
                
                 

                
            doneInit = true;

            
        }

        public static DateTime RoundDown(DateTime dt, TimeSpan d)
        {
            var delta = dt.Ticks % d.Ticks;
            return new DateTime(dt.Ticks - delta, dt.Kind);
        }

        private void generateGrid()
        {
            if (!doneInit || !dataloaded )
                return;
            GetSettings();


            Grid grid = new Grid
            {
                Left = 0,
                Width = this.Width
            };
            

            Panel newCurrentTimeSlot = new Panel();
            Panel newPlus30TimeSlot = new Panel();
            Panel newPlus60TimeSlot = new Panel();
            Label newCurrentTSLabel = new Label();
            Label newPlus30TSLabel = new Label();
            Label newPlus60TSLabel = new Label();

            curTime = DateTime.Now;
            currentTimeSlot = RoundDown(curTime+TimeSpan.FromMinutes(lookahead), TimeSpan.FromMinutes(30));
            nextTimeSlot = currentTimeSlot + TimeSpan.FromMinutes(30);
            secondTimeSlot = currentTimeSlot + TimeSpan.FromMinutes(60);

            Panel blank = new Panel
            {
                Top = 0,
                Width = clockPanel.Width,
                Height = clockPanel.Height,
                Left = topRow.Left,
                BackColor = gridBackground,
                BorderStyle = BorderStyle.Fixed3D
            };
            grid.Controls.Add(blank);

            newCurrentTimeSlot.Top = blank.Top;
            newCurrentTimeSlot.Left = topRow.Left + clockPanel.Width;
            newCurrentTimeSlot.Width = (this.Width - (gridMargin * 2)) / 4;
            newCurrentTimeSlot.Height = clockPanel.Height;
            newCurrentTSLabel.Width = newCurrentTimeSlot.Width - (textMargin * 2);
            newCurrentTSLabel.Left = textMargin;
            newCurrentTSLabel.Height = newCurrentTimeSlot.Height;
            newCurrentTSLabel.Top = 0;
            newCurrentTSLabel.TextAlign = ContentAlignment.MiddleCenter;
            newCurrentTSLabel.Font = font;
            newCurrentTSLabel.ForeColor = timeForeground;
            newCurrentTSLabel.Text = currentTimeSlot.ToString("h:mm tt");
            newCurrentTimeSlot.Visible = true;
            newCurrentTimeSlot.BackColor = gridBackground;
            newCurrentTimeSlot.BorderStyle = BorderStyle.Fixed3D;
            newCurrentTimeSlot.Controls.Add(newCurrentTSLabel);
            grid.Controls.Add(newCurrentTimeSlot);

            newPlus30TimeSlot.Top = blank.Top;
            newPlus30TimeSlot.Left = newCurrentTimeSlot.Right;
            newPlus30TimeSlot.Width = (this.Width - (gridMargin * 2)) / 4;
            newPlus30TimeSlot.Height = newCurrentTimeSlot.Height;
            newPlus30TSLabel.Width = newPlus30TimeSlot.Width - (textMargin * 2);
            newPlus30TSLabel.Left = textMargin;
            newPlus30TSLabel.Height = newPlus30TimeSlot.Height;
            newPlus30TSLabel.Top = 0;
            newPlus30TSLabel.TextAlign = ContentAlignment.MiddleCenter;
            newPlus30TSLabel.Font = font;
            newPlus30TSLabel.ForeColor = timeForeground;
            newPlus30TSLabel.Text = nextTimeSlot.ToString("h:mm tt");
            newPlus30TimeSlot.Visible = true;
            newPlus30TimeSlot.BackColor = gridBackground;
            newPlus30TimeSlot.BorderStyle = BorderStyle.Fixed3D;
            newPlus30TimeSlot.Controls.Add(newPlus30TSLabel);
            grid.Controls.Add(newPlus30TimeSlot);

            newPlus60TimeSlot.Top = blank.Top;
            newPlus60TimeSlot.Left = newPlus30TimeSlot.Right;
            newPlus60TimeSlot.Width = (this.Width - (gridMargin * 2)) / 4;
            newPlus60TimeSlot.Height = newPlus30TimeSlot.Height;
            newPlus60TSLabel.Width = newPlus60TimeSlot.Width - (textMargin * 2);
            newPlus60TSLabel.Left = textMargin;
            newPlus60TSLabel.Height = newPlus60TimeSlot.Height;
            newPlus60TSLabel.Top = 0;
            newPlus60TSLabel.TextAlign = ContentAlignment.MiddleCenter;
            newPlus60TSLabel.Font = font;
            newPlus60TSLabel.ForeColor = timeForeground;
            newPlus60TSLabel.Text = secondTimeSlot.ToString("h:mm tt");
            newPlus60TimeSlot.Visible = true;
            newPlus60TimeSlot.BackColor = gridBackground;
            newPlus60TimeSlot.BorderStyle = BorderStyle.Fixed3D;
            newPlus60TimeSlot.Controls.Add(newPlus60TSLabel);
            grid.Controls.Add(newPlus60TimeSlot);

            //newCurrentTimeSlot.BringToFront();
            //newPlus30TimeSlot.BringToFront();
            //newPlus60TimeSlot.BringToFront();

            grid.newCurrentTimeSlot = newCurrentTimeSlot;
            grid.newPlus30TimeSlot = newPlus30TimeSlot;
            grid.newPlus60TimeSlot = newPlus60TimeSlot;
            //gridBottom = newPlus60TimeSlot.Bottom;

            var blank2 = new Panel
            {
                Top = newPlus60TimeSlot.Bottom,
                Width = clockPanel.Width,
                Height = clockPanel.Height,
                Left = topRow.Left,
                BackColor = gridBackground,
                BorderStyle = BorderStyle.Fixed3D
            };
            grid.Controls.Add(blank2);

            Panel datePanel = new Panel();
            Label dateLabel = new Label();
            datePanel.Top = newPlus60TimeSlot.Bottom;
            datePanel.Width = clockPanel.Width *3 ;
            datePanel.Height = clockPanel.Height;
            datePanel.Left = blank.Right;
            datePanel.BackColor = gridBackground;
            datePanel.BorderStyle = BorderStyle.Fixed3D;
            dateLabel.Top = 0;
            dateLabel.Left = 0;
            dateLabel.Height = datePanel.Height;
            dateLabel.Width = datePanel.Width;
            dateLabel.Font = font;
            dateLabel.ForeColor = timeForeground;
            dateLabel.TextAlign = ContentAlignment.MiddleCenter;
            dateLabel.Text = currentTimeSlot.ToString("dddd MMMM d yyyy");
            datePanel.Controls.Add(dateLabel);
            grid.Controls.Add(datePanel);

            int gridBottom = datePanel.Bottom;

            XmlDataDocument configFile = new XmlDataDocument();

            XmlNodeList channels;
            FileStream fs = new FileStream("epg.xml", FileMode.Open, FileAccess.Read);
            configFile.Load(fs);
            fs.Close();
            
            
            ////XmltvIdFormat.
            channels = configFile.GetElementsByTagName("channel");
            var stationList = stations.OrderBy(station => station.LogicalChannelNumber).ThenBy(station => station.MinorChannelNumber).ThenBy(station => station.ServiceID).Where(station => station.Included).ToList();
            foreach (TVStation channel in stationList)
            {
                Panel channelPanel = new Panel();
                Label channelNum = new Label();
                Label channelName = new Label();

                channelPanel.Left = topRow.Left;
                channelPanel.Width = clockPanel.Width;
                channelPanel.Height = rowHeight * 2;
                channelPanel.Top = gridBottom;
                channelPanel.BackColor = gridBackground;
                channelPanel.BorderStyle = BorderStyle.Fixed3D;
                channelNum.Top = 0;
                channelNum.Left = 0;
                channelNum.Width = channelPanel.Width - (textMargin * 2);
                channelNum.Height = channelPanel.Height / 2;
                channelNum.Font = font;
                channelNum.ForeColor = timeForeground;
                if (channel.MinorChannelNumber > -1)
                    channelNum.Text = channel.TransportStreamID.ToString() + "." + channel.MinorChannelNumber.ToString();
                else if (channel.LogicalChannelNumber > -1)
                    channelNum.Text = channel.LogicalChannelNumber.ToString();
                else
                    channelNum.Text = channel.TransportStreamID.ToString();
                channelNum.TextAlign = ContentAlignment.MiddleCenter;
                channelName.Top = channelNum.Bottom;
                channelName.Left = 0;
                channelName.Width = channelPanel.Width - (textMargin * 2);
                channelName.Height = channelPanel.Height / 2;
                channelName.Font = font;
                channelName.ForeColor = timeForeground;
                channelName.Text = channel.Name;
                channelName.TextAlign = ContentAlignment.MiddleCenter;
                channelPanel.Controls.Add(channelNum);
                channelPanel.Controls.Add(channelName);
                channelPanel.SendToBack();
                grid.Controls.Add(channelPanel);

                try
                {
                    channelName.Text = channel.Name.Trim().ToUpper();

                    bool noprograms = true;
                    foreach (var program in channel.EPGCollection)
                    {
                        DateTime programStartTime = program.StartTime.ToLocalTime();
                        DateTime programEndTime = programStartTime + program.Duration;
                        TimeSpan contThreshold = new TimeSpan(0, 2, 0);
                        if (programStartTime < secondTimeSlot.AddMinutes(30) && programEndTime > currentTimeSlot)
                        {
                            Panel programPanel = new Panel();
                            Label programLabel = new Label
                            {
                                Text = program.EventName
                            };
                            if (programStartTime < currentTimeSlot && programEndTime > secondTimeSlot + new TimeSpan(0, 30, 0))
                            {
                                programPanel.Left = channelPanel.Right;
                                programPanel.Width = clockPanel.Width * 3;
                                programLabel.Text = "";
                                if (programStartTime < currentTimeSlot - contThreshold)
                                    programLabel.Text = "< ";
                                programLabel.Text += program.EventName;
                                if (programEndTime > secondTimeSlot + new TimeSpan(0,30,0) + contThreshold)
                                    programLabel.Text += " >";
                            }
                            else if (programStartTime < currentTimeSlot)
                            {
                                programPanel.Left = channelPanel.Right;
                                programPanel.Width = Convert.ToInt32(programEndTime.Subtract(currentTimeSlot).TotalMinutes) * clockPanel.Width / 30;
                                programLabel.Text = "";
                                if (programStartTime < currentTimeSlot - contThreshold)
                                    programLabel.Text = "< ";
                                programLabel.Text += program.EventName;
                            }
                            else if (programEndTime > secondTimeSlot + new TimeSpan(0,30,0))
                            {
                                programPanel.Left = channelPanel.Right + Convert.ToInt32(programStartTime.Subtract(currentTimeSlot).TotalMinutes) * clockPanel.Width / 30;
                                programPanel.Width = Convert.ToInt32(secondTimeSlot.AddMinutes(30).Subtract(programStartTime).TotalMinutes) * clockPanel.Width / 30;
                                programLabel.Text = program.EventName;
                                if (programEndTime > secondTimeSlot + new TimeSpan(0, 30, 0) + contThreshold)
                                    programLabel.Text += " >";
                            }
                            else
                            {
                                programPanel.Left = channelPanel.Right + Convert.ToInt32(programStartTime.Subtract(currentTimeSlot).TotalMinutes) * clockPanel.Width / 30;
                                programPanel.Width = Convert.ToInt32(programEndTime.Subtract(programStartTime).TotalMinutes) * clockPanel.Width / 30;
                            }

                            programPanel.Top = channelPanel.Top;
                            programPanel.Height = channelPanel.Height;
                            programPanel.BackColor = gridBackground;
                            programPanel.SendToBack();
                            programPanel.BorderStyle = BorderStyle.Fixed3D;
                            programLabel.Font = font;
                            programLabel.ForeColor = gridForeground;
                            programLabel.Left = textMargin;
                            programLabel.Width = programPanel.Width - (textMargin * 2);
                            programLabel.Top = textMargin;
                            programLabel.Height = programPanel.Height - (textMargin * 2);
                            programLabel.TextAlign = ContentAlignment.TopLeft;
                            programPanel.Controls.Add(programLabel);
                                        
                            programLabel.UseMnemonic = false;
                            grid.Controls.Add(programPanel);
                            noprograms = false;
                        }
                            
                    }
                    if (noprograms)
                    {
                        Panel staticPanel = new Panel();
                        Label staticLabel = new Label();
                        staticPanel.Width = channelPanel.Width * 3;
                        staticPanel.Height = channelPanel.Height;
                        staticPanel.Top = channelPanel.Top;
                        staticPanel.Left = channelPanel.Right;
                        staticPanel.BackColor = gridBackground;
                        staticPanel.BorderStyle = BorderStyle.Fixed3D;
                        staticPanel.SendToBack();
                        staticLabel.Width = staticPanel.Width - (textMargin * 2);
                        staticLabel.Height = staticPanel.Height - (textMargin * 2);
                        staticLabel.Left = textMargin;
                        staticLabel.Top = textMargin;
                        staticLabel.ForeColor = gridForeground;
                        staticLabel.Font = font;
                        staticLabel.TextAlign = ContentAlignment.TopLeft;
                        staticLabel.Text = "Data Unavailable";
                        staticPanel.Controls.Add(staticLabel);
                        grid.Controls.Add(staticPanel);
                    }
                }
                catch
                {
                    Panel staticPanel = new Panel();
                    Label staticLabel = new Label();
                    staticPanel.Width = channelPanel.Width * 3;
                    staticPanel.Height = channelPanel.Height;
                    staticPanel.Top = channelPanel.Top;
                    staticPanel.Left = channelPanel.Right;
                    staticPanel.BackColor = gridBackground;
                    staticPanel.BorderStyle = BorderStyle.Fixed3D;
                    staticPanel.SendToBack();
                    staticLabel.Width = staticPanel.Width - (textMargin * 2);
                    staticLabel.Height = staticPanel.Height - (textMargin * 2);
                    staticLabel.Left = textMargin;
                    staticLabel.Top = textMargin;
                    staticLabel.ForeColor = gridForeground;
                    staticLabel.Font = font;
                    staticLabel.TextAlign = ContentAlignment.TopLeft;
                    staticLabel.Text = "Data Unavailable";
                    staticPanel.Controls.Add(staticLabel);
                    grid.Controls.Add(staticPanel);
                }
                
                gridBottom = channelPanel.Bottom;
            }
            Panel endPanel = new Panel();
            Label endLabel = new Label();
            endPanel.Width = clockPanel.Width * 4;
            endPanel.Height = rowHeight;
            endPanel.Top = gridBottom;
            endPanel.Left = gridMargin;
            endPanel.BackColor = endBackground;
            endPanel.BorderStyle = BorderStyle.Fixed3D;
            endPanel.SendToBack();
            endLabel.Width = endPanel.Width - (textMargin * 2);
            endLabel.Height = endPanel.Height - (textMargin * 2);
            endLabel.Left = textMargin;
            endLabel.Top = textMargin;
            endLabel.ForeColor = endForeground;
            endLabel.Font = font;
            endLabel.TextAlign = ContentAlignment.MiddleCenter;
            endLabel.Text = endText;
            endPanel.Controls.Add(endLabel);
            gridBottom = endPanel.Bottom;
            grid.Controls.Add(endPanel);

            title.Text = titletext;
            grid.Height = gridBottom;
            
            grid.SendToBack();
            
            if (grids.Count > 0)
                grid.Top = (from box in grids select box.Bottom).Max();
            else
                grid.Top = this.Bottom;
            grids.Add(grid);
            this.Controls.Add(grid);
            if (pauseatgrid == null)
            {
                pauseatgrid = grid;
                pauseatbox = (Panel)grid.Controls[6];
            }
                
            if (grids.Count < 2)
                this.gridBottom = grid.Height + this.Height;
            else 
                this.gridBottom += grid.Height;

        }

        private void formResize(object sender, EventArgs e)
        {
            //create clock
            topRow.Top = gridVerticalStart;
            topRow.Left = gridMargin;
            topRow.Width = this.Width - (gridMargin * 2);
            topRow.Height = rowHeight;
            topRow.BorderStyle = BorderStyle.None;
            clockPanel.Width = (this.Width - (gridMargin * 2)) / 4;
            clockPanel.Height = rowHeight;
            clock.Width = clockPanel.Width - (textMargin * 2);
            clock.Left = textMargin;
            clock.Height = clockPanel.Height;
            clock.Top = 0;
            clock.TextAlign = ContentAlignment.MiddleRight;
            clock.Font = font;
            clock.ForeColor = gridForeground;
            clockPanel.Visible = true;
            clockPanel.BackColor = timeBackground;
            clockPanel.BorderStyle = BorderStyle.Fixed3D;
            clockPanel.Controls.Add(clock);
            gridTimeCurrent.Top = this.Height;

            //create the mask that hides the times as they get pushed off
            topMask.BackColor = background;
            topMask.Top = 0;
            topMask.Height = topRow.Top;
            topMask.Left = topRow.Left;
            topMask.Width = this.Width - (gridMargin * 2);

            foreach (Panel grid in grids)
            {
                this.Controls.Remove(grid);
            }
            grids.Clear();
            pauseatbox = null;
            
        }

        private void GetGuideData()
        {
            DateTime now = DateTime.Now;
            
            runReference = now.DayOfYear.ToString() + now.TimeOfDay.Hours.ToString() + now.TimeOfDay.Minutes.ToString() + now.TimeOfDay.Seconds.ToString();
            
            do
            {
                RunParameters.Instance = new RunParameters(ParameterSet.Collector, RunType.Collection);
                if (stations is null)
                {
                    stations = RunParameters.Instance.StationCollection;//.OrderBy(station => station.LogicalChannelNumber).ThenBy(station => station.MinorChannelNumber).ThenBy(station => station.ServiceID).Where(station => station.Included).ToList();
                    dataloaded = true;
                }
                RunParameters.Instance.Process("EPG Collector.ini");
                HistoryRecord.Current = new HistoryRecord(DateTime.Now);
                RunParameters.Instance.Options.Add(new OptionEntry(OptionName.RunFromService));
                RunParameters.Instance.Options.Add(new OptionEntry(OptionName.CreateChannelDefFile));
                EPGCollector.Run(true);
                foreach (var item in RunParameters.Instance.StationCollection)
                {
                    if (item.MinorChannelNumber != -1)
                        item.LogicalChannelNumber = item.TransportStreamID;
                    item.EPGCollection = new Collection<EPGEntry>(item.EPGCollection.Where(epg => epg.StartTime + epg.Duration > currentTimeSlot).ToList());
                }
                stations = RunParameters.Instance.StationCollection.ToList();
                dataloaded = true;
                Thread.Sleep(listingInterval * 1000);
            } while (true);
        }


        private void GetSettings()
        {
            FileStream fs = null;
            try
            {
                XmlDocument settingsfile = new XmlDocument();
                fs = new FileStream("epg.xml", FileMode.Open, FileAccess.Read);
                settingsfile.Load(fs);
                RunParameters.BaseDirectory = "";
                XmlNodeList nodeList = settingsfile.SelectNodes("/epg/settings");
                foreach (XmlNode item in nodeList)
                {
                    if (item.Name == "settings")
                    {
                        titletext = item["Title"].InnerText;
                        endText = item["EndText"].InnerText;

                        string rgb = item["BackColor"].InnerText;
                        string[] colors = rgb.Split(',');
                        background = Color.FromArgb(Convert.ToInt32(colors[0]), Convert.ToInt32(colors[1]), Convert.ToInt32(colors[2]));

                        rgb = item["TimeBackground"].InnerText;
                        colors = rgb.Split(',');
                        timeBackground = Color.FromArgb(Convert.ToInt32(colors[0]), Convert.ToInt32(colors[1]), Convert.ToInt32(colors[2]));

                        rgb = item["TimeForeground"].InnerText;
                        colors = rgb.Split(',');
                        timeForeground = Color.FromArgb(Convert.ToInt32(colors[0]), Convert.ToInt32(colors[1]), Convert.ToInt32(colors[2]));

                        rgb = item["GridBackground"].InnerText;
                        colors = rgb.Split(',');
                        gridBackground = Color.FromArgb(Convert.ToInt32(colors[0]), Convert.ToInt32(colors[1]), Convert.ToInt32(colors[2]));

                        rgb = item["GridForeground"].InnerText;
                        colors = rgb.Split(',');
                        gridForeground = Color.FromArgb(Convert.ToInt32(colors[0]), Convert.ToInt32(colors[1]), Convert.ToInt32(colors[2]));

                        rgb = item["EndBackground"].InnerText;
                        colors = rgb.Split(',');
                        endBackground = Color.FromArgb(Convert.ToInt32(colors[0]), Convert.ToInt32(colors[1]), Convert.ToInt32(colors[2]));

                        rgb = item["EndForeground"].InnerText;
                        colors = rgb.Split(',');
                        endForeground = Color.FromArgb(Convert.ToInt32(colors[0]), Convert.ToInt32(colors[1]), Convert.ToInt32(colors[2]));

                        font = new Font(item["Font"].InnerText, Convert.ToInt32(item["FontSize"].InnerText));
                        speed = Convert.ToInt32(item["Speed"].InnerText);
                        pauselength = Convert.ToInt32(item["PauseLength"].InnerText);

                        int tempgridmargin = Convert.ToInt32(item["GridMargin"].InnerText);
                        int tempverticalstart = Convert.ToInt32(item["GridVerticalStart"].InnerText);
                        int temprowheight = Convert.ToInt32(item["RowHeight"].InnerText);
                        int tempmargin = Convert.ToInt32(item["TextMargin"].InnerText);
                        
                        bool fullscreen = Convert.ToBoolean(item["Fullscreen"].InnerText);

                        if (tempgridmargin != gridMargin || tempverticalstart != gridVerticalStart || temprowheight != rowHeight || tempmargin != textMargin || (this.FormBorderStyle == FormBorderStyle.None) != fullscreen)
                        {
                            gridMargin = tempgridmargin;
                            gridVerticalStart = tempverticalstart;
                            rowHeight = temprowheight;
                            textMargin = tempmargin;
                            if (fullscreen)
                            {
                                this.FormBorderStyle = FormBorderStyle.None;
                                this.WindowState = FormWindowState.Maximized;
                                Cursor.Hide();
                                this.TopMost = true;
                            }
                            else
                            {
                                this.WindowState = FormWindowState.Maximized;
                                this.FormBorderStyle = FormBorderStyle.Sizable;
                                this.TopMost = false;
                                Cursor.Show();
                            }
                            formResize(null, EventArgs.Empty);

                            formResize(null, EventArgs.Empty);
                        }


                        lookahead = Convert.ToInt32(item["LookAhead"].InnerText);
                        listingInterval = Convert.ToInt32(item["GetListingsInverval"].InnerText);

                        title.Text = titletext;
                        title.Width = topMask.Width - (textMargin * 2);
                        title.Left = textMargin;
                        title.Top = textMargin;
                        title.Height = topMask.Height - (textMargin * 2);
                        title.Font = font;
                        title.ForeColor = gridForeground;
                        title.TextAlign = ContentAlignment.BottomCenter;
                        topMask.BackColor = background;
                        clockPanel.BackColor = timeBackground;
                        clock.ForeColor = gridForeground;
                        clock.Font = font;
                        this.BackColor = background;
                    }
                }
                
            }
            catch
            {
                //MessageBox.Show("Error reading settings from epg.xml");
            }
            if (fs != null)
            {
                fs.Close();
            }
            

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

    internal class Grid : Panel
    {
        public bool hasTimePanels = true;
        public Panel newCurrentTimeSlot;
        public Panel newPlus30TimeSlot;
        public Panel newPlus60TimeSlot;

    }
}
