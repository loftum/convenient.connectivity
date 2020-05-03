using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Convenient.ZeroConf.Domain.Records;

namespace Convenient.ZeroConf.Domain
{
    public class MessageWriter
    {
        private readonly List<byte> _bytes = new List<byte>();
        
        public byte[] Write(ZeroconfMessage message)
        {
            WriteHeader(message);
            foreach (var question in message.Questions)
            {
                Write(question);
            }

            foreach (var answer in message.Answers)
            {
                Write(answer);
            }

            return _bytes.ToArray();
        }

        private void WriteHeader(ZeroconfMessage message)
        {
            var header = CreateHeader(message);
            _bytes.AddRange(header.Id.ToBytes());
            _bytes.AddRange(header.Flags.ToBytes());
            _bytes.AddRange(header.QDCount.ToBytes());
            _bytes.AddRange(header.ANCount.ToBytes());
            _bytes.AddRange(header.NSCount.ToBytes());
            _bytes.AddRange(header.ARCount.ToBytes());
        }
        
        private static Header CreateHeader(ZeroconfMessage message)
        {
            return new Header
            {
                Id = message.Id,
                QR = message.Type != MessageType.Query,
                OPCODE = message.OpCode,
                AA = message.AuthoriativeAnswer,
                TC = message.Truncation,
                RD = message.RecursionDesired,
                RA = message.RecursionAvailable,
                Z = 0,
                RCODE = message.ResponseCode,
                QDCount = (ushort) message.Questions.Count,
                ANCount = (ushort) message.Answers.Count,
                NSCount = (ushort) message.Authorities.Count,
                ARCount = (ushort) message.Additionals.Count
            };
        }

        private void Write(ResourceRecord resourceRecord)
        {
            _bytes.AddRange(DomainNameToBytes(resourceRecord.Name));
            _bytes.AddRange(((ushort)resourceRecord.Type).ToBytes());
            _bytes.AddRange(((ushort)resourceRecord.Class).ToBytes());
            _bytes.AddRange(resourceRecord.Ttl.ToBytes());
            Write(resourceRecord.Record);
        }

        private void Write(IRecord record)
        {
            switch (record)
            {
                case null:
                    throw new ArgumentNullException(nameof(record));
                case PointerRecord p:
                {
                    var bytes = DomainNameToBytes(p.PTRDName);
                    _bytes.AddRange(((ushort)bytes.Length).ToBytes());
                    _bytes.AddRange(DomainNameToBytes(p.PTRDName));
                    break;
                }
                case ServiceRecord s:
                {
                    var bytes = new List<byte>();
                    bytes.AddRange(s.Priority.ToBytes());
                    bytes.AddRange(s.Weight.ToBytes());
                    bytes.AddRange(s.Port.ToBytes());
                    bytes.AddRange(DomainNameToBytes(s.Target));
                    
                    _bytes.AddRange(((ushort)bytes.Count).ToBytes());
                    _bytes.AddRange(bytes);
                    break;
                }
                case TextRecord t:
                {
                    var bytes = new List<byte>();
                    foreach (var text in t.Text)
                    {
                        bytes.Add((byte)text.Length);
                        bytes.AddRange(Encoding.UTF8.GetBytes(text));
                    }
                    _bytes.AddRange(((ushort)bytes.Count).ToBytes());
                    _bytes.AddRange(bytes);
                    break;
                }
                case UnknownRecord u:
                {
                    _bytes.AddRange(((ushort)u.RData.Length).ToBytes());
                    _bytes.AddRange(u.RData);
                    break;
                }
                case ARecord a:
                {
                    var numbers = a.Address.Split('.').Select(byte.Parse).ToArray();
                    _bytes.AddRange(((ushort)numbers.Length).ToBytes());
                    _bytes.AddRange(numbers);
                    break;
                }
                
                default:
                    throw new InvalidOperationException($"Unknown record {record.GetType().Name}");
            }
        }

        private void Write(Question question)
        {
            _bytes.AddRange(DomainNameToBytes(question.QName));
            _bytes.AddRange(((ushort) question.QType).ToBytes());
            _bytes.AddRange(((ushort) question.QClass).ToBytes());
        }
        
        
        
        static byte[] DomainNameToBytes(string src)
        {
            if (!src.EndsWith(".", StringComparison.Ordinal))
            {
                src = $"{src}.";
            }

            if (src == ".")
            {
                return new byte[1];
            }
                

            var sb = new StringBuilder();
            int ii, jj, intLen = src.Length;
            sb.Append('\0');
            for (ii = 0, jj = 0; ii < intLen; ii++, jj++)
            {
                sb.Append(src[ii]);
                if (src[ii] == '.')
                {
                    sb[ii - jj] = (char)(jj & 0xff);
                    jj = -1;
                }
            }
            sb[sb.Length -1] = '\0';
            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}