using AOSharp.Common.GameData;
using SmokeLounge.AOtomation.Messaging.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;

namespace SmokeLounge.AOtomation.Messaging.Serialization.Serializers.Custom
{
    // Reads/writes the N3 header plus the fields shared by IDynelFullUpdate messages,
    // following the client's reader at Gamecode.dll 0x100a143b.
    static class DynelFullUpdateCodec
    {
        public static void Read(StreamReader streamReader, N3Message message)
        {
            IDynelFullUpdate update = (IDynelFullUpdate)message;
            message.N3MessageType = (N3MessageType)streamReader.ReadInt32();
            message.Identity = ReadIdentity(streamReader);
            message.Unknown = streamReader.ReadByte();

            update.Version = streamReader.ReadInt32();
            update.Owner = ReadIdentity(streamReader);

            if (update.Owner.Type == 0)
            {
                update.Position = new Vector3(streamReader.ReadSingle(), streamReader.ReadSingle(), streamReader.ReadSingle());
                Quaternion rotation = new Quaternion(streamReader.ReadSingle(), streamReader.ReadSingle(), streamReader.ReadSingle(), streamReader.ReadSingle());

                // The client treats an all-zero rotation as identity
                update.Rotation = rotation.X == 0 && rotation.Y == 0 && rotation.Z == 0 && rotation.W == 0 ? Quaternion.Identity : rotation;
            }

            update.Playfield = streamReader.ReadInt32();
            update.StateMachine = ReadIdentity(streamReader);
            update.InventoryId = streamReader.ReadByte();
            update.CurrBodyLocation = streamReader.ReadByte();

            update.Stats = new GameTuple<Stat, int>[ReadCount(streamReader)];
            for (int i = 0; i < update.Stats.Length; i++)
                update.Stats[i] = new GameTuple<Stat, int> { Value1 = (Stat)streamReader.ReadInt32(), Value2 = streamReader.ReadInt32() };

            int blobLength = streamReader.ReadInt32();
            update.Blob = blobLength > 0 ? streamReader.ReadBytes(blobLength) : new byte[0];
        }

        public static void Write(StreamWriter streamWriter, N3Message message)
        {
            IDynelFullUpdate update = (IDynelFullUpdate)message;
            streamWriter.WriteInt32((int)message.N3MessageType);
            WriteIdentity(streamWriter, message.Identity);
            streamWriter.WriteByte(message.Unknown);

            streamWriter.WriteInt32(update.Version);
            WriteIdentity(streamWriter, update.Owner);

            if (update.Owner.Type == 0)
            {
                streamWriter.WriteSingle(update.Position.X);
                streamWriter.WriteSingle(update.Position.Y);
                streamWriter.WriteSingle(update.Position.Z);
                streamWriter.WriteSingle(update.Rotation.X);
                streamWriter.WriteSingle(update.Rotation.Y);
                streamWriter.WriteSingle(update.Rotation.Z);
                streamWriter.WriteSingle(update.Rotation.W);
            }

            streamWriter.WriteInt32(update.Playfield);
            WriteIdentity(streamWriter, update.StateMachine);
            streamWriter.WriteByte(update.InventoryId);
            streamWriter.WriteByte(update.CurrBodyLocation);

            GameTuple<Stat, int>[] stats = update.Stats ?? new GameTuple<Stat, int>[0];
            WriteCount(streamWriter, stats.Length);
            foreach (GameTuple<Stat, int> stat in stats)
            {
                streamWriter.WriteInt32((int)stat.Value1);
                streamWriter.WriteInt32(stat.Value2);
            }

            byte[] blob = update.Blob ?? new byte[0];
            streamWriter.WriteInt32(blob.Length);
            streamWriter.WriteBytes(blob);
        }

        public static Identity ReadIdentity(StreamReader streamReader)
        {
            return new Identity((IdentityType)streamReader.ReadInt32(), streamReader.ReadInt32());
        }

        public static void WriteIdentity(StreamWriter streamWriter, Identity identity)
        {
            streamWriter.WriteInt32((int)identity.Type);
            streamWriter.WriteInt32(identity.Instance);
        }

        // Same encoding as ArraySizeType.X3F1: (n + 1) * 1009
        public static int ReadCount(StreamReader streamReader)
        {
            return streamReader.ReadInt32() / 0x03F1 - 1;
        }

        public static void WriteCount(StreamWriter streamWriter, int count)
        {
            streamWriter.WriteInt32((count + 1) * 0x03F1);
        }
    }
}
