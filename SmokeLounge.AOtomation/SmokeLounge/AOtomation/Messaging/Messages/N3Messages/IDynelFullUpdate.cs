using AOSharp.Common.GameData;
using SmokeLounge.AOtomation.Messaging.GameData;

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    // Fields every item/door full update starts with. The client reads them in one shared function
    // (Gamecode.dll 0x100a143b) before any message-specific fields.
    // Messages implement this instead of sharing a base class because contracts are only discovered
    // on direct subclasses of N3Message.
    public interface IDynelFullUpdate
    {
        // Client ignores the message unless this is 11
        int Version { get; set; }

        Identity Owner { get; set; }

        // Position and Rotation are only on the wire when Owner.Type == 0
        Vector3 Position { get; set; }

        Quaternion Rotation { get; set; }

        int Playfield { get; set; }

        Identity StateMachine { get; set; }

        // Stat 55
        byte InventoryId { get; set; }

        // Stat 220
        byte CurrBodyLocation { get; set; }

        GameTuple<Stat, int>[] Stats { get; set; }

        byte[] Blob { get; set; }
    }
}
