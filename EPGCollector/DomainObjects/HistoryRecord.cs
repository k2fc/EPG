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

using System;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a history record.
    /// </summary>
    public class HistoryRecord
    {
        /// <summary>
        /// Get or set the current history record.
        /// </summary>
        public static HistoryRecord Current { get; set; }

        /// <summary>
        /// Get or set the start date and time of the event.
        /// </summary>
        public DateTime StartDate { get; set; }
        /// <summary>
        /// Get or set the collection result.
        /// </summary>
        public string CollectionResult { get; set; }
        /// <summary>
        /// Get or set the collection EPG count.
        /// </summary>
        public int CollectionCount { get; set; }
        /// <summary>
        /// Get or set the collection time.
        /// </summary>
        public TimeSpan? CollectionDuration { get; set; }
        /// <summary>
        /// Get or set the metadata lookup result.
        /// </summary>
        public string LookupResult { get; set; }
        /// <summary>
        /// Get or set the metadata lookup rate. 
        /// </summary>
        public int LookupRate { get; set; }
        /// <summary>
        /// Get or set the metadata lookup time.
        /// </summary>
        public TimeSpan? LookupDuration { get; set; }
        /// <summary>
        /// Get or set the update result.
        /// </summary>
        public string UpdateResult { get; set; }
        /// <summary>
        /// Get or set the number of updates added.
        /// </summary>
        public int UpdateAdded { get; set; }
        /// <summary>
        /// Get or set the number of updates changed.
        /// </summary>
        public int UpdateChanged { get; set; }
        /// <summary>
        /// Get or set the update time.
        /// </summary>
        public TimeSpan? UpdateDuration { get; set; }
        /// <summary>
        /// Get the software verasion number.
        /// </summary>
        public string SoftwareVersion { get; private set; }

        /// <summary>
        /// Get the fields of the record as a comma separated string.
        /// </summary>
        public string Contents
        {
            get
            {
                return (SoftwareVersion + "," +
                    StartDate.ToShortDateString() + "," +
                    StartDate.ToShortTimeString() + "," +
                    CollectionResult + "," +
                    CollectionCount + "," +
                    (CollectionDuration.HasValue ? CollectionDuration.Value.ToString() : string.Empty) + "," +
                    LookupResult + "," +
                    LookupRate + "," +
                    (LookupDuration.HasValue ? LookupDuration.Value.ToString() : string.Empty) + "," +
                    UpdateResult + "," +
                    UpdateAdded + "," +
                    UpdateChanged + "," +
                    (UpdateDuration.HasValue ? UpdateDuration.Value.ToString() : string.Empty));
            }
        }

        private HistoryRecord() { }

        /// <summary>
        /// Initialize a new instance of the HistoryRecord class.
        /// </summary>
        /// <param name="startDate">The start date and time.</param>
        public HistoryRecord(DateTime startDate)
        {
            SoftwareVersion = RunParameters.SystemVersion;
            StartDate = startDate;

            LookupRate = -1;
        }

        /// <summary>
        /// Initialize a new instance of the HistoryRecord class.
        /// </summary>
        /// <param name="line">The comma separated data for the new instance.</param>
        public HistoryRecord(string line)
        {
            string[] fields = line.Split(new char[] { ',' });

            if (fields.Length != 13)
                return;

            SoftwareVersion = fields[0];

            try
            {
                StartDate = DateTime.Parse(fields[1] + " " + fields[2]);
            }
            catch (FormatException)
            {
                StartDate = DateTime.Now;
            }

            CollectionResult = fields[3];
            CollectionCount = Int32.Parse(fields[4]);

            if (!string.IsNullOrWhiteSpace(fields[5]))
                CollectionDuration = TimeSpan.Parse(fields[5]);

            LookupResult = fields[6];
            LookupRate = Int32.Parse(fields[7]);

            if (!string.IsNullOrWhiteSpace(fields[8]))
                LookupDuration = TimeSpan.Parse(fields[8]);
            
            UpdateResult = fields[9];
            UpdateAdded = Int32.Parse(fields[10]);
            UpdateChanged = Int32.Parse(fields[11]);

            if (!string.IsNullOrWhiteSpace(fields[12]))
                UpdateDuration = TimeSpan.Parse(fields[12]);
        }
    }
}
