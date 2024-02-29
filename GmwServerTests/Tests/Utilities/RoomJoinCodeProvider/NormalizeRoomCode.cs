
using GmwServer;

namespace GmwServerTests;

public partial class RoomCodeIdProviderTests
{
    [Theory]
    [InlineData("aaaaaaaa", "aaaaaaaa")]
    [InlineData("aaaaaaa", "aaaaaaa")]
    [InlineData("aaaaaaaaa", "aaaaaaaaa")]
    [InlineData("aaab1aaa", "aaab1aaa")]
    [InlineData("elnqrvaa", "ELNQRVaa")]
    [InlineData("abcdefghijklmnopqrstuvwxyz0123456789", "abcdEfghiJkLmNopQRstuVwxyz0123456789")]
    [InlineData("a`~!@#$%^&*()-_=+{}[];:'\"<>,.?/\\| \t", "a..................................")]
    [InlineData("", "")]
    public void NormalizeJoinCodeTests(string input, string expected){
        var actor = new RoomJoinCodeProvider();

        var act = actor.NormalizeJoinCode(new RoomJoinCode(input));
        var exp = new RoomJoinCode(expected);

        act.Should().Be(exp);
    }
}