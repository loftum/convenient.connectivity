using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Convenient.Gooday.Domain;
using Convenient.Gooday.Domain.Extensions;
using Convenient.Gooday.Domain.Records;

namespace Convenient.Gooday.Parsing
{
    public class MessageWriter
    {
        private readonly List<byte> _bytes = new List<byte>();
        
        public byte[] Write(DomainMessage message)
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

        private void WriteHeader(DomainMessage message)
        {
            var header = CreateHeader(message);
            _bytes.AddRange(header.Id.ToBytes());
            _bytes.AddRange(header.Flags.ToBytes());
            _bytes.AddRange(header.QDCount.ToBytes());
            _bytes.AddRange(header.ANCount.ToBytes());
            _bytes.AddRange(header.NSCount.ToBytes());
            _bytes.AddRange(header.ARCount.ToBytes());
        }
        
        private static Header CreateHeader(DomainMessage message)
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
            _bytes.AddRange(DomainName.ToBytes(resourceRecord.Name));
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
                case PTRRecord p:
                {
                    var bytes = DomainName.ToBytes(p.PTRDName);
                    _bytes.AddRange(((ushort)bytes.Length).ToBytes());
                    _bytes.AddRange(DomainName.ToBytes(p.PTRDName));
                    break;
                }
                case SRVRecord s:
                {
                    var bytes = new List<byte>();
                    bytes.AddRange(s.Priority.ToBytes());
                    bytes.AddRange(s.Weight.ToBytes());
                    bytes.AddRange(s.Port.ToBytes());
                    bytes.AddRange(DomainName.ToBytes(s.Target));
                    
                    _bytes.AddRange(((ushort)bytes.Count).ToBytes());
                    _bytes.AddRange(bytes);
                    break;
                }
                case TXTRecord t:
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
            _bytes.AddRange(DomainName.ToBytes(question.QName));
            _bytes.AddRange(((ushort) question.QType).ToBytes());
            _bytes.AddRange(((ushort) question.QClass).ToBytes());
        }
    }
}