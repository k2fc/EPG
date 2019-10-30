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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using DomainObjects;
using DVBServices;
using System.Linq;

namespace EPG
{
    public partial class Form1 : Form
    {
        DateTime curTime = new DateTime();
        DateTime currentTimeSlot = new DateTime();
        DateTime nextTimeSlot = new DateTime();
        DateTime secondTimeSlot = new DateTime();
        Panel gridTimeCurrent = new Panel();
        Panel gridTimePlus30 = new Panel();
        Panel gridTimePlus60 = new Panel();
        Panel newCurrentTimeSlot = new Panel();
        Panel newPlus30TimeSlot = new Panel();
        Panel newPlus60TimeSlot = new Panel();
        Panel topMask = new Panel();
        Panel clockPanel = new Panel();
        Label gridTimeCurrentLabel = new Label();
        Label gridTimePlus30Label = new Label();
        Label gridTimePlus60Label = new Label();
        Label clock = new Label();
        Label title = new Label();

        List<Control> grid = new List<Control>();
        

        int gridMargin = 25;
        int gridVerticalStart = 240;
        int rowHeight = 34;
        int textMargin = 3;
        int lookahead = 0;
        Color background;
        Color timeBackground;
        Color timeForeground;
        Color gridBackground;
        Color gridForeground;
        Color endBackground;
        Color endForeground;
        Font font = new Font("Consolas",14);
        String titletext = "";
        String endText = "";

        int gridBottom = 0;
        int speed = 1;

        int pauseat =6;
        int pauselength = 2;
        DateTime pauseuntil;
        bool paused = false;

        bool doneInit = false;

        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(640, 480);
            getSettings();

            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
            Cursor.Hide();
            this.TopMost = true;


            //create clock
            clockPanel.Top = gridVerticalStart;
            clockPanel.Left = gridMargin;
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
            this.Controls.Add(clockPanel);

            //create the mask that hides the times as they get pushed off
            topMask.BackColor = background;
            topMask.Top = 0;
            topMask.Height = clockPanel.Top;
            topMask.Left = clockPanel.Left;
            topMask.Width = this.Width - (gridMargin * 2);
            topMask.Controls.Add(title);
            this.Controls.Add(topMask);

            formResize(null, null);
            timer1.Start();
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            curTime = DateTime.Now;
            clock.Text = curTime.ToString("h:mm:ss");

            if (gridBottom <= clockPanel.Bottom)
            {
                generateGrid();
            }
            
            if (paused)
            {
                if (curTime > pauseuntil)
                    paused = false;
                bool foundnextpause = false;
                foreach (Control box in grid)
                {
                    if (box.Bottom > this.Height && box.Top > clockPanel.Bottom)
                    {
                        if (!foundnextpause)
                            pauseat = grid.IndexOf(box);
                        foundnextpause = true;
                    }
                }
                if (!foundnextpause)
                    pauseat = 6;
            }
         
            else {

                gridBottom = (from box in grid select box.Bottom).Max();
                grid.All(box => { box.Top-= speed; return true; });

                if (newCurrentTimeSlot != null)
                {
                    if (newCurrentTimeSlot.Top < gridTimeCurrent.Bottom)
                    {
                        gridTimeCurrent.Top = gridTimeCurrent.Top - speed;
                        gridTimePlus30.Top = gridTimePlus30.Top - speed;
                        gridTimePlus60.Top = gridTimePlus60.Top - speed;
                        gridTimeCurrent.SendToBack();
                        gridTimePlus30.SendToBack();
                        gridTimePlus60.SendToBack();
                        
                    }

                    if (newCurrentTimeSlot.Bottom <= clockPanel.Bottom)
                    {
                        this.Controls.Remove(gridTimeCurrent);
                        this.Controls.Remove(gridTimePlus30);
                        this.Controls.Remove(gridTimePlus60);
                        gridTimeCurrent = newCurrentTimeSlot;
                        gridTimePlus30 = newPlus30TimeSlot;
                        gridTimePlus60 = newPlus60TimeSlot;
                        grid.Remove(newCurrentTimeSlot);
                        grid.Remove(newPlus30TimeSlot);
                        grid.Remove(newPlus60TimeSlot);
                        newCurrentTimeSlot = null;
                        newPlus30TimeSlot = null;
                        newPlus60TimeSlot = null;

                        gridTimeCurrent.Top = clockPanel.Top;
                        gridTimePlus30.Top = clockPanel.Top;
                        gridTimePlus60.Top = clockPanel.Top;
                    }

                }
                
                if (grid[pauseat].Top <= clockPanel.Bottom && grid[pauseat].Top >= clockPanel.Top)
                {
                    
                    pauseuntil = curTime.AddSeconds(pauselength);
                    paused = true;
                    getSettings();
                                        
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
            getSettings();
            foreach (Control box in grid)
            {
                if (box != gridTimeCurrent && box != gridTimePlus30 && box != gridTimePlus60)
                    this.Controls.Remove(box);
                
            }

            grid.Clear();
            newCurrentTimeSlot = new Panel();
            newPlus30TimeSlot = new Panel();
            newPlus60TimeSlot = new Panel();
            Label newCurrentTSLabel = new Label();
            Label newPlus30TSLabel = new Label();
            Label newPlus60TSLabel = new Label();

            curTime = DateTime.Now;
            currentTimeSlot = RoundDown(curTime+TimeSpan.FromMinutes(lookahead), TimeSpan.FromMinutes(30));
            nextTimeSlot = currentTimeSlot + TimeSpan.FromMinutes(30);
            secondTimeSlot = currentTimeSlot + TimeSpan.FromMinutes(60);

            Panel blank = new Panel();
            blank.Top = this.Height;
            blank.Width = clockPanel.Width;
            blank.Height = clockPanel.Height;
            blank.Left = clockPanel.Left;
            blank.BackColor = timeBackground;
            blank.BorderStyle = BorderStyle.Fixed3D;
            grid.Add(blank);

            newCurrentTimeSlot.Top = this.Height;
            newCurrentTimeSlot.Left = clockPanel.Right;
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
            newCurrentTimeSlot.BackColor = timeBackground;
            newCurrentTimeSlot.BorderStyle = BorderStyle.Fixed3D;
            newCurrentTimeSlot.Controls.Add(newCurrentTSLabel);
            grid.Add(newCurrentTimeSlot);

            newPlus30TimeSlot.Top = this.Height;
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
            newPlus30TimeSlot.BackColor = timeBackground;
            newPlus30TimeSlot.BorderStyle = BorderStyle.Fixed3D;
            newPlus30TimeSlot.Controls.Add(newPlus30TSLabel);
            grid.Add(newPlus30TimeSlot);

            newPlus60TimeSlot.Top = this.Height;
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
            newPlus60TimeSlot.BackColor = timeBackground;
            newPlus60TimeSlot.BorderStyle = BorderStyle.Fixed3D;
            newPlus60TimeSlot.Controls.Add(newPlus60TSLabel);
            grid.Add(newPlus60TimeSlot);
            gridBottom = newPlus60TimeSlot.Bottom;

            blank = new Panel();
            blank.Top = gridBottom;
            blank.Width = clockPanel.Width;
            blank.Height = clockPanel.Height;
            blank.Left = clockPanel.Left;
            blank.BackColor = gridBackground;
            blank.BorderStyle = BorderStyle.Fixed3D;
            grid.Add(blank);

            Panel datePanel = new Panel();
            Label dateLabel = new Label();
            datePanel.Top = gridBottom;
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
            grid.Add(datePanel);

            gridBottom = blank.Bottom;

            XmlDataDocument configFile = new XmlDataDocument();
            XmlNodeList files;

            XmlNodeList channels;
            FileStream fs = new FileStream("epg.xml", FileMode.Open, FileAccess.Read);
            configFile.Load(fs);
            fs.Close();
            files = configFile.GetElementsByTagName("XMLTVFile");
            Collection<TVStation> stations = new Collection<TVStation>();
            List<EPGEntry> entries = new List<EPGEntry>();
            RunParameters.BaseDirectory = "";
            RunParameters.Instance.ImportFiles = new Collection<ImportFileSpec>();
            foreach (XmlNode file in files)
            {
                ImportFileSpec fileSpec = new ImportFileSpec(file.Attributes["path"].Value);
                fileSpec.IdFormat = (XmltvIdFormat)Convert.ToInt32(file.Attributes["IdFormat"].Value);
                RunParameters.Instance.ImportFiles.Add(fileSpec);
            }
            EPGController.Instance.FinishRun();
            stations = RunParameters.Instance.StationCollection;
            //XmltvIdFormat.
            channels = configFile.GetElementsByTagName("channel");
            pauseat = grid.Count;
            foreach (XmlNode channel in channels)
            {
                Panel channelPanel = new Panel();
                Label channelNum = new Label();
                Label channelName = new Label();

                channelPanel.Left = clockPanel.Left;
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
                channelNum.Text = channel.Attributes["number"].Value;
                channelNum.TextAlign = ContentAlignment.MiddleCenter;
                channelName.Top = channelNum.Bottom;
                channelName.Left = 0;
                channelName.Width = channelPanel.Width - (textMargin * 2);
                channelName.Height = channelPanel.Height / 2;
                channelName.Font = font;
                channelName.ForeColor = timeForeground;
                channelName.Text = channel.Attributes["name"].Value;
                channelName.TextAlign = ContentAlignment.MiddleCenter;
                channelPanel.Controls.Add(channelNum);
                channelPanel.Controls.Add(channelName);
                channelPanel.SendToBack();
                grid.Add(channelPanel);

                title.Text = "Loading schedule for channel " + channel.Attributes["number"].Value;

                if (channel.Attributes["source"].Value == "static")
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
                    staticLabel.Text = channel.Attributes["text"].Value;
                    staticPanel.Controls.Add(staticLabel);
                    grid.Add(staticPanel);
                }
                else if (channel.Attributes["source"].Value == "XMLTV")
                {
                    try
                    {
                        TVStation station = null;
                        foreach (var channelrecord in stations)
                        {
                            if (channelrecord.ProviderName == channel.Attributes["channel_id"].Value)
                            {
                                station = channelrecord;
                                break;
                            }
                        }
                        if (channel.Attributes["name"].Value == "")
                        {
                            channelName.Text = station.Name;
                        }
                            
                        bool noprograms = true;
                        foreach (var program in station.EPGCollection)
                        {
                            DateTime programStartTime = program.StartTime.ToLocalTime();
                            DateTime programEndTime = programStartTime + program.Duration;

                            if (programStartTime < secondTimeSlot.AddMinutes(30) && programEndTime > currentTimeSlot)
                            {
                                Panel programPanel = new Panel();
                                Label programLabel = new Label();
                                programLabel.Text = program.EventName;
                                //int length = Convert.ToInt32(programEndTime.Subtract(programStartTime).TotalMinutes);
                                if (programStartTime < currentTimeSlot && programEndTime > secondTimeSlot.AddMinutes(30))
                                {
                                    programPanel.Left = clockPanel.Right;
                                    programPanel.Width = clockPanel.Width * 3;
                                    programLabel.Text = "< " + program.EventName + " >";
                                }
                                else if (programStartTime < currentTimeSlot)
                                {
                                    programPanel.Left = clockPanel.Right;
                                    programPanel.Width = Convert.ToInt32(programEndTime.Subtract(currentTimeSlot).TotalMinutes) * clockPanel.Width / 30;
                                    programLabel.Text = "< " + program.EventName;
                                }
                                else if (programEndTime > secondTimeSlot.AddMinutes(30))
                                {
                                    programPanel.Left = clockPanel.Right + Convert.ToInt32(programStartTime.Subtract(currentTimeSlot).TotalMinutes) * clockPanel.Width / 30;
                                    programPanel.Width = Convert.ToInt32(secondTimeSlot.AddMinutes(30).Subtract(programStartTime).TotalMinutes) * clockPanel.Width / 30;
                                    programLabel.Text = program.EventName + " >";
                                }
                                else
                                {
                                    programPanel.Left = clockPanel.Right + Convert.ToInt32(programStartTime.Subtract(currentTimeSlot).TotalMinutes) * clockPanel.Width / 30;
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
                                grid.Add(programPanel);
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
                            grid.Add(staticPanel);
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
                        grid.Add(staticPanel);
                    }
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
            grid.Add(endPanel);

            foreach (Control box in grid)
            {
                this.Controls.Add(box);
            }
            title.Text = titletext;

        }

        private void formResize(object sender, EventArgs e)
        {
            //create clock
            clockPanel.Top = gridVerticalStart;
            clockPanel.Left = gridMargin;
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


            //create the mask that hides the times as they get pushed off
            topMask.BackColor = background;
            topMask.Top = 0;
            topMask.Height = clockPanel.Top;
            topMask.Left = clockPanel.Left;
            topMask.Width = this.Width - (gridMargin * 2);


            if (doneInit)
                generateGrid();

        }

        private void getSettings()
        {
            FileStream fs = null;
            try
            {
                XmlDocument settingsfile = new XmlDocument();
                fs = new FileStream("epg.xml", FileMode.Open, FileAccess.Read);
                settingsfile.Load(fs);
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
                            
                            formResize(null, EventArgs.Empty);
                        }

                        lookahead = Convert.ToInt32(item["LookAhead"].InnerText);


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
    }
    
}
