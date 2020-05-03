namespace Convenient.ZeroConf.Domain
{
    /// <summary>
    /// RFC 1035
    /// </summary>
    public enum ResponseCode
    {
        NoError = 0,
        FormatErr = 1,
        ServerFailure = 2,
        NameError = 3,
        NotImplemented = 4,
        Refused = 5,
    }
}