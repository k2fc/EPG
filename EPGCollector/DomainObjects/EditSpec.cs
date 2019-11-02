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
using System.Text.RegularExpressions;
using System.Text;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a programme title or description text string.
    /// </summary>
    public class EditSpec
    {
        /// <summary>
        /// Get or set the text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Get or set the description.
        /// </summary>
        public TextLocation Location { get; set; }

        /// <summary>
        /// Get or set the text replacement mode.
        /// </summary>
        public TextReplacementMode ReplacementMode { get; set; } 

        /// <summary>
        /// Get or set the replacement text.
        /// </summary>
        public string ReplacementText { get; set; }

        /// <summary>
        /// Get or set whether to apply to titles.
        /// </summary>
        public bool ApplyToTitles { get; set; }

        /// <summary>
        /// Get or set whether to apply to descriptions.
        /// </summary>
        public bool ApplyToDescriptions { get; set; }

        /// <summary>
        /// Get or set the actual search text.
        /// </summary>
        public string ActualText { get; set; }

        /// <summary>
        /// Get or set the actual replacement text.
        /// </summary>
        public string ActualReplacementText { get; set; }

        private static bool edited;

        private EditSpec() { }

        /// <summary>
        /// Initialize a new instance of the EditSpec class.
        /// </summary>
        /// <param name="text">The substring text.</param>
        /// <param name="location">The position of the substring.</param>
        public EditSpec(string text, TextLocation location)
        {
            Text = text;
            Location = location;

            ActualText = convertText(Text);
        }

        /// <summary>
        /// Initialize a new instance of the EditSpec class.
        /// </summary>
        /// <param name="text">The substring text.</param>
        /// <param name="location">The position of the substring.</param>
        /// <param name="replacementText">The new text.</param>
        public EditSpec(string text, TextLocation location, string replacementText) : this (text, location)
        {
            ReplacementText = replacementText;

            if (ReplacementText != null)
                ActualReplacementText = convertText(ReplacementText);
        }

        /// <summary>
        /// Return a description of this instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (Text);
        }

        /// <summary>
        /// Process a programme title to remove or replace text.
        /// </summary>
        /// <param name="title">The title to process.</param>
        /// <returns>The edited string.</returns>
        public static string ProcessTitle(string title)
        {
            return (processText(title, true, RunParameters.Instance.EditSpecs));
        }

        /// <summary>
        /// Process a programme description to remove or replace text.
        /// </summary>
        /// <param name="description">The description to process.</param>
        /// <returns>The edited string.</returns>
        public static string ProcessDescription(string description)
        {
            return (processText(description, false, RunParameters.Instance.EditSpecs));
        }

        private static string processText(string text, bool applyToTitles, Collection<EditSpec> editSpecs)
        {
            if (editSpecs == null || text == null)
                return (text);

            string editedText = text;
            edited = false;

            foreach (EditSpec titleText in editSpecs)
            {
                if ((applyToTitles && titleText.ApplyToTitles) || (!applyToTitles && titleText.ApplyToDescriptions)) 
                    editedText = processText(editedText, titleText);                
            }

            if (!edited)
                return (editedText);
            else
                return (new Regex("[ ]{2,}").Replace(editedText, " ").Trim());
        }

        private static string processText(string text, EditSpec editSpec)
        {
            string editedText = text;            

            switch (editSpec.Location)
            {
                case TextLocation.Start:
                    if (editedText.StartsWith(editSpec.ActualText))
                    {
                        editedText = editedText.Remove(0, editSpec.ActualText.Length);
                        
                        if (editSpec.ActualReplacementText != null)
                        {
                            switch (editSpec.ReplacementMode)
                            {
                                case TextReplacementMode.TextOnly:
                                case TextReplacementMode.TextAndPreceeding:
                                    editedText = editSpec.ActualReplacementText + editedText;
                                    break;
                                case TextReplacementMode.TextAndFollowing:
                                case TextReplacementMode.Everything:
                                    editedText = editSpec.ActualReplacementText;
                                    break;
                                default:
                                    editedText = editSpec.ActualReplacementText + editedText;
                                    break;
                            }
                        }
                        else
                        {
                            switch (editSpec.ReplacementMode)
                            {
                                case TextReplacementMode.TextOnly:
                                case TextReplacementMode.TextAndPreceeding:
                                    break;
                                case TextReplacementMode.TextAndFollowing:
                                case TextReplacementMode.Everything:
                                    editedText = null;
                                    break;
                                default:
                                    break;
                            }
                        }

                        edited = true;
                    }
                    break;
                case TextLocation.End:
                    if (editedText.EndsWith(editSpec.ActualText))
                    {
                        editedText = editedText.Remove(editedText.Length - editSpec.ActualText.Length);                        
                        
                        if (editSpec.ActualReplacementText != null)
                        {
                            switch (editSpec.ReplacementMode)
                            {
                                case TextReplacementMode.TextOnly:
                                case TextReplacementMode.TextAndFollowing:
                                    editedText = editedText + editSpec.ActualReplacementText;
                                    break;
                                case TextReplacementMode.TextAndPreceeding:
                                case TextReplacementMode.Everything:
                                    editedText = editSpec.ActualReplacementText;
                                    break;
                                default:
                                    editedText = editedText + editSpec.ActualReplacementText;
                                    break;
                            }
                        }
                        else
                        {
                            switch (editSpec.ReplacementMode)
                            {
                                case TextReplacementMode.TextOnly:
                                case TextReplacementMode.TextAndFollowing:
                                    break;
                                case TextReplacementMode.TextAndPreceeding:
                                case TextReplacementMode.Everything:
                                    editedText = null;
                                    break;
                                default:
                                    break;
                            }
                        }

                        edited = true;
                    }
                    break;
                case TextLocation.Anywhere:
                    if (editedText.Contains(editSpec.ActualText))
                    {
                        if (editSpec.ActualReplacementText != null)
                        {
                            switch (editSpec.ReplacementMode)
                            {
                                case TextReplacementMode.TextOnly:
                                    editedText = editedText.Replace(editSpec.ActualText, editSpec.ActualReplacementText);
                                    break;
                                case TextReplacementMode.TextAndFollowing:
                                    editedText = editedText.Remove(editedText.IndexOf(editSpec.ActualText)) + editSpec.ActualReplacementText;
                                    break;
                                case TextReplacementMode.TextAndPreceeding:
                                    editedText = editSpec.ActualReplacementText + editedText.Substring(editedText.IndexOf(editSpec.ActualText) + editSpec.ActualText.Length);
                                    break;
                                case TextReplacementMode.Everything:
                                    editedText = editSpec.ActualReplacementText;
                                    break;
                                default:
                                    editedText = editedText.Replace(editSpec.ActualText, editSpec.ActualReplacementText);
                                    break;
                            }
                        }
                        else
                        {
                            switch (editSpec.ReplacementMode)
                            {
                                case TextReplacementMode.TextOnly:
                                    editedText = editedText.Replace(editSpec.ActualText, string.Empty);
                                    break;
                                case TextReplacementMode.TextAndFollowing:
                                    editedText = editedText.Remove(editedText.IndexOf(editSpec.ActualText));
                                    break;
                                case TextReplacementMode.TextAndPreceeding:
                                    editedText = editedText.Substring(editedText.IndexOf(editSpec.ActualText) + editSpec.ActualText.Length);
                                    break;
                                case TextReplacementMode.Everything:
                                    editedText = null;
                                    break;
                                default:
                                    editedText = editedText.Replace(editSpec.ActualText, string.Empty);
                                    break;
                            }
                        }

                        edited = true;
                    }
                    break;
                default:
                    break;
            }

            return (editedText);
        }

        private static string convertText(string text)
        {
            if (text == null || text.Length < 4)
                return (text);

            string processString = text.ToLowerInvariant();

            if (!text.StartsWith("0x"))
                return (text);

            if ((text.Length % 2) != 0)
                return (text);

            byte[] bytes = new byte[(text.Length - 1) / 2];
            int outIndex = 0;

            for (int index = 2; index < text.Length; index += 2)
            {
                int leftNibble;
                int rightNibble;

                if (text[index] >= '0' && text[index] <= '9')
                    leftNibble = text[index] - '0';
                else
                {
                    if (text[index] >= 'a' && text[index] <= 'f')
                        leftNibble = (text[index] - 'a') + 10;
                    else
                        return (text);
                }

                if (text[index + 1] >= '0' && text[index + 1] <= '9')
                    rightNibble = text[index + 1] - '0';
                else
                {
                    if (text[index + 1] >= 'a' && text[index + 1] <= 'f')
                        rightNibble = (text[index + 1] - 'a') + 10;
                    else
                        return (text);
                }

                bytes[outIndex] = (byte)((leftNibble * 16) + rightNibble);
                outIndex++;
            }

            return (Encoding.ASCII.GetString(bytes));
        }
    }
}
