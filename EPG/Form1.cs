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
using System.Linq.Expressions;

namespace EPG
{
    public partial class Form1 : Form
    {
        private DateTime curTime = new DateTime();
        private DateTime currentTimeSlot = new DateTime();
        private DateTime nextTimeSlot = new DateTime();
        private DateTime secondTimeSlot = new DateTime();
        private Panel gridTimeCurrent = new Box();
        private Panel gridTimePlus30 = new Box();
        private Panel gridTimePlus60 = new Box();
        private Panel topRow = new Panel();
        private readonly Box clockPanel = new Box();
        private readonly Label clock = new OutlineLabel();
        private readonly Label title = new OutlineLabel();
        private IEnumerable<TVStation> stations;
        private Panel grids = new Panel();
        private string runReference;
        private Mutex cancelMutex;
        private int gridMargin = 25;
        private int gridVerticalStart = 240;
        private int rowHeight = 34;
        private int lookahead = 0;
        private Color background;
        private Color timeBackground;
        private Color timeForeground;
        private Color gridBackground;
        private Color gridForeground;
        private Color endBackground;
        private Color endForeground;
        private Font font = new Font("Consolas",14);
        private string titletext = "";
        private string endText = "";
        private string endLogo = "";
        private float speed = 1;
        private int listingInterval = 0;
        private int BoxBorderSize = 0;
        private float FontOutlineSize = 0;
        private Control pauseatbox;
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
            topRow.BorderStyle = BorderStyle.None;
            clockPanel.Top = 0;
            clockPanel.Left = 0;
            clockPanel.Height = rowHeight;
            clock.Width = clockPanel.Width - (BoxBorderSize * 2);
            clock.Left = BoxBorderSize;
            clock.Height = clockPanel.Height;
            clock.Top = 0;
            clock.TextAlign = ContentAlignment.MiddleRight;
            clock.Font = font;
            clock.ForeColor = gridForeground;
            clockPanel.Visible = true;
            clockPanel.BackColor = timeBackground;
            clockPanel.BorderStyle = BorderStyle.None;
            clockPanel.BorderSize = BoxBorderSize;
            clockPanel.BorderColor = gridForeground;
            clockPanel.Controls.Add(clock);
            topRow.Controls.Add(clockPanel);
            this.Controls.Add(topRow);

            grids.Top = topRow.Bottom;
            grids.Left = gridMargin;
            grids.Width = this.Width - (gridMargin * 2);
            grids.Height = this.Height - topRow.Bottom;
            grids.BorderStyle = BorderStyle.None;
            this.Controls.Add(grids);


            formResize(null, null);
            
            Task.Run(() => GetGuideData());
            timer1.Start();
            title.Top = 0;
            title.Left = 0;
            title.Width = this.Width;
            title.Height = gridVerticalStart - (BoxBorderSize * 2);
            title.Text = "Loading Data\r\nPlease Wait...";
            title.TextAlign = ContentAlignment.MiddleCenter;
            this.Controls.Add(title);

            
        }
        Box nextCurrent = null;
        Box nextP30 = null;
        Box nextP60 = null;
        private Grid nextGrid;
        private Stopwatch stopwatch;
        protected void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            int frames = 1;
            if (stopwatch == null)
            {
                stopwatch = new Stopwatch();
                stopwatch.Start();
            }
            else
            {
                frames = (int)(stopwatch.ElapsedMilliseconds / 16.667);
                stopwatch.Restart();
            }
            curTime = DateTime.Now;
            clock.Text = curTime.ToString("h:mm:ss");
            foreach (Grid grid in grids.Controls)
            {
                if (grid.Bottom < 0)
                {
                    foreach (Control control in grid.Controls)
                    {
                        control.Dispose();
                    }
                    grid.Controls.Clear();
                    grids.Controls.Remove(grid);
                    grid.Dispose();
                }
            }
            if (paused) 
            {
                if (curTime > pauseuntil)
                    paused = false;
                bool foundnextpause = false;
                foreach (Grid grid in grids.Controls)
                {
                    
                    if (grid.Bottom > grids.Height && grid.Top < 0) // grid is visible
                    {
                        foreach (Control box in grid.Controls)
                        {
                        
                            if (box.Bottom + grid.Top > grids.Height && box.Top + grid.Top > 0) // row is partially visible
                            {
                                if (!foundnextpause& box.GetType() == typeof(Box))
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
                            pauseatbox = grid.Controls[grid.Controls.Count - 1];
                        }
                    }
                    else if (grid.Top < grids.Height && !foundnextpause)
                    {
                        foreach (Control box in grid.Controls)
                        {

                            if (box.Bottom + grid.Top > grids.Height && box.Top + grid.Top > 0)
                            {
                                if (!foundnextpause && box.GetType() == typeof(Box))
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
                            pauseatbox = grid.Controls[grid.Controls.Count - 1]; 
                        }
                    }
                    else if (!foundnextpause)
                    {
                        pauseatgrid = grid;
                        pauseatbox = grid.Controls[6];
                    }
                }

                if (grids.Controls.Count == 0 || grids.Controls[grids.Controls.Count - 1].Bottom <= grids.Height * 2)
                {
                    generateGrid();

                }



            }
         
            else {

                foreach (Grid grid in grids.Controls)
                {
                    int speed = (int)(frames * this.speed);
                    grid.Top -= speed;
                    
                    
                    if (grid.newCurrentTimeSlot != null)
                    {
                        if (grid.Top < 0)
                        {
                            if (nextGrid != grid)
                            {
                                nextGrid = grid;
                                nextCurrent = new Box()
                                {
                                    Width = grid.newCurrentTimeSlot.Width,
                                    Height = grid.newCurrentTimeSlot.Height,
                                    Left = clockPanel.Right,
                                    Top = topRow.Height,
                                    BackColor = timeBackground,
                                    BorderSize = BoxBorderSize,
                                    BorderColor = gridForeground
                                };
                                topRow.Controls.Add(nextCurrent);
                                nextP30 = new Box()
                                {
                                    Width = grid.newPlus30TimeSlot.Width,
                                    Height = grid.newPlus30TimeSlot.Height,
                                    Left = nextCurrent.Right,
                                    Top = topRow.Height,
                                    BackColor = timeBackground,
                                    BorderSize = BoxBorderSize,
                                    BorderColor = gridForeground
                                };
                                topRow.Controls.Add(nextP30);
                                nextP60 = new Box()
                                {
                                    Width = grid.newPlus60TimeSlot.Width,
                                    Height = grid.newPlus60TimeSlot.Height,
                                    Left = nextP30.Right,
                                    Top = topRow.Height,
                                    BackColor = timeBackground,
                                    BorderSize = BoxBorderSize,
                                    BorderColor = gridForeground
                                };
                                topRow.Controls.Add(nextP60);
                                var ncLabel = grid.newCurrentTimeSlot.Controls[0] as Label;
                                var np30Label = grid.newPlus30TimeSlot.Controls[0] as Label;
                                var np60Label = grid.newPlus60TimeSlot.Controls[0] as Label;
                                nextCurrent.Controls.Add(new OutlineLabel()
                                {
                                    Top = BoxBorderSize,
                                    Left = BoxBorderSize,
                                    Height = nextCurrent.Height - (BoxBorderSize * 2),
                                    Width = nextCurrent.Width - (BoxBorderSize * 2),
                                    Text = ncLabel.Text,
                                    ForeColor = timeForeground,
                                    BackColor = timeBackground,
                                    Font = ncLabel.Font,
                                    TextAlign = ContentAlignment.MiddleCenter
                                });
                                nextP30.Controls.Add(new OutlineLabel()
                                {
                                    Top = BoxBorderSize,
                                    Left = BoxBorderSize,
                                    Height = nextP30.Height - (BoxBorderSize * 2),
                                    Width = nextP30.Width - (BoxBorderSize * 2),
                                    Text = np30Label.Text,
                                    ForeColor = timeForeground,
                                    BackColor = timeBackground,
                                    Font = np30Label.Font,
                                    TextAlign = ContentAlignment.MiddleCenter
                                });
                                nextP60.Controls.Add(new OutlineLabel()
                                {
                                    Top = BoxBorderSize,
                                    Left = BoxBorderSize,
                                    Height = nextP60.Height - (BoxBorderSize * 2),
                                    Width = nextP60.Width - (BoxBorderSize * 2),
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
                        }

                        if (grid.Top <= -grid.newCurrentTimeSlot.Height)
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
                            //grid.Controls.Remove(grid.newCurrentTimeSlot);
                            //grid.Controls.Remove(grid.newPlus30TimeSlot);
                            //grid.Controls.Remove(grid.newPlus60TimeSlot);
                            grid.newCurrentTimeSlot = null;
                            grid.newPlus30TimeSlot = null;
                            grid.newPlus60TimeSlot = null;
                            gridTimeCurrent.Top = clockPanel.Top;
                            gridTimePlus30.Top = clockPanel.Top;
                            gridTimePlus60.Top = clockPanel.Top;
                            gridTimeCurrent.Left = clockPanel.Width;
                            gridTimePlus30.Left = gridTimeCurrent.Right;
                            gridTimePlus60.Left = gridTimePlus30.Right;
                            //topRow.Controls.Add(gridTimeCurrent);
                            //topRow.Controls.Add(gridTimePlus30);
                            //topRow.Controls.Add(gridTimePlus60);
                            //gridTimeCurrent.BringToFront();
                            //gridTimePlus30.BringToFront();
                            //gridTimePlus60.BringToFront();
                            

                            pauseatbox = grid.Controls[6];
                        }

                    }

                }

                if (doneInit && dataloaded)
                {
                    if (grids.Controls.Count < 1)
                    {
                        generateGrid();
                    }
                    

                    if (pauseatgrid != null && pauseatbox != null)
                    {
                        if (pauseatbox.Top + pauseatgrid.Top <= 0)
                        {

                            pauseuntil = curTime.AddSeconds(pauselength);
                            paused = true;
                            GetSettings();

                        }
                    }
                    
                }
                
                
                
            }
                
                 

                
            doneInit = true;

            timer1.Start();
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
                Width = grids.Width
            };
            

            Box newCurrentTimeSlot = new Box();
            Box newPlus30TimeSlot = new Box();
            Box newPlus60TimeSlot = new Box();
            Label newCurrentTSLabel = new OutlineLabel();
            Label newPlus30TSLabel = new OutlineLabel();
            Label newPlus60TSLabel = new OutlineLabel();

            curTime = DateTime.Now;
            currentTimeSlot = RoundDown(curTime+TimeSpan.FromMinutes(lookahead), TimeSpan.FromMinutes(30));
            nextTimeSlot = currentTimeSlot + TimeSpan.FromMinutes(30);
            secondTimeSlot = currentTimeSlot + TimeSpan.FromMinutes(60);
            var timeslotwidth = (grid.Width - clockPanel.Width) / 3;
            Panel blank = new Box
            {
                Top = 0,
                Width = clockPanel.Width,
                Height = clockPanel.Height,
                Left = 0,
                BackColor = gridBackground,
                BorderStyle = BorderStyle.None,
                BorderSize = BoxBorderSize,
                BorderColor = gridForeground
            };
            grid.Controls.Add(blank);

            newCurrentTimeSlot.Top = blank.Top;
            newCurrentTimeSlot.Left = blank.Right;
            newCurrentTimeSlot.Width = timeslotwidth;
            newCurrentTimeSlot.Height = clockPanel.Height;
            newCurrentTimeSlot.BorderSize = BoxBorderSize;
            newCurrentTimeSlot.BorderColor = gridForeground;
            newCurrentTSLabel.Width = newCurrentTimeSlot.Width - (BoxBorderSize * 2);
            newCurrentTSLabel.Left = BoxBorderSize;
            newCurrentTSLabel.Height = newCurrentTimeSlot.Height - (BoxBorderSize * 2);
            newCurrentTSLabel.Top = BoxBorderSize;
            newCurrentTSLabel.TextAlign = ContentAlignment.MiddleCenter;
            newCurrentTSLabel.Font = font;
            newCurrentTSLabel.ForeColor = timeForeground;
            newCurrentTSLabel.Text = currentTimeSlot.ToString("h:mm tt");
            newCurrentTimeSlot.Visible = true;
            newCurrentTimeSlot.BackColor = gridBackground;
            newCurrentTimeSlot.BorderStyle = BorderStyle.None;
            newCurrentTimeSlot.Controls.Add(newCurrentTSLabel);
            grid.Controls.Add(newCurrentTimeSlot);

            newPlus30TimeSlot.Top = blank.Top;
            newPlus30TimeSlot.Left = newCurrentTimeSlot.Right;
            newPlus30TimeSlot.Width = timeslotwidth;
            newPlus30TimeSlot.Height = newCurrentTimeSlot.Height;
            newPlus30TimeSlot.BorderSize = BoxBorderSize;
            newPlus30TimeSlot.BorderColor = gridForeground;
            newPlus30TSLabel.Width = newPlus30TimeSlot.Width - (BoxBorderSize * 2);
            newPlus30TSLabel.Left = BoxBorderSize;
            newPlus30TSLabel.Height = newPlus30TimeSlot.Height - (BoxBorderSize * 2);
            newPlus30TSLabel.Top = BoxBorderSize;
            newPlus30TSLabel.TextAlign = ContentAlignment.MiddleCenter;
            newPlus30TSLabel.Font = font;
            newPlus30TSLabel.ForeColor = timeForeground;
            newPlus30TSLabel.Text = nextTimeSlot.ToString("h:mm tt");
            newPlus30TimeSlot.Visible = true;
            newPlus30TimeSlot.BackColor = gridBackground;
            newPlus30TimeSlot.BorderStyle = BorderStyle.None;
            newPlus30TimeSlot.Controls.Add(newPlus30TSLabel);
            grid.Controls.Add(newPlus30TimeSlot);

            newPlus60TimeSlot.Top = blank.Top;
            newPlus60TimeSlot.Left = newPlus30TimeSlot.Right;
            newPlus60TimeSlot.Width = timeslotwidth;
            newPlus60TimeSlot.Height = newPlus30TimeSlot.Height;
            newPlus60TimeSlot.BorderSize = BoxBorderSize;
            newPlus60TimeSlot.BorderColor = gridForeground;
            newPlus60TSLabel.Width = newPlus60TimeSlot.Width - (BoxBorderSize * 2);
            newPlus60TSLabel.Left = BoxBorderSize;
            newPlus60TSLabel.Height = newPlus60TimeSlot.Height - (BoxBorderSize * 2);
            newPlus60TSLabel.Top = BoxBorderSize;
            newPlus60TSLabel.TextAlign = ContentAlignment.MiddleCenter;
            newPlus60TSLabel.Font = font;
            newPlus60TSLabel.ForeColor = timeForeground;
            newPlus60TSLabel.Text = secondTimeSlot.ToString("h:mm tt");
            newPlus60TimeSlot.Visible = true;
            newPlus60TimeSlot.BackColor = gridBackground;
            newPlus60TimeSlot.BorderStyle = BorderStyle.None;
            newPlus60TimeSlot.Controls.Add(newPlus60TSLabel);
            grid.Controls.Add(newPlus60TimeSlot);

            //newCurrentTimeSlot.BringToFront();
            //newPlus30TimeSlot.BringToFront();
            //newPlus60TimeSlot.BringToFront();

            grid.newCurrentTimeSlot = newCurrentTimeSlot;
            grid.newPlus30TimeSlot = newPlus30TimeSlot;
            grid.newPlus60TimeSlot = newPlus60TimeSlot;
            //gridBottom = newPlus60TimeSlot.Bottom;

            var blank2 = new Box
            {
                Top = newPlus60TimeSlot.Bottom,
                Width = clockPanel.Width,
                Height = clockPanel.Height,
                Left = 0,
                BackColor = gridBackground,
                BorderStyle = BorderStyle.None,
                BorderSize = BoxBorderSize,
                BorderColor = gridForeground
            };
            grid.Controls.Add(blank2);

            Box datePanel = new Box();
            Label dateLabel = new OutlineLabel();
            datePanel.Top = newPlus60TimeSlot.Bottom;
            datePanel.Width = timeslotwidth * 3;
            datePanel.Height = clockPanel.Height;
            datePanel.Left = blank.Right;
            datePanel.BackColor = gridBackground;
            datePanel.BorderStyle = BorderStyle.None;
            datePanel.BorderSize = BoxBorderSize;
            datePanel.BorderColor = gridForeground;
            dateLabel.Top = BoxBorderSize;
            dateLabel.Left = BoxBorderSize  ;
            dateLabel.Height = datePanel.Height - (BoxBorderSize * 2);
            dateLabel.Width = datePanel.Width - (BoxBorderSize * 2);
            dateLabel.Font = font;
            dateLabel.ForeColor = timeForeground;
            dateLabel.TextAlign = ContentAlignment.MiddleCenter;
            dateLabel.Text = DateTime.Now.ToString("dddd MMMM d yyyy");
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
                Box channelPanel = new Box();
                Label channelNum = new OutlineLabel();
                Label channelName = new OutlineLabel();

                channelPanel.Left = 0;
                channelPanel.Width = clockPanel.Width;
                channelPanel.Height = rowHeight * 2;
                channelPanel.Top = gridBottom;
                channelPanel.BackColor = gridBackground;
                channelPanel.BorderStyle = BorderStyle.None;
                channelPanel.BorderSize = BoxBorderSize;
                channelPanel.BorderColor = gridForeground;
                channelNum.AutoSize = false;
                channelNum.Top = BoxBorderSize;
                channelNum.Left = BoxBorderSize;
                channelNum.Width = channelPanel.Width - (BoxBorderSize * 2);
                channelNum.Height = (channelPanel.Height / 2) - BoxBorderSize;
                channelNum.Font = font;
                channelNum.ForeColor = timeForeground;
                channelNum.TextAlign = ContentAlignment.MiddleCenter;
                if (channel.MinorChannelNumber > -1)
                    channelNum.Text = channel.TransportStreamID.ToString() + "." + channel.MinorChannelNumber.ToString();
                else if (channel.LogicalChannelNumber > -1)
                    channelNum.Text = channel.LogicalChannelNumber.ToString();
                else
                    channelNum.Text = channel.TransportStreamID.ToString();
                channelNum.TextAlign = ContentAlignment.MiddleCenter;
                channelName.AutoSize = false;
                channelName.Top = channelNum.Bottom;
                channelName.Left = BoxBorderSize;
                channelName.Width = channelPanel.Width - (BoxBorderSize * 2);
                channelName.Height = (channelPanel.Height / 2) - BoxBorderSize;
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
                            Box programPanel = new Box() {  BorderSize = BoxBorderSize, BorderColor = gridForeground };
                            Label programLabel = new Label()
                            {
                                Text = program.EventName
                            };
                            if (programStartTime < currentTimeSlot && programEndTime > secondTimeSlot + new TimeSpan(0, 30, 0))
                            {
                                programPanel.Left = channelPanel.Right;
                                programPanel.Width = timeslotwidth * 3;
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
                                programPanel.Width = Convert.ToInt32(programEndTime.Subtract(currentTimeSlot).TotalMinutes) * timeslotwidth / 30;
                                programLabel.Text = "";
                                if (programStartTime < currentTimeSlot - contThreshold)
                                    programLabel.Text = "< ";
                                programLabel.Text += program.EventName;
                            }
                            else if (programEndTime > secondTimeSlot + new TimeSpan(0,30,0))
                            {
                                programPanel.Left = channelPanel.Right + Convert.ToInt32(programStartTime.Subtract(currentTimeSlot).TotalMinutes) * timeslotwidth / 30;
                                programPanel.Width = Convert.ToInt32(secondTimeSlot.AddMinutes(30).Subtract(programStartTime).TotalMinutes) * timeslotwidth / 30;
                                programLabel.Text = program.EventName;
                                if (programEndTime > secondTimeSlot + new TimeSpan(0, 30, 0) + contThreshold)
                                    programLabel.Text += " >";
                            }
                            else
                            {
                                programPanel.Left = channelPanel.Right + Convert.ToInt32(programStartTime.Subtract(currentTimeSlot).TotalMinutes) * timeslotwidth / 30;
                                programPanel.Width = Convert.ToInt32(programEndTime.Subtract(programStartTime).TotalMinutes) * timeslotwidth / 30;
                            }

                            programPanel.Top = channelPanel.Top;
                            programPanel.Height = channelPanel.Height;
                            programPanel.BackColor = gridBackground;
                            programPanel.SendToBack();
                            programPanel.BorderStyle = BorderStyle.None;
                            programLabel.Font = font;
                            programLabel.ForeColor = gridForeground;
                            programLabel.Left = BoxBorderSize;
                            programLabel.Width = programPanel.Width - (BoxBorderSize * 2);
                            programLabel.Top = BoxBorderSize;
                            programLabel.Height = programPanel.Height - (BoxBorderSize * 2);
                            programLabel.TextAlign = ContentAlignment.TopLeft;
                            programPanel.Controls.Add(programLabel);
                                        
                            programLabel.UseMnemonic = false;
                            grid.Controls.Add(programPanel);
                            noprograms = false;
                        }
                            
                    }
                    if (noprograms)
                    {
                        Box staticPanel = new Box() { BorderSize = BoxBorderSize, BorderColor = gridForeground };
                        Label staticLabel = new OutlineLabel();
                        staticPanel.Width = timeslotwidth * 3;
                        staticPanel.Height = channelPanel.Height;
                        staticPanel.Top = channelPanel.Top;
                        staticPanel.Left = channelPanel.Right;
                        staticPanel.BackColor = gridBackground;
                        staticPanel.BorderStyle = BorderStyle.None;
                        staticPanel.SendToBack();
                        staticLabel.Width = staticPanel.Width - (BoxBorderSize * 2);
                        staticLabel.Height = staticPanel.Height - (BoxBorderSize * 2);
                        staticLabel.Left = BoxBorderSize;
                        staticLabel.Top = BoxBorderSize;
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
                    Panel staticPanel = new Box() { BorderSize = BoxBorderSize, BorderColor = gridForeground };
                    Label staticLabel = new OutlineLabel();
                    staticPanel.Width = channelPanel.Width * 3;
                    staticPanel.Height = channelPanel.Height;
                    staticPanel.Top = channelPanel.Top;
                    staticPanel.Left = channelPanel.Right;
                    staticPanel.BackColor = gridBackground;
                    staticPanel.BorderStyle = BorderStyle.None;
                    staticPanel.SendToBack();
                    staticLabel.Width = staticPanel.Width - (BoxBorderSize * 2);
                    staticLabel.Height = staticPanel.Height - (BoxBorderSize * 2);
                    staticLabel.Left = BoxBorderSize;
                    staticLabel.Top = BoxBorderSize;
                    staticLabel.ForeColor = gridForeground;
                    staticLabel.Font = font;
                    staticLabel.TextAlign = ContentAlignment.TopLeft;
                    staticLabel.Text = "Data Unavailable";
                    staticPanel.Controls.Add(staticLabel);
                    grid.Controls.Add(staticPanel);
                }
                
                gridBottom = channelPanel.Bottom;
            }
            Panel endPanel = new Box() { BorderSize = BoxBorderSize, BorderColor = gridForeground };
            Label endLabel = new OutlineLabel();
            endPanel.Width = (grid.Width / 4) * 4;
            endPanel.Height = rowHeight;
            endPanel.Top = gridBottom;
            endPanel.Left = 0;
            endPanel.BackColor = endBackground;
            endPanel.BorderStyle = BorderStyle.None;
            endPanel.SendToBack();
            endLabel.Width = endPanel.Width - (BoxBorderSize * 2);
            endLabel.Height = endPanel.Height - (BoxBorderSize * 2);
            endLabel.Left = BoxBorderSize;
            endLabel.Top = BoxBorderSize;
            endLabel.ForeColor = endForeground;
            endLabel.Font = font;
            endLabel.TextAlign = ContentAlignment.MiddleCenter;
            endLabel.Text = endText;
            endPanel.Controls.Add(endLabel);
            gridBottom = endPanel.Bottom;
            grid.Controls.Add(endPanel);

            title.Text = titletext;

            if (!string.IsNullOrEmpty(endLogo))
            {
                PictureBox logo = new PictureBox();
                logo.ImageLocation = endLogo;
                try
                {
                    logo.Load();
                }
                catch
                {

                }
                if (logo.Image != null)
                {
                    var ar = ((double)(logo.Image.Width) / logo.Image.Height);
                    logo.Left = 0;
                    logo.Width = (grid.Width / 4) * 4;
                    logo.Height = (int)(logo.Width / ar);
                    logo.Top = gridBottom;
                    logo.SizeMode = PictureBoxSizeMode.Zoom;
                    gridBottom = logo.Bottom;
                    grid.Controls.Add(logo);
                }
            }
            
            grid.Height = gridBottom;
            grid.SendToBack();
            
            if (grids.Controls.Count > 0)
                grid.Top = grids.Controls[grids.Controls.Count - 1].Bottom;
            else
                grid.Top = 0;
            grids.Controls.Add(grid);
            if (pauseatgrid == null)
            {
                pauseatgrid = grid;
                pauseatbox = grid.Controls[6];
            }

            if (grids.Controls[grids.Controls.Count - 1].Bottom <= grids.Height * 2)
            {
                generateGrid();

            }

        }

        private void formResize(object sender, EventArgs e)
        {
            //create clock
            topRow.Top = gridVerticalStart;
            topRow.Left = gridMargin;
            topRow.Width = this.Width - (gridMargin * 2);
            topRow.Height = rowHeight;
            clock.AutoSize = false;
            clockPanel.Height = rowHeight;
            clock.Width = clockPanel.Width - (BoxBorderSize * 2);
            clock.Left = BoxBorderSize;
            clock.Height = clockPanel.Height - (BoxBorderSize * 2);
            clock.Top = BoxBorderSize;
            clock.TextAlign = ContentAlignment.MiddleRight;
            clock.Font = font;
            clock.ForeColor = gridForeground;
            clockPanel.Visible = true;
            clockPanel.BackColor = timeBackground;
            clockPanel.BorderStyle = BorderStyle.None;
            clockPanel.Controls.Add(clock);
            gridTimeCurrent.Top = this.Height;
            grids.Top = topRow.Bottom;
            grids.Left = gridMargin;
            grids.Width = this.Width - (gridMargin * 2);
            grids.Height = this.Height - topRow.Bottom;
            title.Width = this.Width;
            title.Top = 0;
            title.Height = gridVerticalStart - (BoxBorderSize * 2);

            foreach (Panel grid in grids.Controls)
            {
                grids.Controls.Remove(grid);
            }
            grids.Controls.Clear();
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
                stations = replaceMissingChannels(RunParameters.Instance.StationCollection.ToList());
                dataloaded = true;
                Thread.Sleep(listingInterval * 1000);
            } while (true);
        }

        private IEnumerable<TVStation> replaceMissingChannels(List<TVStation> newList)
        {
            stations.ToList().ForEach(station =>
            {
                if (station.MinorChannelNumber > 0 && !newList.Any(newStation => newStation.OriginalNetworkID == station.OriginalNetworkID))
                {
                    newList.AddRange(stations.Where(s => s.OriginalNetworkID == station.OriginalNetworkID));
                }
            });
            return newList;
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
                        speed = (float)Convert.ToDouble(item["Speed"].InnerText);
                        pauselength = Convert.ToInt32(item["PauseLength"].InnerText);

                        int tempgridmargin = Convert.ToInt32(item["GridMargin"].InnerText);
                        int tempverticalstart = Convert.ToInt32(item["GridVerticalStart"].InnerText);
                        int temprowheight = Convert.ToInt32(item["RowHeight"].InnerText);
                        int tempbordersize = Convert.ToInt32(item["BoxBorderSize"].InnerText);
                        clockPanel.Width = Convert.ToInt32(item["ClockWidth"].InnerText);
                        float tempfontoutlinesize = (float)Convert.ToDouble(item["FontOutlineSize"].InnerText);
                        string tempLogo = item["LogoPath"].InnerText;
                        
                        bool fullscreen = Convert.ToBoolean(item["Fullscreen"].InnerText);

                        if (tempgridmargin != gridMargin || tempverticalstart != gridVerticalStart || temprowheight != rowHeight || (this.FormBorderStyle == FormBorderStyle.None) != fullscreen||
                            BoxBorderSize != tempbordersize || FontOutlineSize != tempfontoutlinesize || endLogo != tempLogo)
                        {
                            gridMargin = tempgridmargin;
                            gridVerticalStart = tempverticalstart;
                            rowHeight = temprowheight;
                            BoxBorderSize = tempbordersize;
                            FontOutlineSize = tempfontoutlinesize;
                            endLogo = tempLogo;
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
                        title.Width = this.Width - (BoxBorderSize * 2);
                        title.Left = BoxBorderSize;
                        title.Top = this.Top;
                        title.Height = gridVerticalStart - (BoxBorderSize * 2);
                        title.Font = font;
                        title.ForeColor = gridForeground;
                        title.TextAlign = ContentAlignment.BottomCenter;
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

    
}
