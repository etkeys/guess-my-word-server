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
            set.Add(act).Should().BeTrue("because generating room codes should be random enough to not cause collisions.");
        }
    }
}