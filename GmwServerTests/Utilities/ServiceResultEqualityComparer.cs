
using System.Diagnostics.CodeAnalysis;
using GmwServer;

namespace GmwServerTests;


[Obsolete("Use direct property comparisions instead")]
public class ServiceResultEqaulityComparer<T>: IEqualityComparer<IServiceResult<T>>
{
    public delegate bool OverrideDataComparer(T x, T y);
    public delegate bool OverrideErrorComparer(string x, string y);

    private readonly OverrideDataComparer? _dataComparer;
    private readonly OverrideErrorComparer? _errorComparer;
    public ServiceResultEqaulityComparer(
        OverrideDataComparer? dataComparer = null,
        OverrideErrorComparer? errorComparer = null) {
            _dataComparer = dataComparer;
            _errorComparer = errorComparer;
        }

    public bool Equals(IServiceResult<T>? x, IServiceResult<T>? y){
        if (x is null && y is null) return true;
        if (x is null || y is null) return false;

        if (x.Status != y.Status) return false;
        if (x.IsError != y.IsError) return false;


        if (x.Error is null ^ y.Error is null) return false;
        var errorResult =
            (x.Error is null && y.Error is null)
            || (_errorComparer?.Invoke(x.Error!, y.Error!) ?? x.Error!.Equals(y.Error!));
        if (!errorResult) return false;


        if (x.Data is null ^ y.Data is null) return false;
        var dataResult =
            (x.Data is null && y.Data is null)
            || (_dataComparer?.Invoke(x.Data!, y.Data!) ?? x.Data!.Equals(y.Data!));
        if (!dataResult) return false;

        return true;
    }

    public int GetHashCode([DisallowNull] IServiceResult<T> obj){
        unchecked{
            int hash = 999331;
            hash = hash * 331999 + obj.IsError.GetHashCode();
            hash = hash * 331999 + obj.Status.GetHashCode();
            hash = hash * 331999 + obj.Error?.GetHashCode() ?? 0;
            hash = hash * 331999 + obj.Data?.GetHashCode() ?? 0;
            return hash;
        }
    }

}