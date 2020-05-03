namespace Convenient.Gooday.Domain.Types
{
    public enum OpCode
    {
        Query = 0,				// a standard query (QUERY)
        IQUERY = 1,				// OpCode Retired (previously IQUERY - No further [RFC3425]
        // assignment of this code available)
        Status = 2,				// a server status request (STATUS) RFC1035
        RESERVED3 = 3,			// IANA

        Notify = 4,				// RFC1996
        Update = 5,				// RFC2136

        RESERVED6 = 6,
        RESERVED7 = 7,
        RESERVED8 = 8,
        RESERVED9 = 9,
        RESERVED10 = 10,
        RESERVED11 = 11,
        RESERVED12 = 12,
        RESERVED13 = 13,
        RESERVED14 = 14,
        RESERVED15 = 15,
    }
}