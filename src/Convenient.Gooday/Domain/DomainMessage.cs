using System.Collections.Generic;
using System.Text;
using Convenient.Gooday.Domain.Types;

namespace Convenient.Gooday.Domain
{
    public class DomainMessage
    {
        public ushort Id { get; set; }
        /// <summary>
        /// QR
        /// </summary>
        public MessageType Type { get; set; }
        /// <summary>
        /// OPCODE
        /// </summary>
        public OpCode OpCode { get; set; }
        /// <summary>
        /// AA
        /// </summary>
        public bool AuthoriativeAnswer { get; set; }
        /// <summary>
        /// TC
        /// </summary>
        public bool Truncation { get; set; }
        /// <summary>
        /// RD
        /// </summary>
        public bool RecursionDesired { get; set; }
        /// <summary>
        /// RA
        /// </summary>
        public bool RecursionAvailable { get; set; }
        /// <summary>
        /// RCODE
        /// </summary>
        public ResponseCode ResponseCode { get; set; }
        
        public List<Question> Questions { get; set; } = new List<Question>();
        public List<ResourceRecord> Answers { get; set; } = new List<ResourceRecord>();
        public List<ResourceRecord> Authorities { get; set; } = new List<ResourceRecord>();
        public List<ResourceRecord> Additionals { get; set; } = new List<ResourceRecord>();

        public override string ToString()
        {
            var builder = new StringBuilder()
                .AppendLine($"Id:{Id}, Type:{Type}, AA:{AuthoriativeAnswer}, RCODE:{ResponseCode}")
                .AppendIfAny("Questions", Questions)
                .AppendIfAny("Answers", Answers)
                .AppendIfAny("Authorities", Authorities)
                .AppendIfAny("Additionals", Additionals);
            return builder.ToString();
        }
    }
}