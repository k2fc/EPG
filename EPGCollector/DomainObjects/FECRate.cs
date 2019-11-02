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
using System.Collections.ObjectModel;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes the forward error correction.
    /// </summary>
    public class FECRate
    {
        /// <summary>
        /// The FEC of 1/2.
        /// </summary>
        public const string FECRate12 = "1/2";
        /// <summary>
        /// The FEC of 1/3.
        /// </summary>
        public const string FECRate13 = "1/3";
        /// <summary>
        /// The FEC of 1/4.
        /// </summary>
        public const string FECRate14 = "1/4";
        /// <summary>
        /// The FEC of 2/3.
        /// </summary>
        public const string FECRate23 = "2/3";
        /// <summary>
        /// The FEC of 2/5.
        /// </summary>
        public const string FECRate25 = "2/5";
        /// <summary>
        /// The FEC of 3/4.
        /// </summary>
        public const string FECRate34 = "3/4";
        /// <summary>
        /// The FEC of 3/5.
        /// </summary>
        public const string FECRate35 = "3/5";
        /// <summary>
        /// The FEC of 4/5.
        /// </summary>
        public const string FECRate45 = "4/5";
        /// <summary>
        /// The FEC of 5/11.
        /// </summary>
        public const string FECRate511 = "5/11";
        /// <summary>
        /// The FEC of 5/6.
        /// </summary>
        public const string FECRate56 = "5/6";
        /// <summary>
        /// The FEC of 6/7.
        /// </summary>
        public const string FECRate67 = "6/7";
        /// <summary>
        /// The FEC of 7/8.
        /// </summary>
        public const string FECRate78 = "7/8";
        /// <summary>
        /// The FEC of 8/9.
        /// </summary>
        public const string FECRate89 = "8/9";
        /// <summary>
        /// The FEC of 9/10.
        /// </summary>
        public const string FECRate910 = "9/10";
        /// <summary>
        /// The maximum FEC.
        /// </summary>
        public const string FECRateMax = "Max";
        /// <summary>
        /// The undefined FEC.
        /// </summary>
        public const string FECRateUndefined = "Undefined";

        /// <summary>
        /// Get all the FEC rates.
        /// </summary>
        public static Collection<FECRate> FECRates
        {
            get
            {
                Collection<FECRate> fecRates = new Collection<FECRate>();

                fecRates.Add(new FECRate(FECRate12));
                fecRates.Add(new FECRate(FECRate13));
                fecRates.Add(new FECRate(FECRate14));
                fecRates.Add(new FECRate(FECRate23));
                fecRates.Add(new FECRate(FECRate25));
                fecRates.Add(new FECRate(FECRate34));
                fecRates.Add(new FECRate(FECRate35));
                fecRates.Add(new FECRate(FECRate45));
                fecRates.Add(new FECRate(FECRate511));
                fecRates.Add(new FECRate(FECRate56));
                fecRates.Add(new FECRate(FECRate67));
                fecRates.Add(new FECRate(FECRate78));
                fecRates.Add(new FECRate(FECRate89));
                fecRates.Add(new FECRate(FECRate910));
                fecRates.Add(new FECRate(FECRateMax));
                fecRates.Add(new FECRate(FECRateUndefined));

                return (fecRates);
            }
        }

        /// <summary>
        /// Get or set the FEC rate.
        /// </summary>
        public string Rate
        {
            get { return (fecRate); }
            set
            {
                switch (value)
                {
                    case FECRate12:
                    case FECRate13:
                    case FECRate14:
                    case FECRate23:
                    case FECRate25:
                    case FECRate34:
                    case FECRate35:
                    case FECRate45:
                    case FECRate511:
                    case FECRate56:
                    case FECRate67:
                    case FECRate78:
                    case FECRate89:
                    case FECRate910:
                    case FECRateMax:
                    case FECRateUndefined:
                        fecRate = value;
                        break;
                    default:
                        throw (new ArgumentException("FECRate given unknown value of " + value));
                }
            }
        }

        private string fecRate = FECRate34;

        /// <summary>
        /// Initialize a new instance of the FECRate class.
        /// </summary>
        public FECRate() { }

        /// <summary>
        /// Initialize a new instance of the FECRate class.
        /// </summary>
        /// <param name="fecRate">The FEC rate to be set.</param>
        public FECRate(string fecRate)
        {
            Rate = fecRate; 
        }

        /// <summary>
        /// Return a string representation of this instance.
        /// </summary>
        /// <returns>A string describing this FEC rate.</returns>
        public override string ToString()
        {
            return (fecRate);
        }

        /// <summary>
        /// Convert the DVB FEC rate to the internal FEC rate.
        /// </summary>
        /// <param name="fecRate">The rate to be converted.</param>
        /// <returns>The converted value.</returns>
        public static FECRate ConvertDVBFecRate(int fecRate)
        {
            switch (fecRate)
            {
                case 0:
                    return (new FECRate(FECRate.FECRateUndefined));
                case 1:
                    return (new FECRate(FECRate.FECRate12));
                case 2:
                    return (new FECRate(FECRate.FECRate23));
                case 3:
                    return (new FECRate(FECRate.FECRate34));
                case 4:
                    return (new FECRate(FECRate.FECRate56));
                case 5:
                    return (new FECRate(FECRate.FECRate78));
                case 6:
                    return (new FECRate(FECRate.FECRate89));
                case 7:
                    return (new FECRate(FECRate.FECRate35));
                case 8:
                    return (new FECRate(FECRate.FECRate45));
                case 9:
                    return (new FECRate(FECRate.FECRate910));
                default:
                    return (new FECRate(FECRate.FECRateUndefined));
            }
        }

        /// <summary>
        /// Get the index number of a DVB FEC value.
        /// </summary>
        /// <param name="fecRate">The FEC value.</param>
        /// <returns>The index number.</returns>
        public static int GetIndex(FECRate fecRate)
        {
            if (fecRate.Rate == FECRate.FECRate12)
                return (0);

            if (fecRate.Rate == FECRate.FECRate13)
                return (1);

            if (fecRate.Rate == FECRate.FECRate14)
                return (2);

            if (fecRate.Rate == FECRate.FECRate23)
                return (3);

            if (fecRate.Rate == FECRate.FECRate25)
                return (4);

            if (fecRate.Rate == FECRate.FECRate34)
                return (5);

            if (fecRate.Rate == FECRate.FECRate35)
                return (6);

            if (fecRate.Rate == FECRate.FECRate45)
                return (7);

            if (fecRate.Rate == FECRate.FECRate511)
                return (8);

            if (fecRate.Rate == FECRate.FECRate56)
                return (9);

            if (fecRate.Rate == FECRate.FECRate67)
                return (10);

            if (fecRate.Rate == FECRate.FECRate78)
                return (11);

            if (fecRate.Rate == FECRate.FECRate89)
                return (12);

            if (fecRate.Rate == FECRate.FECRate910)
                return (13);

            if (fecRate.Rate == FECRate.FECRateMax)
                return (14);

            if (fecRate.Rate == FECRate.FECRateUndefined)
                return (15);

            return (15);
        }
    }
}
