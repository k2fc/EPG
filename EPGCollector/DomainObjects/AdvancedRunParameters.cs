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

namespace DomainObjects
{
    /// <summary>
    /// The class that describes the per frequency advanced parameters.
    /// </summary>
    public class AdvancedRunParameters
    {
        /// <summary>
        /// Get or set the name of the byte conversion table.
        /// </summary>
        public string ByteConvertTable { get; set; }
        /// <summary>
        /// Get or set the name of the EIT carousel.
        /// </summary>
        public string EITCarousel { get; set; }
        /// <summary>
        /// Get or set the number of days EPG to collect.
        /// </summary>
        public int EPGDays { get; set; }
        /// <summary>
        /// Get or set the country code.
        /// </summary>
        public string CountryCode { get; set; }        
        /// <summary>
        /// Get or set the bouquet (ie area).
        /// </summary>
        public int ChannelBouquet { get; set; }
        /// <summary>
        /// Get or set the region.
        /// </summary>
        public int ChannelRegion { get; set; }
        /// <summary>
        /// Get or set the character set.
        /// </summary>
        public string CharacterSet { get; set; }
        /// <summary>
        /// Get or set the EPG input language.
        /// </summary>
        public string InputLanguage { get; set; }
        /// <summary>
        /// Get or set the SDT pid to use.
        /// </summary>
        public int SDTPid { get; set; }
        /// <summary>
        /// Get or set the EIT pid to use.
        /// </summary>
        public int EITPid { get; set; }
        /// <summary>
        /// Get or set the MHW1 pids to use.
        /// </summary>
        public int[] MHW1Pids { get; set; }
        /// <summary>
        /// Get or set the MHW2 pids to use. 
        /// </summary>
        public int[] MHW2Pids { get; set; }
        /// <summary>
        /// Get or set the Dish Network pid to use.
        /// </summary>
        public int DishNetworkPid { get; set; }
        /// <summary>
        /// Get or set the list of options.
        /// </summary>
        public Collection<OptionEntry> Options { get; set; }

        /// <summary>
        /// Get or set the region.
        /// </summary>
        public int Region { get; set; }

        /// <summary>
        /// Initialize a new instance of the AdvancedRunParameters class.
        /// </summary>
        public AdvancedRunParameters() 
        {
            EPGDays = -1;
            ChannelBouquet = -1;
            ChannelRegion = -1;
            SDTPid = -1;
            EITPid = -1;            
            DishNetworkPid = -1;

            Options = new Collection<OptionEntry>();
        }

        /// <summary>
        /// Check these parameters for equality with another.
        /// </summary>
        /// <param name="oldParameters">The other set of parameters.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public bool EqualTo(AdvancedRunParameters oldParameters)
        {
            if (ByteConvertTable != oldParameters.ByteConvertTable)
                return (false);
            if (EITCarousel != oldParameters.EITCarousel)
                return (false);
            if (EPGDays != oldParameters.EPGDays)
                return (false);
            if (CountryCode != oldParameters.CountryCode)
                return (false);
            if (ChannelBouquet != oldParameters.ChannelBouquet)
                return (false);
            if (ChannelRegion != oldParameters.ChannelRegion)
                return (false);
            if (Region != oldParameters.Region)
                return (false);
            if (CharacterSet != oldParameters.CharacterSet)
                return (false);
            if (InputLanguage != oldParameters.InputLanguage)
                return (false);
            if (SDTPid != oldParameters.SDTPid)
                return (false);
            if (EITPid != oldParameters.EITPid)
                return (false);            

            if ((MHW1Pids == null && oldParameters.MHW1Pids != null) || (MHW1Pids != null && oldParameters.MHW1Pids == null))
                return (false);

            if (MHW1Pids != null)
            {
                if (MHW1Pids.Length != oldParameters.MHW1Pids.Length)
                    return (false);

                foreach (int newPid in MHW1Pids)
                {
                    bool found = false;
                    foreach (int oldPid in oldParameters.MHW1Pids)
                    {
                        if (oldPid == newPid)
                            found = true;
                    }
                    if (!found)
                        return (false);
                }
            }

            if ((MHW2Pids == null && oldParameters.MHW2Pids != null) || (MHW2Pids != null && oldParameters.MHW2Pids == null))
                return (false);

            if (MHW2Pids != null)
            {
                if (MHW2Pids.Length != oldParameters.MHW2Pids.Length)
                    return (false);

                foreach (int newPid in MHW2Pids)
                {
                    bool found = false;
                    foreach (int oldPid in oldParameters.MHW2Pids)
                    {
                        if (oldPid == newPid)
                            found = true;
                    }
                    if (!found)
                        return (false);
                }
            }

            if (DishNetworkPid != oldParameters.DishNetworkPid)
                return (false);

            if (Options != null)
            {
                if (oldParameters.Options == null || Options.Count != oldParameters.Options.Count)
                    return (false);

                foreach (OptionEntry optionEntry in Options)
                {
                    if (OptionEntry.FindEntry(oldParameters.Options, optionEntry.ToString()) == null)
                        return (false);
                }
            }
            else
            {
                if (oldParameters.Options != null)
                    return (false);
            }

            return (true);
        }

        /// <summary>
        /// Output the advanced parameters.
        /// </summary>
        /// <param name="streamWriter">The stream writer to use.</param>
        public void OutputParameters(StreamWriter streamWriter)
        {
            if (ByteConvertTable != null)
                streamWriter.WriteLine("ByteConvertTable=" + ByteConvertTable);

            if (EITCarousel != null)
                streamWriter.WriteLine("EITCarousel=" + EITCarousel);

            if (EPGDays != -1)
                streamWriter.WriteLine("EPGDays=" + EPGDays);

            if (CountryCode != null)
                streamWriter.WriteLine("Location=" + CountryCode + "," + Region);

            if (InputLanguage != null)
                streamWriter.WriteLine("InputLanguage=" + InputLanguage);

            if (ChannelBouquet != -1)
            {
                if (ChannelRegion != -1)
                    streamWriter.WriteLine("Channels=" + ChannelBouquet + "," + ChannelRegion);
                else
                    streamWriter.WriteLine("Channels=" + ChannelBouquet);
            }

            if (CharacterSet != null)
                streamWriter.WriteLine("Charset=" + CharacterSet);

            if (SDTPid != -1)
                streamWriter.WriteLine("SDTPid=" + SDTPid);

            if (EITPid != -1)
                streamWriter.WriteLine("EITPid=" + EITPid);

            if (MHW1Pids != null)
                streamWriter.WriteLine("MHW1Pids=" + MHW1Pids[0] + "," + MHW1Pids[1]);

            if (MHW2Pids != null)
                streamWriter.WriteLine("MHW2Pids=" + MHW2Pids[0] + "," + MHW2Pids[1] + "," + MHW2Pids[2]);

            if (DishNetworkPid != -1)
                streamWriter.WriteLine("DishNetworkPid=" + DishNetworkPid);

            if (Options.Count != 0)
            {
                streamWriter.Write("Option=");

                bool first = true;

                foreach (OptionEntry optionEntry in Options)
                {
                    if (!first)
                        streamWriter.Write(",");
                    streamWriter.Write(optionEntry.ToString());
                    first = false;
                }

                streamWriter.WriteLine();
            }
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public AdvancedRunParameters Clone()
        {
            AdvancedRunParameters newParameters = new AdvancedRunParameters();

            newParameters.ByteConvertTable = ByteConvertTable;
            newParameters.EITCarousel = EITCarousel;
            newParameters.ChannelBouquet = ChannelBouquet;
            newParameters.ChannelRegion = ChannelRegion;
            newParameters.CharacterSet = CharacterSet;
            newParameters.CountryCode = CountryCode;
            newParameters.DishNetworkPid = DishNetworkPid;
            newParameters.SDTPid = SDTPid;
            newParameters.EITPid = EITPid;            
            newParameters.EPGDays = EPGDays;
            newParameters.InputLanguage = InputLanguage;
            
            if (MHW1Pids != null)
                newParameters.MHW1Pids = (int[])MHW1Pids.Clone();
            if (MHW2Pids != null)
                newParameters.MHW2Pids = (int[])MHW2Pids.Clone();
            
            newParameters.Region = Region;

            foreach (OptionEntry optionEntry in Options)
                newParameters.Options.Add(optionEntry.Clone());

            return (newParameters);
        }
    }
}
