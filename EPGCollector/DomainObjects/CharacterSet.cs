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
using System.Text;

namespace DomainObjects
{
    /// <summary>
    ///  The class that describes a character set.
    /// </summary>
    public class CharacterSet
    {
        /// <summary>
        /// Get the collection of character sets.
        /// </summary>
        public static Collection<CharacterSet> CharacterSets
        {
            get
            {
                if (characterSets == null)
                {
                    characterSets = new Collection<CharacterSet>();

                    foreach (EncodingInfo encodingInfo in ASCIIEncoding.GetEncodings())
                    {
                        if (DebugEntry.IsDefined(DebugName.LogCodepages))
                        {
                            Logger.Instance.Write("Codepage " + encodingInfo.CodePage + " " +
                                encodingInfo.DisplayName + " " +
                                encodingInfo.Name);
                        }
                        characterSets.Add(new CharacterSet(encodingInfo.Name, encodingInfo.DisplayName, encodingInfo.CodePage));
                    }

                    characterSets.Insert(0, new CharacterSet(string.Empty, " -- Undefined --", 0));
                }
                
                return (characterSets);
            }
        }

        /// <summary>
        /// Get the character set name.
        /// </summary>
        public string Name { get { return (name); } }
        /// <summary>
        /// Get the character set description.
        /// </summary>
        public string Description { get { return (description); } }

        private string name;
        private string description;
        private int codePage;

        private CharacterSetUsage usage = CharacterSetUsage.NotUsed;
        private int countUsed;

        private static Collection<CharacterSet> characterSets;

        private CharacterSet() { }

        /// <summary>
        /// Initialize a new instance of the CharacterSet class.
        /// </summary>
        /// <param name="name">The character set name.</param>
        /// <param name="description">The character set description.</param>
        /// <param name="codePage">The character set codepage.</param>
        public CharacterSet(string name, string description, int codePage)
        {
            this.name = name;
            this.description = description;
            this.codePage = codePage;
        }

        /// <summary>
        /// Find a character set.
        /// </summary>
        /// <param name="name">The name of the character set.</param>
        /// <returns>The character set or null if it cannot be found.</returns>
        public static CharacterSet FindCharacterSet(string name)
        {
            foreach (CharacterSet characterSet in CharacterSets)
            {
                if (characterSet.Name == name)
                    return (characterSet);
            }

            return (null);
        }

        /// <summary>
        /// Flag the character set as used.
        /// </summary>
        /// <param name="name">The name of the character set.</param>
        /// <param name="usage">The type of usage.</param>
        public static void MarkAsUsed(string name, CharacterSetUsage usage)
        {
            CharacterSet characterSet = FindCharacterSet(name);
            if (characterSet == null)
            {
                characterSet = new CharacterSet(name, null, 0);
                CharacterSets.Add(characterSet);                
            }

            characterSet.usage = usage;
            characterSet.countUsed++;
        }

        /// <summary>
        /// Log the character set usage.
        /// </summary>
        public static void LogUsage()
        {
            if (characterSets != null)
            {
                Logger.Instance.WriteSeparator("Character Sets Used");

                foreach (CharacterSet characterSet in characterSets)
                {
                    if (characterSet.usage != CharacterSetUsage.NotUsed)
                    {
                        if (characterSet.Description != null)
                            Logger.Instance.Write("Character set:" +
                                " Codepage = " + characterSet.codePage +
                                " Name = " + characterSet.name +
                                " Description = " + characterSet.Description +
                                " Usage = " + characterSet.usage +
                                " Count = " + characterSet.countUsed);
                        else
                            Logger.Instance.Write("Character set:" +
                                " Codepage = not defined" + 
                                " Name = " + characterSet.name +
                                " Description = not defined" +
                                " Usage = " + characterSet.usage +
                                " Count = " + characterSet.countUsed);
                    }
                }

                Logger.Instance.WriteSeparator("End Of Character Sets Used");
            }
        }

        /// <summary>
        /// Get a description of this instance.
        /// </summary>
        /// <returns>A string describing this instance.</returns>
        public override string ToString()
        {
            return (description);
        }
    }
}
