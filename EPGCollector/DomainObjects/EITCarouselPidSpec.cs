using System;
using System.Collections.ObjectModel;
using System.Xml;

namespace DomainObjects
{
    /// <summary>
    /// The class that defines the details for an EIT carousel pid.
    /// </summary>
    public class EITCarouselPidSpec
    {
        /// <summary>
        /// Get the pid number.
        /// </summary>
        public int Pid { get; private set; }
        /// <summary>
        /// Get the list of carousel directories.
        /// </summary>
        public Collection<string> CarouselDirectories { get; private set; }
        /// <summary>
        /// Get the list of zip directories.
        /// </summary>
        public Collection<string> ZipDirectories { get; private set; }

        /// <summary>
        /// Initialize a new instance of the EITCarouselPidSpec class. 
        /// </summary>
        public EITCarouselPidSpec() { }

        internal void Load(int pid, XmlReader reader)
        {
            Pid = pid;

            while (!reader.EOF)
            {
                reader.Read();
                if (reader.IsStartElement())
                {
                    switch (reader.Name.ToLowerInvariant())
                    {
                        case "carouseldirectory":
                            if (CarouselDirectories == null)
                                CarouselDirectories = new Collection<string>();
                            CarouselDirectories.Add(reader.ReadString());
                            break;
                        case "zipdirectory":
                            if (ZipDirectories == null)
                                ZipDirectories = new Collection<string>();
                            ZipDirectories.Add(reader.ReadString());
                            break;
                        default:
                            break;
                    }
                }
            }

            reader.Close();
        }

    }
}
