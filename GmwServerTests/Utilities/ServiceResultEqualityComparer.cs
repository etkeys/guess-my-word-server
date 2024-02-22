
using System.Diagnostics.CodeAnalysis;
using GmwServer;

namespace GmwServerTests;

public class ServiceResultEqaulityComparer : IEqualityComparer<IServiceResult>
{
    public delegate bool OverrideComparer(object x, object y);
    private readonly OverrideComparer? _dataComparer;
    private readonly OverrideComparer? _errorComparer;

    public ServiceResultEqaulityComparer(
        OverrideComparer? dataComparer = null,
        OverrideComparer? errorComparer = null) {
            _dataComparer = dataComparer;
            _errorComparer = errorComparer;
        }

    public bool Equals(IServiceResult? x, IServiceResult? y)
    {
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;

        if (x.Status != y.Status) return false;
        if (x.IsError != y.IsError) return false;

        if (x.GetError() is null ^ y.GetError() is null) return false;
        var xError = x.GetError();
        var yError = y.GetError();
        var errorResult =
            (xError is null && yError is null)
            || (_errorComparer?.Invoke(xError!, yError!) ?? xError!.Equals(yError!));

        if (!errorResult) return false;

        if (x.GetData() is null ^ y.GetData() is null) return false;
        var xData = x.GetData();
        var yData = y.GetData();
        var dataResult =
            (xData is null && yData is null)
            || (_dataComparer?.Invoke(xData!, yData!) ?? xData!.Equals(yData!));

        if (!dataResult) return false;

        return true;
    }

    public int GetHashCode([DisallowNull] IServiceResult obj)
    {
        unchecked{
            int hash = 999331;
            hash = hash * 331999 + obj.IsError.GetHashCode();
            hash = hash * 331999 + obj.Status.GetHashCode();
            hash = hash * 331999 + obj.GetError()?.GetHashCode() ?? 0;
            hash = hash * 331999 + obj.GetData()?.GetHashCode() ?? 0;
            return hash;
        }
    }
}