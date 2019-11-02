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

namespace DomainObjects
{
    /// <summary>
    /// The class that describes an exclusion entry for repeat program checking.
    /// </summary>
    public class RepeatExclusion
    {
        /// <summary>
        /// Get the title of the program.
        /// </summary>
        public string Title { get { return (title); } }
        /// <summary>
        /// Get the description of the program.
        /// </summary>
        public string Description { get { return (description); } }
        
        private string title;
        private string description;

        private RepeatExclusion() { }

        /// <summary>
        /// Initialize a new instance of the RepeatExclusion class.
        /// </summary>
        /// <param name="title">The title of the program.</param>
        /// <param name="description">The description of the program.</param>
        public RepeatExclusion(string title, string description)
        {
            this.title = title;
            this.description = description;
        }

        /// <summary>
        /// Check if a programme has been excluded.
        /// </summary>
        /// <param name="exclusions">The list of exclusions to check.</param>
        /// <param name="title">The title of the programme.</param>
        /// <param name="description">The description of the programme.</param>
        /// <returns>True if the programme has been excluded from repeat checking; false otherwise.</returns>
        public static bool CheckForExcludedProgram(Collection<RepeatExclusion> exclusions, string title, string description)
        {
            foreach (RepeatExclusion exclusion in exclusions)
            {
                bool titleReply;
                bool descriptionReply;

                if (exclusion.Title.Length != 0)
                    titleReply = checkForMatchingText(title, exclusion.Title);
                else
                    titleReply = true;

                if (exclusion.Description.Length != 0)
                    descriptionReply = checkForMatchingText(description, exclusion.Description);
                else
                    descriptionReply = true;

                if (titleReply && descriptionReply)
                    return (true);
            }

            return (false);
        }

        private static bool checkForMatchingText(string programText, string repeatText)
        {
            string lowerCaseProgramText = programText.ToLower();

            int matchMethod;
            string matchString;

            if (repeatText.StartsWith("<"))
            {
                if (repeatText.EndsWith(">"))
                {
                    matchMethod = 1;
                    matchString = (repeatText.Substring(1, repeatText.Length - 2)).ToLower();
                }
                else
                {
                    matchMethod = 2;
                    matchString = (repeatText.Substring(1)).ToLower();
                }
            }
            else
            {
                if (repeatText.EndsWith(">"))
                {
                    matchMethod = 3;
                    matchString = repeatText.Substring(0, repeatText.Length - 1).ToLower();
                }
                else
                {
                    matchMethod = 0;
                    matchString = repeatText.ToLower();
                }
            }

            switch (matchMethod)
            {
                case 0:
                    if (lowerCaseProgramText == matchString)
                        return (true);
                    break;
                case 1:
                    if (lowerCaseProgramText.Contains(matchString))
                        return (true);
                    break;
                case 2:
                    if (lowerCaseProgramText.StartsWith(matchString))
                        return (true);
                    break;
                case 3:
                    if (lowerCaseProgramText.EndsWith(matchString))
                        return (true);
                    break;
                default:
                    break;
            }

            return (false);
        }
    }
}
