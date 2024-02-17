using GmwServer;

namespace GmwServerTests;

public partial class RoomCodeIdProviderTests
{

    [Fact]
    public void GetRoomCodeTests(){
        var generator = new RoomJoinCodeProvider();
        var set = new HashSet<RoomJoinCode>();

        for(var i = 0; i < 10_000; i++){
            var act = generator.GetRoomJoinCode();
            Assert.True(set.Add(act), $"Duplicate room code generated: {act}");
        }
    }
}