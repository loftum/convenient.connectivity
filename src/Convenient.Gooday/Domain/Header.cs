using System.Collections;
using System.Collections.Generic;
using Convenient.Gooday.Domain.Extensions;
using Convenient.Gooday.Domain.Types;

namespace Convenient.Gooday.Domain
{
	public class Header
    {
        public ushort Id;
        public ushort Flags;
        public ushort QDCount;
        public ushort ANCount;
        public ushort NSCount;
        public ushort ARCount;


        /// <summary>
		/// Represents the header as a byte array
		/// </summary>
		public byte[] Data
		{
			get
			{
				var a = new BitArray(new byte[2]);
				
				var data = new List<byte>();
				data.AddRange(Id.ToBytes());
				data.AddRange(Flags.ToBytes());
				data.AddRange(QDCount.ToBytes());
				data.AddRange(ANCount.ToBytes());
				data.AddRange(NSCount.ToBytes());
				data.AddRange(ARCount.ToBytes());
				return data.ToArray();
			}
		}


        /// <summary>
		/// query (false), or a response (true)
		/// </summary>
		public bool QR
		{
			get => Flags.GetBits(15, 1) == 1;
			set => Flags = Flags.SetBits(15, 1, value);
		}

		/// <summary>
		/// Specifies kind of query
		/// </summary>
		public OpCode OPCODE
		{
			get => (OpCode) Flags.GetBits(11, 4);
			set => Flags = Flags.SetBits(11, 4, (ushort)value);
		}

		/// <summary>
		/// Authoritative Answer
		/// </summary>
		public bool AA
		{
			get => Flags.GetBits(10, 1) == 1;
			set => Flags = Flags.SetBits(10, 1, value);
		}

		/// <summary>
		/// TrunCation
		/// </summary>
		public bool TC
		{
			get => Flags.GetBits(9, 1) == 1;
			set => Flags = Flags.SetBits(9, 1, value);
		}

		/// <summary>
		/// Recursion Desired
		/// </summary>
		public bool RD
		{
			get => Flags.GetBits(8, 1) == 1;
			set => Flags = Flags.SetBits(8, 1, value);
		}

		/// <summary>
		/// Recursion Available
		/// </summary>
		public bool RA
		{
			get => Flags.GetBits(7, 1) == 1;
			set => Flags = Flags.SetBits(7, 1, value);
		}

		/// <summary>
		/// Reserved for future use
		/// </summary>
		public ushort Z
		{
			get => Flags.GetBits(4, 3);
			set => Flags = Flags.SetBits(4, 3, value);
		}

		/// <summary>
		/// Response code
		/// </summary>
		public ResponseCode RCODE
		{
			get => (ResponseCode) Flags.GetBits(0, 4);
			set => Flags = Flags.SetBits(0, 4, (ushort)value);
		}

		public override string ToString()
		{
			return $"Id: {Id}, QR: {QR}, Opcode: {OPCODE}, AA: {AA}, TC: {TC}, RD: {RD}, RA: {RA}, Z: {Z}, RCODE: {RCODE}, QDCount: {QDCount}, ANCount: {ANCount}, NSCount: {NSCount}, ARCount: {ARCount}";
		}
    }
}