using System;
using System.Linq;

namespace Convenient.Gooday.Domain.Extensions
{
    internal static class ShortExtensions
    {
        internal static ushort SetBits(this ushort oldValue, int position, int length, bool blnValue)
        {
            return SetBits(oldValue, position, length, blnValue ? (ushort)1 : (ushort)0);
        }

        internal static ushort SetBits(this ushort oldValue, int position, int length, ushort newValue)
        {
            // sanity check
            if (length <= 0 || position >= 16)
            {
                return oldValue;
            }

            // get some mask to put on
            var mask = (2 << (length - 1)) - 1;

            // clear out value
            oldValue &= (ushort)~(mask << position);

            // set new value
            oldValue |= (ushort)((newValue & mask) << position);
            return oldValue;
        }

        internal static ushort GetBits(this ushort oldValue, int position, int length)
        {
            // sanity check
            if (length <= 0 || position >= 16)
                return 0;

            // get some mask to put on
            var mask = (2 << (length - 1)) - 1;

            // shift down to get some value and mask it
            return (ushort)((oldValue >> position) & mask);
        }

        internal static byte[] ToBytes(this ushort sValue)
        {
            return BitConverter.GetBytes(HostToNetworkOrder((short)sValue));
        }
        
        internal static byte[] ToBytes(this uint value)
        {
            return BitConverter.GetBytes(value).Reverse().ToArray();
        }

        private static short HostToNetworkOrder(this short host)
        {
            return (short)(((host & 0xff) << 8) | ((host >> 8) & 0xff));
        }
    }
}