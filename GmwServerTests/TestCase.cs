
namespace GmwServerTests;

using Collection = Dictionary<string, object?>;

public class TestCase(string name): IEquatable<TestCase>
{
    private Collection _expected = new();
    private Collection _inputs = new();
    private Collection _setups = new();
    public Collection Expected => _expected;
    public Collection Inputs => _inputs;
    public Collection Setups => _setups;
    public string Name => name;

    public bool Equals(TestCase? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode() => StringComparer.OrdinalIgnoreCase.GetHashCode(Name);

    public override string ToString() => Name;

    public TestCase WithExpected(string key, object? value){
        _expected.Add(key, value);
        return this;
    }

    public TestCase WithInput(string key, object? value){
        _inputs.Add(key, value);
        return this;
    }

    public TestCase WithSetup(string key, object? value){
        _setups.Add(key, value);
        return this;
    }
}