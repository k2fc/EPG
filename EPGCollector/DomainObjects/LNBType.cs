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
    /// The class that describes the LNB types for Dish Network.
    /// </summary>
    public class LNBType
    {
        /// <summary>
        /// The legacy type.
        /// </summary>
        public const string Legacy = "Legacy";
        /// <summary>
        /// The DishPro digital satellite service type.
        /// </summary>
        public const string DishProDigitalService = "DSS";
        /// <summary>
        /// The DishPro fixed fixed service satellite type.
        /// </summary>
        public const string DishProFixedService = "FSS";
        
        /// <summary>
        /// Get all the LNB types.
        /// </summary>
        public static Collection<LNBType> LNBTypes
        {
            get
            {
                Collection<LNBType> lnbTypes = new Collection<LNBType>();

                lnbTypes.Add(new LNBType(Legacy));
                lnbTypes.Add(new LNBType(DishProDigitalService));
                lnbTypes.Add(new LNBType(DishProFixedService)); 

                return (lnbTypes);
            }
        }

        /// <summary>
        /// Get the LNB type.
        /// </summary>
        public string Type { get { return(type); } }

        private string type = Legacy;

        private const string legacyDecode = "Legacy";
        private const string dssDecode = "DishPro digital satellite service (DSS)";
        private const string fssDecode = "DishPro fixed satellite service (FSS)";

        /// <summary>
        /// Initialize a new instance of the LNBType class.
        /// </summary>
        public LNBType() { }

        /// <summary>
        /// Initialize a new instance of the LNBType class.
        /// </summary>
        /// <param name="type">The LNB type to be set.</param>
        public LNBType(string type)
        {
            this.type = type;
        }

        /// <summary>
        /// Return a string representation of this instance.
        /// </summary>
        /// <returns>A string describing this LNB type.</returns>
        public override string ToString()
        {
            switch (type)
            {
                case Legacy:
                    return (legacyDecode);
                case DishProDigitalService:
                    return (dssDecode);
                case DishProFixedService:
                    return (fssDecode);
                default:
                    return (legacyDecode);
            }
        }

        /// <summary>
        /// Get a new instance of the LNBType class.
        /// </summary>
        /// <param name="decode">The lnb type decode.</param>
        /// <returns>A new instance of the LNBType class.</returns>
        public static LNBType GetInstance(string decode)
        {
            switch (decode)
            {
                case legacyDecode:
                    return (new LNBType(Legacy));
                case dssDecode:
                    return (new LNBType(DishProDigitalService));
                case fssDecode:
                    return (new LNBType(DishProFixedService));
                default:
                    return (new LNBType(Legacy));
            }
        }
    }
}
