////////////////////////////////////////////////////////////////////////////////// 
//                                                                              //
//      Copyright (C) 2005-2016 nzsjb                                           //
//                                                                              //
//  This Program is free software; you can redistribute it and/or modify        //
//  it under the terms of the GNU General Public License as published by        //
//  the Free Software Foundation; either version 2, or (at your option)         //
//  any later version.                                                          //
//                                                                              //
//  This Program is distributed in the hope that it will be useful,             //
//  but WITHOUT ANY WARRANTY; without even the implied warranty of              //
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                //
//  GNU General Public License for more details.                                //
//                                                                              //
//  You should have received a copy of the GNU General Public License           //
//  along with GNU Make; see the file COPYING.  If not, write to                //
//  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.       //
//  http://www.gnu.org/copyleft/gpl.html                                        //
//                                                                              //  
//////////////////////////////////////////////////////////////////////////////////

using System.Collections.ObjectModel;
using System.IO;

using DomainObjects;

namespace ChannelUpdate
{
    class DVBLinkTVSourceSettings
    {
        internal Collection<DVBLinkTVSourceSetting> Settings { get; set; }

        internal DVBLinkTVSourceSettings() { }

        internal bool Load(string installPath)
        {
            string path = Path.Combine(installPath, "sources");

            DirectoryInfo baseDirectory = new DirectoryInfo(path);

            foreach (DirectoryInfo directory in baseDirectory.GetDirectories())
            {
                DVBLinkTVSourceSetting setting = new DVBLinkTVSourceSetting();
                bool settingReply = setting.Load(directory.FullName);
                if (settingReply)
                {
                    if (Settings == null)
                        Settings = new Collection<DVBLinkTVSourceSetting>();
                    Settings.Add(setting);
                }
            }

            return (true);
        }

        internal bool Unload()
        {
            if (Settings != null)
            {
                foreach (DVBLinkTVSourceSetting setting in Settings)
                {
                    bool unloaded = setting.Unload();
                    if (!unloaded)
                        return (false);
                }
            }

            return (true);
        }

        internal void Clear()
        {
            if (Settings == null)
                return;

            foreach (DVBLinkTVSourceSetting sourceSetting in Settings)
                sourceSetting.Clear();

            Logger.Instance.Write("TVSource settings cleared");
        }
    }
}
