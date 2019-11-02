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
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Text;
using System.ComponentModel;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a network stream used for a collection.
    /// </summary>
    public class StreamFrequency : TuningFrequency
    {
        /// <summary>
        /// Get or set the IP address.
        /// </summary>
        public string IPAddress
        {
            get { return (ipAddress); }
            set { ipAddress = value; }
        }

        /// <summary>
        /// Get or set the port number.
        /// </summary>
        public int PortNumber
        {
            get { return (portNumber); }
            set { portNumber = value; }
        }

        /// <summary>
        /// Get or set the Protocol.
        /// </summary>
        public StreamProtocol Protocol
        {
            get { return (protocol); }
            set { protocol = value; }
        }

        /// <summary>
        /// Get or set the path.
        /// </summary>
        public string Path
        {
            get { return (path); }
            set { path = value; }
        }

        /// <summary>
        /// Get or set the multicast source address.
        /// </summary>
        public string MulticastSource
        {
            get { return (multicastSource); }
            set { multicastSource = value; }
        }

        /// <summary>
        /// Get or set the multicast source port.
        /// </summary>
        public int MulticastPort
        {
            get { return (multicastPort); }
            set { multicastPort = value; }
        }

        /// <summary>
        /// Get or set the host name.
        /// </summary>
        public string HostName
        {
            get { return (hostName); }
            set { hostName = value; }
        }

        /// <summary>
        /// Get the tuner type for this type of frequency.
        /// </summary>
        public override TunerType TunerType { get { return (TunerType.Stream); } }

        private string ipAddress;
        private int portNumber;
        private StreamProtocol protocol;
        private string path;
        private string multicastSource;
        private int multicastPort;
        private string hostName;

        /// <summary>
        /// Enumerate servers.
        /// </summary>
        /// <param name="ServerName">Server name.</param>
        /// <param name="dwLevel">Level.</param>
        /// <param name="pBuf">Buffer.</param>
        /// <param name="dwPrefMaxLen">Maximum length.</param>
        /// <param name="dwEntriesRead">Entries returned.</param>
        /// <param name="dwTotalEntries">Total entries.</param>
        /// <param name="dwServerType">Server type.</param>
        /// <param name="domain">Domain.</param>
        /// <param name="dwResumeHandle">Handle</param>
        /// <returns>Zero if successful; non-zero otherwise.</returns>
        [DllImport("Netapi32", CharSet = CharSet.Auto, SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]        
        public static extern int NetServerEnum(
            string ServerName, // must be null
            int dwLevel,
            ref IntPtr pBuf,
            int dwPrefMaxLen,
            out int dwEntriesRead,
            out int dwTotalEntries,
            int dwServerType,
            string domain, // null for login domain
            out int dwResumeHandle
            );

        
        /// <summary>
        /// Free the results buffer.
        /// </summary>
        /// <param name="pBuf">Buffer pointer.</param>
        /// <returns>Zero if successful; non-zero otherwise.</returns>
        [DllImport("Netapi32", SetLastError = true), SuppressUnmanagedCodeSecurityAttribute]
        public static extern int NetApiBufferFree(IntPtr pBuf);

        //create a _SERVER_INFO_100 STRUCTURE
        /// <summary>
        /// The ServerInfo100 structure definition.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]        
        public struct ServerInfo100
        {
            internal int sv100_platform_id;
            [MarshalAs(UnmanagedType.LPWStr)]
            internal string sv100_name;
        }

        private const int maxPreferredLength = -1;
        private const int svTypeWorkstation = 1;
        private const int svTypeServer = 2;

        private static AutoResetEvent waiter;
        private static int instances;
        private static object lockObject = new object();
        private static Collection<IPAddress> activeAddresses;

        /// <summary>
        /// Initialize a new instance of the StreamFrequency class.
        /// </summary>
        public StreamFrequency() { }

        /// <summary>
        /// Check if this instance is equal to another.
        /// </summary>
        /// <param name="frequency">The other instance.</param>
        /// <param name="level">The level of equality to be checked.</param>
        /// <returns></returns>
        public override bool EqualTo(TuningFrequency frequency, EqualityLevel level)
        {
            bool reply = base.EqualTo(frequency, level);
            if (!reply)
                return (false);

            StreamFrequency streamFrequency = frequency as StreamFrequency;
            if (streamFrequency == null)
                return (false);

            if (IPAddress != streamFrequency.IPAddress)
                return (false);

            if (PortNumber != streamFrequency.PortNumber)
                return (false);

            if (level == EqualityLevel.Identity)
                return (true);

            if (Protocol != streamFrequency.Protocol)
                return (false);

            if (Path != streamFrequency.Path)
                return (false);

            if (MulticastSource != streamFrequency.MulticastSource)
                return (false);

            if (MulticastPort != streamFrequency.MulticastPort)
                return (false);

            return (true);
        }

        /// <summary>
        /// Get a description of this instance.
        /// </summary>
        /// <returns>A string describing this instance.</returns>
        public override string ToString()
        {
            if (ipAddress != "0.0.0.0")
                return (protocol + "://" + ipAddress + ":" + portNumber);
            else
                return (protocol + "://" + ":" + portNumber);
        }

        /// <summary>
        /// Get a string representing this frequency as a valid file name.
        /// </summary>
        /// <returns></returns>
        public override string GetValidFileName()
        {
            if (path != null)
            {
                char[] illegalChars = System.IO.Path.GetInvalidPathChars();

                string legalPath = path;

                foreach (char illegalChar in illegalChars)
                    legalPath = legalPath.Replace(illegalChar, '_');

                legalPath = legalPath.Replace(':', '_');
                legalPath = legalPath.Replace(@"\", "_");
                legalPath = legalPath.Replace('?', '_');

                if (legalPath.EndsWith("."))
                    legalPath = legalPath.Substring(0, legalPath.Length - 1) + '_';

                illegalChars = System.IO.Path.GetInvalidFileNameChars();
                foreach (char illegalChar in illegalChars)
                    legalPath = legalPath.Replace(illegalChar, '_');

                return (protocol + " " +
                    (ipAddress == "0.0.0.0" ? "" : ipAddress) +
                    " Port " + portNumber +
                    " " + legalPath);
            }
            else
                return (protocol + " " +
                    (ipAddress == "0.0.0.0" ? "" : ipAddress) +
                    " Port " + portNumber);

        }

        /// <summary>
        /// Generate a copy of this frequency.
        /// </summary>
        /// <returns>A new instance with the same properties as the old instance.</returns>
        public override TuningFrequency Clone()
        {
            StreamFrequency newFrequency = new StreamFrequency();
            base.Clone(newFrequency);

            newFrequency.IPAddress = ipAddress;
            newFrequency.PortNumber = portNumber;
            newFrequency.Protocol = protocol;
            newFrequency.Path = path;
            newFrequency.MulticastSource = multicastSource;
            newFrequency.MulticastPort = multicastPort;
            newFrequency.HostName = hostName;

            return (newFrequency);
        }

        /// <summary>
        /// Format check a user entered IP address.
        /// </summary>
        /// <param name="address">The address string.</param>
        /// <param name="protocol">The protocol.</param>
        /// <returns>An error message or null if the address is valid.</returns>
        public static string ValidateIPAddress(string address, string protocol)
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                if (address[0] > '9')
                    return (null);

                string[] parts = address.Trim().Split(new char[] { '.' });
                if (parts.Length != 4)
                    return ("The IP address is incorrect.");

                IPAddress testAddress;
                bool result = System.Net.IPAddress.TryParse(address.Trim(), out testAddress);
                if (!result)
                    return ("The IP address is incorrect.");
            }
            else
            {
                if (protocol != null)
                {
                    if (protocol == StreamProtocol.Rtsp.ToString() || protocol == StreamProtocol.Http.ToString())
                        return ("No IP address entered");
                }
            }

            return (null);
        }

        /// <summary>
        /// Get a list of network names and addresses.
        /// </summary>
        /// <returns>The list of names and addresses.</returns>
        public static Collection<NetworkSpec> GetNetworkComputers(string localAddress)
        {
            Collection<string> networkComputers = new Collection<string>();            
            
            IntPtr buffer = IntPtr.Zero;
            int entriesRead = 0;
            int totalEntries = 0;
            int resHandle = 0;
            int sizeofInfo = Marshal.SizeOf(typeof(ServerInfo100));

            try
            {
                int reply = NetServerEnum(null, 100, ref buffer, maxPreferredLength,
                    out entriesRead,
                    out totalEntries, svTypeWorkstation | svTypeServer, null, 
                    out	resHandle);
                if (reply == 0)
                {
                    for (int index = 0; index < totalEntries; index++)
                    {
                        IntPtr tmpBuffer = new IntPtr((int)buffer + (index * sizeofInfo));
                        ServerInfo100 svrInfo = (ServerInfo100)Marshal.PtrToStructure(tmpBuffer, typeof(ServerInfo100));

                        networkComputers.Add(svrInfo.sv100_name);
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                NetApiBufferFree(buffer);
            }

            Collection<NetworkSpec> networkSpecs = new Collection<NetworkSpec>();

            foreach (string name in networkComputers)
            {
                try
                {
                    IPAddress[] addressList = Dns.GetHostAddresses(name);
                    if (addressList.Length > 0 && (addressList[0].ToString().Contains(".") && !addressList[0].ToString().Contains(":")))
                        networkSpecs.Add(new NetworkSpec(name, addressList[0]));
                }
                catch (SocketException) 
                {
                    networkSpecs.Add(new NetworkSpec(name, null));
                }
            }

            Collection<IPAddress> addresses = GetActiveIPAddresses(localAddress);

            foreach (IPAddress address in addresses)
            {
                bool found = searchList(networkSpecs, address);
                if (!found)
                {
                    try
                    {
                        networkSpecs.Add(new NetworkSpec(Dns.GetHostEntry(address).HostName, address));
                    }
                    catch (SocketException) 
                    {
                        networkSpecs.Add(new NetworkSpec(null, address));
                    }
                }
            }

            return (networkSpecs);
        }

        private static bool searchList(Collection<NetworkSpec> networkSpecs, IPAddress address)
        {
            foreach (NetworkSpec networkSpec in networkSpecs)
            {
                if (networkSpec.Address != null && networkSpec.Address.Equals(address))
                    return (true);
            }

            return (false);
        }

        /// <summary>
        /// Get the contactable IP addresses on the local network.
        /// </summary>
        /// <param name="localAddress">The list of IP addresses.</param>
        /// <returns></returns>
        public static Collection<IPAddress> GetActiveIPAddresses(string localAddress)
        {
            int index = localAddress.LastIndexOf('.');
            if (index == -1)
                return(null);

            string baseIP = localAddress.Substring(0, index + 1);
            instances = 255;

            waiter = new AutoResetEvent(false);

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(workerDoWork);
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerCompleted);
            worker.RunWorkerAsync(baseIP);

            waiter.WaitOne();

            Collection<string> names = new Collection<string>();

            foreach (IPAddress activeAddress in activeAddresses)
            {
                try
                {
                    names.Add(Dns.GetHostEntry(activeAddress).HostName);
                }
                catch (SocketException) { }
            }

            return (activeAddresses);
        }

        private static void workerDoWork(object sender, DoWorkEventArgs e)
        {
            string baseIP = e.Argument as string;

            Collection<Ping> pingers = createPingers(instances);
            activeAddresses = new Collection<IPAddress>();

            PingOptions pingOptions = new PingOptions(5, true);
            ASCIIEncoding enc = new System.Text.ASCIIEncoding();
            byte[] data = enc.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");

            int count = 1;

            foreach (Ping pinger in pingers)
            {
                pinger.SendAsync(baseIP + count.ToString(), 250, data, pingOptions);
                count += 1;
            }

            while (instances > 0)
            {
                Thread.Sleep(100);
            }

            destroyPingers(pingers);

            waiter.Set();
        }

        private static void workerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                throw new InvalidOperationException("Background worker failed - see inner exception", e.Error);
        }

        private static Collection<Ping> createPingers(int count)
        {
            Collection<Ping> pingers = new Collection<Ping>();

            for (int index = 1; index <= count; index++)
            {
                Ping pinger = new Ping();
                pinger.PingCompleted += new PingCompletedEventHandler(pingCompleted);
                pingers.Add(pinger);
            }

            return (pingers);
        }

        private static void pingCompleted(object sender, PingCompletedEventArgs e)
        {
            lock (lockObject)
            {
                instances -= 1;

                if (!e.Cancelled && e.Error == null && e.Reply.Status == IPStatus.Success)
                    activeAddresses.Add(e.Reply.Address);
            }
        }

        private static void destroyPingers(Collection<Ping> pingers)
        {
            foreach (Ping pinger in pingers)
            {
                pinger.PingCompleted -= pingCompleted;
                pinger.Dispose();
            }

            pingers.Clear();
        }

        /// <summary>
        /// Get the first IPv4 address from a list.
        /// </summary>
        /// <param name="addressList"></param>
        /// <returns></returns>
        public static IPAddress GetAddress(IPAddress[] addressList)
        {
            if (addressList == null || addressList.Length == 0)
                return (null);

            foreach (IPAddress address in addressList)
            {
                string addressString = address.ToString();

                if (!addressString.Contains(":"))
                {
                    string[] addressParts = addressString.Split(new char[] { '.' });

                    if (addressParts.Length == 4 && addressParts[0] != "127")
                        return(address);
                }

            }

            return (null);
        }
    }
}
