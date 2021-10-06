using System.Linq;

namespace Convenient.Gooday
{
    public readonly struct ServiceIdentifier
    {
        /// <summary>
        /// E.g. 4ccc4aa6-d4e7-4bc4-9646-f794fe723c3e 
        /// </summary>
        public string InstanceName { get; }
        /// <summary>
        /// E.g. _my-music-service._tcp
        /// </summary>
        public string ServiceType { get; }
        /// <summary>
        /// E.g. local
        /// </summary>
        public string Domain { get; }
        
        public ServiceIdentifier(string instanceName, string serviceType, string domain)
        {
            InstanceName = instanceName;
            ServiceType = serviceType;
            Domain = domain;
        }

        /// <summary>
        /// E.g. 4ccc4aa6-d4e7-4bc4-9646-f794fe723c3e._my-music-service._tcp.local.
        /// </summary>
        /// <returns>E.g. 4ccc4aa6-d4e7-4bc4-9646-f794fe723c3e._my-music-service._tcp.local.</returns>
        public string Format()
        {
            return $"{InstanceName}.{ServiceType}.{Domain}."; // Yes, trailing dot.
        }

        public static bool TryParse(string formatted, out ServiceIdentifier identifier)
        {
            var parts = formatted.Split('.');
            if (parts.Length < 3)
            {
                identifier = default;
                return false;
            }
            
            var instanceName = parts[0];
            var serviceType = string.Join(".", parts.Skip(1).Take(parts.Length - 2));
            var domain = parts.Last();
            identifier = new ServiceIdentifier(instanceName, serviceType, domain);
            
            return true;
        }

        public override string ToString() => Format();
    }
}