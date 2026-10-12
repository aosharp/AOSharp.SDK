// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuestStream.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the QuestStream type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.Serialization.Serializers.Custom
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    using SmokeLounge.AOtomation.Messaging.GameData;

    /// <summary>
    /// The quest bodies the quest messages share (Quest_t, its reward and its actions), read and written as the client
    /// does. A read the client rejects (it throws a DataStreamException_t, or puts its stream in error) fails the
    /// message: Read* returns false after <see cref="StreamReader.Fail"/>.
    /// </summary>
    internal static class QuestStream
    {
        #region Constants

        /// <summary>A list's count: (n + 1) * 0x3f1.</summary>
        private const int ListUnit = 0x3f1;

        #endregion

        #region Public Methods and Operators

        public static Identity ReadIdentity(StreamReader r)
        {
            return new Identity { Type = (IdentityType)r.ReadInt32(), Instance = r.ReadInt32() };
        }

        public static void WriteIdentity(StreamWriter w, Identity identity)
        {
            w.WriteInt32((int)identity.Type);
            w.WriteInt32(identity.Instance);
        }

        /// <summary>The quest list: a positive 0x3F1 count, then each quest's identity and body.</summary>
        public static bool ReadQuestList(StreamReader r, out Quest[] quests)
        {
            quests = new Quest[0];
            var count = ReadCount(r, true);
            if (count < 0)
            {
                return Fail(r);
            }

            var list = new List<Quest>(count);
            for (var i = 0; i < count; i++)
            {
                var quest = new Quest { Identity = ReadIdentity(r) };
                if (!ReadQuest(r, quest))
                {
                    return false;
                }

                list.Add(quest);
            }

            quests = list.ToArray();
            return true;
        }

        public static void WriteQuestList(StreamWriter w, Quest[] quests)
        {
            quests = quests ?? new Quest[0];
            w.WriteInt32((quests.Length + 1) * ListUnit);
            foreach (var quest in quests)
            {
                WriteIdentity(w, quest.Identity);
                WriteQuest(w, quest);
            }
        }

        /// <summary>Quest_t's body, versions 7 to 15.</summary>
        public static bool ReadQuest(StreamReader r, Quest q)
        {
            var version = r.ReadInt32();
            if ((uint)(version - 7) > 8)
            {
                return Fail(r);
            }

            q.Version = version;
            q.Unknown1 = r.ReadInt32();
            q.Unknown2 = r.ReadInt32();
            q.Flags = r.ReadInt32();
            q.Name = ReadCString(r, 31);

            var length = r.ReadInt32();
            if ((uint)(length - 1) >= 0x1000)
            {
                return Fail(r);
            }

            q.Description = r.ReadString(length);
            q.Identity38 = ReadIdentity(r);
            QuestReward reward;
            if (!ReadReward(r, out reward))
            {
                return false;
            }

            q.Reward = reward;
            q.Identity40 = ReadIdentity(r);
            q.Icon = r.ReadInt32();
            q.Unknown70 = r.ReadInt32();
            q.Unknown74 = r.ReadInt32();

            List<QuestActionInfo> actions;
            if (!ReadActionList(r, out actions))
            {
                return false;
            }

            q.Actions = actions;
            List<Identity> receivers;
            if (!ReadIdentityList(r, out receivers))
            {
                return false;
            }

            q.Receivers = receivers;

            var n = r.ReadInt32();
            for (var i = 0; i < n && !r.Failed; i++)
            {
                q.Ints60.Add(r.ReadInt32() & 0x7ffffff);
            }

            n = r.ReadInt32();
            for (var i = 0; i < n && !r.Failed; i++)
            {
                q.Ints50.Add(r.ReadInt32() & 0x7ffffff);
            }

            // An identity and a length-prefixed block each.
            n = r.ReadInt32();
            for (var i = 0; i < n && !r.Failed; i++)
            {
                var block = new QuestBlock { Identity = ReadIdentity(r) };
                var blockLength = r.ReadInt32();
                if (blockLength > 0)
                {
                    block.Data = r.ReadBytes(blockLength);
                }

                q.Blocks.Add(block);
            }

            q.UnknownA8 = version >= 8 ? r.ReadInt32() : 6;
            if (version >= 9)
            {
                List<Identity> receivers2;
                if (!ReadIdentityList(r, out receivers2))
                {
                    return false;
                }

                q.Receivers2 = receivers2;
            }

            q.Unknown4C = version >= 10 ? r.ReadInt32() : 0;
            q.UnknownB0 = version >= 11 ? r.ReadInt32() : 0;
            if (version >= 12)
            {
                q.IdentityB4 = ReadIdentity(r);
                q.UnknownBC = r.ReadInt32();
                q.UnknownC0 = r.ReadInt32();
            }

            if (version >= 13)
            {
                // A plain count, then (identity, int) pairs.
                for (var pairs = r.ReadInt32(); pairs != 0 && !r.Failed; pairs--)
                {
                    var id = ReadIdentity(r);
                    q.PairsC8.Add(new KeyValuePair<Identity, int>(id, r.ReadInt32()));
                }
            }

            q.UnknownD4 = version >= 14 ? r.ReadInt32() : 0;
            if (version >= 15)
            {
                // A positive 0x3F1 count (else nothing more is read), then that many identities.
                q.IdentitiesD8 = new List<Identity>();
                var count = ReadCount(r, true);
                for (var i = 0; i < count; i++)
                {
                    q.IdentitiesD8.Add(ReadIdentity(r));
                }
            }

            return true;
        }

        public static void WriteQuest(StreamWriter w, Quest q)
        {
            var version = q.Version;
            w.WriteInt32(version);
            w.WriteInt32(q.Unknown1);
            w.WriteInt32(q.Unknown2);
            w.WriteInt32(q.Flags);
            var name = q.Name ?? string.Empty;
            w.WriteString(name.Length > 31 ? name.Substring(0, 31) : name);
            w.WriteByte(0);
            var description = q.Description ?? string.Empty;
            w.WriteInt32(description.Length);
            w.WriteString(description);
            WriteIdentity(w, q.Identity38);
            WriteReward(w, q.Reward ?? new QuestReward());
            WriteIdentity(w, q.Identity40);
            w.WriteInt32(q.Icon);
            w.WriteInt32(q.Unknown70);
            w.WriteInt32(q.Unknown74);

            var actions = q.Actions ?? new List<QuestActionInfo>();
            w.WriteInt32((actions.Count + 1) * ListUnit);
            foreach (var action in actions)
            {
                WriteAction(w, action);
            }

            WriteIdentityList(w, q.Receivers);
            WriteInts(w, q.Ints60);
            WriteInts(w, q.Ints50);
            var blocks = q.Blocks ?? new List<QuestBlock>();
            w.WriteInt32(blocks.Count);
            foreach (var block in blocks)
            {
                WriteIdentity(w, block.Identity);
                var data = block.Data ?? new byte[0];
                w.WriteInt32(data.Length);
                w.WriteBytes(data);
            }
            if (version >= 8)
            {
                w.WriteInt32(q.UnknownA8);
            }

            if (version >= 9)
            {
                WriteIdentityList(w, q.Receivers2);
            }

            if (version >= 10)
            {
                w.WriteInt32(q.Unknown4C);
            }

            if (version >= 11)
            {
                w.WriteInt32(q.UnknownB0);
            }

            if (version >= 12)
            {
                WriteIdentity(w, q.IdentityB4);
                w.WriteInt32(q.UnknownBC);
                w.WriteInt32(q.UnknownC0);
            }

            if (version >= 13)
            {
                var pairs = q.PairsC8 ?? new List<KeyValuePair<Identity, int>>();
                w.WriteInt32(pairs.Count);
                foreach (var pair in pairs)
                {
                    WriteIdentity(w, pair.Key);
                    w.WriteInt32(pair.Value);
                }
            }

            if (version >= 14)
            {
                w.WriteInt32(q.UnknownD4);
            }

            if (version >= 15)
            {
                WriteIdentityList(w, q.IdentitiesD8);
            }
        }

        #endregion

        #region Methods

        private static bool Fail(StreamReader r)
        {
            r.Fail();
            return false;
        }

        /// <summary>A count as the client's lists send it: n, or -1 when not a multiple of 0x3f1 (or, positive, not above 0).</summary>
        private static int ReadCount(StreamReader r, bool positive)
        {
            var raw = r.ReadInt32();
            if (raw % ListUnit != 0 || (positive && raw <= 0))
            {
                return -1;
            }

            return raw / ListUnit - 1;
        }

        /// <summary>Bytes up to and with the NUL; the client keeps at most <paramref name="keep"/> of them.</summary>
        private static string ReadCString(StreamReader r, int keep)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var b = r.ReadByte();
                if (b == 0)
                {
                    break;
                }

                if (sb.Length < keep)
                {
                    sb.Append((char)b);
                }
            }

            return sb.ToString();
        }

        /// <summary>An identity list: a multiple of 0x3f1 with at most 30000 entries, else the read fails.</summary>
        private static bool ReadIdentityList(StreamReader r, out List<Identity> list)
        {
            list = new List<Identity>();
            var raw = r.ReadUInt32();
            var count = raw / ListUnit - 1;
            if (raw % ListUnit != 0 || count > 30000)
            {
                return Fail(r);
            }

            for (uint i = 0; i < count; i++)
            {
                list.Add(ReadIdentity(r));
            }

            return true;
        }

        private static void WriteIdentityList(StreamWriter w, List<Identity> list)
        {
            list = list ?? new List<Identity>();
            w.WriteInt32((list.Count + 1) * ListUnit);
            foreach (var identity in list)
            {
                WriteIdentity(w, identity);
            }
        }

        private static void WriteInts(StreamWriter w, List<int> list)
        {
            list = list ?? new List<int>();
            w.WriteInt32(list.Count);
            foreach (var value in list)
            {
                w.WriteInt32(value);
            }
        }

        /// <summary>The quest's reward, versions 3 to 6.</summary>
        private static bool ReadReward(StreamReader r, out QuestReward reward)
        {
            reward = new QuestReward();
            var version = r.ReadInt32();
            if ((uint)(version - 3) > 3)
            {
                return Fail(r);
            }

            reward.Version = version;
            reward.Cash = r.ReadInt32();
            reward.Unknown1 = r.ReadInt32();
            reward.Experience = r.ReadInt32();

            List<Identity> identities1;
            List<Identity> identities2;
            if (!ReadIdentityList(r, out identities1) || !ReadIdentityList(r, out identities2))
            {
                return false;
            }

            reward.Identities1 = identities1;
            reward.Identities2 = identities2;

            var raw = r.ReadUInt32();
            var count = raw / ListUnit - 1;
            if (raw % ListUnit != 0 || count > 30000)
            {
                return Fail(r);
            }

            for (uint i = 0; i < count; i++)
            {
                reward.Items.Add(ReadAcgItem(r));
            }

            if (version >= 4)
            {
                reward.Unknown08 = r.ReadInt32();
                reward.Unknown14 = r.ReadInt32();
                reward.Unknown18 = r.ReadInt32();
            }

            if (version >= 5)
            {
                reward.Unknown0C = r.ReadInt32();
                reward.Unknown10 = r.ReadInt32();
            }

            if (version >= 6)
            {
                reward.Item28 = ReadAcgItem(r);
            }

            return true;
        }

        private static void WriteReward(StreamWriter w, QuestReward reward)
        {
            var version = reward.Version;
            w.WriteInt32(version);
            w.WriteInt32(reward.Cash);
            w.WriteInt32(reward.Unknown1);
            w.WriteInt32(reward.Experience);
            WriteIdentityList(w, reward.Identities1);
            WriteIdentityList(w, reward.Identities2);
            var items = reward.Items ?? new List<AcgItem>();
            w.WriteInt32((items.Count + 1) * ListUnit);
            foreach (var item in items)
            {
                WriteAcgItem(w, item);
            }

            if (version >= 4)
            {
                w.WriteInt32(reward.Unknown08);
                w.WriteInt32(reward.Unknown14);
                w.WriteInt32(reward.Unknown18);
            }

            if (version >= 5)
            {
                w.WriteInt32(reward.Unknown0C);
                w.WriteInt32(reward.Unknown10);
            }

            if (version >= 6)
            {
                WriteAcgItem(w, reward.Item28 ?? new AcgItem());
            }
        }

        /// <summary>An ACGItem_t: low, high (the low id when 0), quality (masked to 0x1ff above it), an int.</summary>
        private static AcgItem ReadAcgItem(StreamReader r)
        {
            var item = new AcgItem { LowId = r.ReadInt32(), HighId = r.ReadInt32() };
            var quality = r.ReadUInt32();
            item.Unknown1 = r.ReadInt32();
            if (quality > 0x1ff)
            {
                quality &= 0x1ff;
            }

            if (item.HighId == 0)
            {
                item.HighId = item.LowId;
            }

            item.Quality = (int)quality;
            return item;
        }

        private static void WriteAcgItem(StreamWriter w, AcgItem item)
        {
            w.WriteInt32(item.LowId);
            w.WriteInt32(item.HighId);
            w.WriteInt32(item.Quality);
            w.WriteInt32(item.Unknown1);
        }

        /// <summary>The action list: a positive 0x3F1 count (else nothing is read), then each QuestAction_t.</summary>
        private static bool ReadActionList(StreamReader r, out List<QuestActionInfo> actions)
        {
            actions = new List<QuestActionInfo>();
            var count = ReadCount(r, true);
            for (var i = 0; i < count; i++)
            {
                if (r.Failed)
                {
                    return Fail(r);
                }

                var a = new QuestActionInfo();
                a.Type = r.ReadInt32();
                a.Identity04 = ReadIdentity(r);
                a.Identity0C = ReadIdentity(r);
                a.Identity14 = ReadIdentity(r);
                a.Identity1C = ReadIdentity(r);
                a.Identity24 = ReadIdentity(r);
                for (var j = 0; j < 4; j++)
                {
                    a.Floats2C[j] = r.ReadSingle();
                }

                a.Identity3C = ReadIdentity(r);
                for (var j = 0; j < 4; j++)
                {
                    a.Floats44[j] = r.ReadSingle();
                }

                a.Identity54 = ReadIdentity(r);
                a.ExpiryTime = r.ReadUInt32();
                a.Unknown60 = r.ReadInt32();
                a.Identity64 = ReadIdentity(r);

                var pos = new QuestWorldPos { Playfield = ReadIdentity(r), X = r.ReadInt32(), Z = r.ReadInt32() };
                pos.Offset = new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
                if (!pos.IsValid || r.Failed)
                {
                    return Fail(r);
                }

                a.WorldPos = pos;
                actions.Add(a);
            }

            return true;
        }

        private static void WriteAction(StreamWriter w, QuestActionInfo a)
        {
            w.WriteInt32(a.Type);
            WriteIdentity(w, a.Identity04);
            WriteIdentity(w, a.Identity0C);
            WriteIdentity(w, a.Identity14);
            WriteIdentity(w, a.Identity1C);
            WriteIdentity(w, a.Identity24);
            for (var j = 0; j < 4; j++)
            {
                w.WriteSingle(a.Floats2C != null && j < a.Floats2C.Length ? a.Floats2C[j] : 0f);
            }

            WriteIdentity(w, a.Identity3C);
            for (var j = 0; j < 4; j++)
            {
                w.WriteSingle(a.Floats44 != null && j < a.Floats44.Length ? a.Floats44[j] : 0f);
            }

            WriteIdentity(w, a.Identity54);
            w.WriteUInt32(a.ExpiryTime);
            w.WriteInt32(a.Unknown60);
            WriteIdentity(w, a.Identity64);
            var pos = a.WorldPos ?? new QuestWorldPos();
            WriteIdentity(w, pos.Playfield);
            w.WriteInt32(pos.X);
            w.WriteInt32(pos.Z);
            w.WriteSingle(pos.Offset.X);
            w.WriteSingle(pos.Offset.Y);
            w.WriteSingle(pos.Offset.Z);
        }

        #endregion
    }
}
