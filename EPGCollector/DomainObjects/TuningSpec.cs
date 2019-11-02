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

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a tuning spec.
    /// </summary>
    public class TuningSpec
    {
        /// <summary>
        /// Get or set the frequency.
        /// </summary>
        public TuningFrequency Frequency
        {
            get { return (frequency); }
            set { frequency = value; }
        }

        /// <summary>
        /// Get or set the satellite.
        /// </summary>
        public Satellite Satellite
        {
            get { return (satellite); }
            set { satellite = value; }
        }

        /// <summary>
        /// Get or set the symbol rate.
        /// </summary>
        public int SymbolRate
        {
            get { return (symbolRate); }
            set { symbolRate = value; }
        }

        /// <summary>
        /// Get or set the FEC rate.
        /// </summary>
        public FECRate FECRate
        {
            get { return (fec); }
            set { fec = value; }
        }

        /// <summary>
        /// Get or set the signal polarization.
        /// </summary>
        public SignalPolarization SignalPolarization
        {
            get { return (signalPolarization); }
            set { signalPolarization = value; }
        }

        /// <summary>
        /// Get or set the bandwidth.
        /// </summary>
        public int Bandwidth
        {
            get { return (bandwidth); }
            set { bandwidth = value; }
        }

        /// <summary>
        /// Get or set the physical channel number.
        /// </summary>
        public int ChannelNumber
        {
            get { return (channelNumber); }
            set { channelNumber = value; }
        }

        /// <summary>
        /// Get or set the Modulation.
        /// </summary>
        public SignalModulation.Modulation Modulation
        {
            get { return (modulation); }
            set { modulation = value; }
        }

        private Satellite satellite;
        private TuningFrequency frequency;
        private int symbolRate;
        private FECRate fec = new FECRate("3/4");
        private SignalPolarization signalPolarization = new SignalPolarization('H');
        private SignalModulation.Modulation modulation = SignalModulation.Modulation.QPSK;

        private int bandwidth;
        private int channelNumber;

        /// <summary>
        /// Initialize a new instance of the TuningSpec class.
        /// </summary>
        public TuningSpec() { }

        /// <summary>
        /// Initialize a new instance of the TuningSpec class for a DVB errestrial frequency.
        /// </summary>
        /// <param name="frequency">The terrestrial frequency to tune to.</param>
        public TuningSpec(TerrestrialFrequency frequency)
        {
             this.frequency = frequency;
             bandwidth = frequency.Bandwidth;   
        }

        /// <summary>
        /// Initialize a new instance of the TuningSpec class for a DVB satellite frequency.
        /// </summary>
        /// <param name="satellite">The satellite to tune to.</param>
        /// <param name="frequency">The frequency to tune to.</param>
        public TuningSpec(Satellite satellite, SatelliteFrequency frequency)
        {
            this.frequency = frequency;
            this.satellite = satellite;
            symbolRate = frequency.SymbolRate;
            fec = frequency.FEC;
            signalPolarization = frequency.Polarization;
            modulation = frequency.Modulation;
        }

        /// <summary>
        /// Initialize a new instance of the TuningSpec class for a DVB cable frequency.
        /// </summary>
        /// <param name="frequency">The frequency to tune to.</param>
        public TuningSpec(CableFrequency frequency)
        {
            this.frequency = frequency;
            symbolRate = frequency.SymbolRate;
            fec = frequency.FEC;
            modulation = frequency.Modulation;
        }

        /// <summary>
        /// Initialize a new instance of the TuningSpec class for an ATSC frequency.
        /// </summary>
        /// <param name="frequency">The frequency to tune to.</param>
        public TuningSpec(AtscFrequency frequency)
        {
            this.frequency = frequency;
            symbolRate = frequency.SymbolRate;
            fec = frequency.FEC;
            modulation = frequency.Modulation;
            channelNumber = frequency.ChannelNumber;
        }

        /// <summary>
        /// Initialize a new instance of the TuningSpec class for a Clear QAM frequency.
        /// </summary>
        /// <param name="frequency">The frequency to tune to.</param>
        public TuningSpec(ClearQamFrequency frequency)
        {
            this.frequency = frequency;
            symbolRate = frequency.SymbolRate;
            fec = frequency.FEC;
            modulation = frequency.Modulation;
            channelNumber = frequency.ChannelNumber;
        }

        /// <summary>
        /// Initialize a new instance of the TuningSpec class for a ISDB satellite frequency.
        /// </summary>
        /// <param name="satellite">The satellite to tune to.</param>
        /// <param name="frequency">The frequency to tune to.</param>
        public TuningSpec(Satellite satellite, ISDBSatelliteFrequency frequency)
        {
            this.frequency = frequency;
            this.satellite = satellite;
            symbolRate = frequency.SymbolRate;
            fec = frequency.FEC;
            signalPolarization = frequency.Polarization;
        }

        /// <summary>
        /// Initialize a new instance of the TuningSpec class for a ISDB errestrial frequency.
        /// </summary>
        /// <param name="frequency">The terrestrial frequency to tune to.</param>
        public TuningSpec(ISDBTerrestrialFrequency frequency)
        {
            this.frequency = frequency;
            bandwidth = frequency.Bandwidth;
        }
    }
}
