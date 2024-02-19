using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;

namespace GmwServerTests;

public class MailAddressEqualityComparer : IEqualityComparer<MailAddress>
{
    public static MailAddressEqualityComparer Instance => new();
    public bool Equals(MailAddress? x, MailAddress? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;

        return
            string.Equals(x.User, y.User, StringComparison.InvariantCultureIgnoreCase)
            && string.Equals(x.Host, y.Host, StringComparison.InvariantCultureIgnoreCase);
    }

    public int GetHashCode([DisallowNull] MailAddress obj) => obj.GetHashCode();
}