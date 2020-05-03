using System.Collections.Generic;
using System.Linq;
using System.Text;
using Convenient.Gooday.Domain.Records;
using Convenient.Gooday.Domain.Types;

namespace Convenient.Gooday.Domain
{
    public class MessageReader
    {
        private readonly byte[] _bytes;
        private int _index;

        public MessageReader(byte[] bytes) : this(bytes, 0)
        {
        }
        
        public MessageReader(byte[] bytes, int index)
        {
            _bytes = bytes;
            _index = index;
        }

        public ZeroconfMessage Read()
        {
            var header = ReadHeader();
            return ReadMessage(header);
        }

        private ZeroconfMessage ReadMessage(Header header)
        {
            var request = new ZeroconfMessage
            {
                Id = header.Id,
                Type = header.QR ? MessageType.Response : MessageType.Query,
                OpCode = header.OPCODE,
                AuthoriativeAnswer = header.AA,
                Truncation = header.TC,
                RecursionDesired = header.RD,
                RecursionAvailable = header.RA,
                ResponseCode = header.RCODE
            };
            
            for (var ii = 0; ii < header.QDCount; ii++)
            {
                request.Questions.Add(ReadQuestion());
            }

            for (var ii = 0; ii < header.ANCount; ii++)
            {
                request.Answers.Add(ReadResourceRecord());
            }
            
            for (var ii = 0; ii < header.NSCount; ii++)
            {
                request.Authorities.Add(ReadResourceRecord());
            }
            
            for (var ii = 0; ii < header.ARCount; ii++)
            {
                request.Additionals.Add(ReadResourceRecord());
            }

            return request;
        }

        private ResourceRecord ReadResourceRecord()
        {
            var record = new ResourceRecord
            {
                Name = ReadDomainName(),
                Type = (RRType) ReadUint16(),
                Class = (Class) ReadUint16(),
                Ttl = ReadUint32(),
            };
            var rdLength = ReadUint16();
            record.Record = ReadRecord(record.Type, rdLength);
            return record;
        }

        private IRecord ReadRecord(RRType type, ushort rdLength)
        {
            switch (type)
            {
                case RRType.A:
                    return new ARecord
                    {
                        Address = $"{ReadByte()}.{ReadByte()}.{ReadByte()}.{ReadByte()}"
                    };
                case RRType.AAAA:
                    return new AAAARecord
                    {
                        Address = $"{ReadUint16():x}:{ReadUint16():x}:{ReadUint16():x}:{ReadUint16():x}:{ReadUint16():x}:{ReadUint16():x}:{ReadUint16():x}:{ReadUint16():x}"
                    };
                case RRType.TXT:
                    return new TextRecord
                    {
                        Text = ReadText(rdLength)
                    };
                case RRType.SRV:
                    return new ServiceRecord
                    {
                        Priority = ReadUint16(),
                        Weight = ReadUint16(),
                        Port = ReadUint16(),
                        Target = ReadDomainName()
                    };
                case RRType.PTR:
                    return new PointerRecord
                    {
                        PTRDName = ReadDomainName()
                    };
                case RRType.NSEC:
                    return new NSECRecord
                    {
                        RData = _bytes.Skip(_index).ToArray()
                    };
                case RRType.MF:
                    return new MailForwarderRecord();
                default:
                    return new UnknownRecord
                    {
                        Type = type,
                        RData = _bytes.Skip(_index).ToArray()
                    };
            }
        }

        private List<string> ReadText(int length)
        {
            var text = new List<string>();
            var position = _index;
            while (_index - position < length &&
                   _index < _bytes.Length)
            {
                text.Add(ReadString());
            }
            return text;
        }

        private string ReadString()
        {
            short length = ReadByte();
            var bytes = new List<byte>();
            for (var ii = 0; ii < length; ii++)
            {
                bytes.Add(ReadByte());
            }
            return Encoding.UTF8.GetString(bytes.ToArray(), 0, bytes.Count);
        }


        private Question ReadQuestion()
        {
            return new Question
            {
                QName = ReadDomainName(),
                QType = (QType) ReadUint16(),
                QClass = (QClass) ReadUint16()
            };
        }

        private string ReadDomainName()
        {
            var bytes = new List<byte>();
            int length;

            // get  the length of the first label
            while ((length = ReadByte()) != 0)
            {
            	// top 2 bits set denotes domain name compression and to reference elsewhere
            	if ((length & 0xc0) == 0xc0)
            	{
            		// work out the existing domain name, copy this pointer
            		var subReader = new MessageReader(_bytes, (length & 0x3f) << 8 | ReadByte());
            		if (bytes.Count > 0)
            		{
            			return Encoding.UTF8.GetString(bytes.ToArray(), 0, bytes.Count) + subReader.ReadDomainName();
            		}
            		return subReader.ReadDomainName();
            	}

            	// if not using compression, copy a char at a time to the domain name
            	while (length > 0)
            	{
            		bytes.Add(ReadByte());
            		length--;
            	}
            	bytes.Add((byte)'.');
            }

            return bytes.Count == 0
                ? "." :
                Encoding.UTF8.GetString(bytes.ToArray(), 0, bytes.Count);
        }

        private Header ReadHeader()
        {
            return new Header
            {
                Id = ReadUint16(),
                Flags = ReadUint16(),
                QDCount = ReadUint16(),
                ANCount = ReadUint16(),
                NSCount = ReadUint16(),
                ARCount = ReadUint16()
            };
        }
        
        private uint ReadUint32() => (uint)(ReadUint16() << 16 | ReadUint16());
        private ushort ReadUint16() => (ushort) (ReadByte() << 8 | ReadByte());
        private byte ReadByte() => _index >= _bytes.Length ? (byte) 0 : _bytes[_index++];
    }
}