namespace Convenient.ZeroConf.Domain.Types
{
    public enum QType : ushort
	{
		A = RRType.A,			// a IPV4 host address
		NS = RRType.NS,		// an authoritative name server
		MD = RRType.MD,		// a mail destination (Obsolete - use MX)
		MF = RRType.MF,		// a mail forwarder (Obsolete - use MX)
		CNAME = RRType.CNAME,	// the canonical name for an alias
		SOA = RRType.SOA,		// marks the start of a zone of authority
		MB = RRType.MB,		// a mailbox domain name (EXPERIMENTAL)
		MG = RRType.MG,		// a mail group member (EXPERIMENTAL)
		MR = RRType.MR,		// a mail rename domain name (EXPERIMENTAL)
		NULL = RRType.NULL,	// a null RR (EXPERIMENTAL)
		WKS = RRType.WKS,		// a well known service description
		PTR = RRType.PTR,		// a domain name pointer
		HINFO = RRType.HINFO,	// host information
		MINFO = RRType.MINFO,	// mailbox or mail list information
		MX = RRType.MX,		// mail exchange
		TXT = RRType.TXT,		// text strings

		RP = RRType.RP,		// The Responsible Person rfc1183
		AFSDB = RRType.AFSDB,	// AFS Data Base location
		X25 = RRType.X25,		// X.25 address rfc1183
		ISDN = RRType.ISDN,	// ISDN address rfc1183
		RT = RRType.RT,		// The Route Through rfc1183

		NSAP = RRType.NSAP,	// Network service access point address rfc1706
		NSAP_PTR = RRType.NSAPPTR, // Obsolete, rfc1348

		SIG = RRType.SIG,		// Cryptographic public key signature rfc2931 / rfc2535
		KEY = RRType.KEY,		// Public key as used in DNSSEC rfc2535

		PX = RRType.PX,		// Pointer to X.400/RFC822 mail mapping information rfc2163

		GPOS = RRType.GPOS,	// Geographical position rfc1712 (obsolete)

		AAAA = RRType.AAAA,	// a IPV6 host address

		LOC = RRType.LOC,		// Location information rfc1876

		NXT = RRType.NXT,		// Obsolete rfc2065 / rfc2535

		EID = RRType.EID,		// *** Endpoint Identifier (Patton)
		NIMLOC = RRType.NIMLOC,// *** Nimrod Locator (Patton)

		SRV = RRType.SRV,		// Location of services rfc2782

		ATMA = RRType.ATMA,	// *** ATM Address (Dobrowski)

		NAPTR = RRType.NAPTR,	// The Naming Authority Pointer rfc3403

		KX = RRType.KX,		// Key Exchange Delegation Record rfc2230

		CERT = RRType.CERT,	// *** CERT RFC2538

		A6 = RRType.A6,		// IPv6 address rfc3363
		DNAME = RRType.DNAME,	// A way to provide aliases for a whole domain, not just a single domain name as with CNAME. rfc2672

		SINK = RRType.SINK,	// *** SINK Eastlake
		OPT = RRType.OPT,		// *** OPT RFC2671

		APL = RRType.APL,		// *** APL [RFC3123]

		DS = RRType.DS,		// Delegation Signer rfc3658

		SSHFP = RRType.SSHFP,	// *** SSH Key Fingerprint RFC-ietf-secsh-dns
		IPSECKEY = RRType.IPSECKEY, // rfc4025
		RRSIG = RRType.RRSIG,	// *** RRSIG RFC-ietf-dnsext-dnssec-2535
		NSEC = RRType.NSEC,	// *** NSEC RFC-ietf-dnsext-dnssec-2535
		DNSKEY = RRType.DNSKEY,// *** DNSKEY RFC-ietf-dnsext-dnssec-2535
		DHCID = RRType.DHCID,	// rfc4701

		NSEC3 = RRType.NSEC3,	// RFC5155
		NSEC3PARAM = RRType.NSEC3PARAM, // RFC5155

		HIP = RRType.HIP,		// RFC-ietf-hip-dns-09.txt

		SPF = RRType.SPF,		// RFC4408
		UINFO = RRType.UINFO,	// *** IANA-Reserved
		UID = RRType.UID,		// *** IANA-Reserved
		GID = RRType.GID,		// *** IANA-Reserved
		UNSPEC = RRType.UNSPEC,// *** IANA-Reserved

		TKEY = RRType.TKEY,	// Transaction key rfc2930
		TSIG = RRType.TSIG,	// Transaction signature rfc2845

		IXFR = 251,			// incremental transfer                  [RFC1995]
		AXFR = 252,			// transfer of an entire zone            [RFC1035]
		MAILB = 253,		// mailbox-related RRs (MB, MG or MR)    [RFC1035]
		MAILA = 254,		// mail agent RRs (Obsolete - see MX)    [RFC1035]
		ANY = 255,			// A request for all records             [RFC1035]

		TA = RRType.TA,		// DNSSEC Trust Authorities    [Weiler]  13 December 2005
		DLV = RRType.DLV		// DNSSEC Lookaside Validation [RFC4431]
	}
}