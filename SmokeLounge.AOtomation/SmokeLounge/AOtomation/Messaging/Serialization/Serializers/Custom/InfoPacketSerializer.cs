// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InfoPacketSerializer.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the InfoPacketSerializer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Serialization.Serializers.Custom
{
    using System;
    using System.Linq.Expressions;
    using System.Collections.Generic;
    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Messages;
    using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;

    public class InfoPacketSerializer : ISerializer
    {
        #region Public Properties

        public Type Type
        {
            get
            {
                return typeof(InfoPacketMessage);
            }
        }

        #endregion

        #region Public Methods and Operators

        public object Deserialize(
            StreamReader streamReader,
            SerializationContext serializationContext,
            PropertyMetaData propertyMetaData = null)
        {
            var message = new InfoPacketMessage();
            message.N3MessageType = (N3MessageType)streamReader.ReadInt32();
            message.Identity = new Identity
            {
                Type = (IdentityType)streamReader.ReadInt32(),
                Instance = streamReader.ReadInt32()
            };
            message.Unknown = streamReader.ReadByte();

            var p = message.Info;
            p.Flags = streamReader.ReadByte();
            p.Unknown1 = (p.Flags & InfoPacket.Unknown1Flag) != 0 ? streamReader.ReadByte() : (byte)0;
            p.Profession = streamReader.ReadByte();
            p.ProfessionLevel = streamReader.ReadByte();
            p.TitleLevel = streamReader.ReadByte();
            if (p.Unknown1 == 0)
            {
                p.Unknown2 = streamReader.ReadByte();
            }

            p.VisualProfession = streamReader.ReadByte();
            p.SideXpBonus = streamReader.ReadUInt16();
            p.Health = streamReader.ReadInt32();
            p.MaxHealth = streamReader.ReadInt32();
            p.Taunt = streamReader.ReadInt32();
            p.Clan = streamReader.ReadInt32();
            p.FirstName = ReadString(streamReader);
            p.LastName = ReadString(streamReader);
            p.TitleName = ReadString(streamReader);
            p.Info = ReadString(streamReader);

            if ((p.Flags & InfoPacket.ClanFlag) != 0)
            {
                p.ClanLevel = ReadString(streamReader);
                if ((p.Flags & InfoPacket.DestinationsFlag) != 0)
                {
                    var count = ReadCount(streamReader);
                    for (var i = 0; i < count; i++)
                    {
                        var destination = new GridDestination
                        {
                            Unknown1 = streamReader.ReadInt32(),
                            Identity = new Identity
                            {
                                Type = (IdentityType)streamReader.ReadInt32(),
                                Instance = streamReader.ReadInt32()
                            },
                            Name = ReadString(streamReader),
                            Unknown2 = streamReader.ReadInt32(),
                            Unknown3 = streamReader.ReadInt32()
                        };
                        if (streamReader.Failed)
                        {
                            break;
                        }

                        p.Destinations.Add(destination);
                    }
                }

                p.ClanCity = streamReader.ReadInt32();
            }

            if ((p.Flags & InfoPacket.ItemsFlag) != 0)
            {
                var count = ReadCount(streamReader);
                for (var i = 0; i < count; i++)
                {
                    var item = new AcgItem { LowId = streamReader.ReadInt32(), HighId = streamReader.ReadInt32() };
                    var quality = streamReader.ReadUInt32();
                    item.Unknown1 = streamReader.ReadInt32();
                    if (quality > 0x1ff)
                    {
                        quality &= 0x1ff;
                    }

                    if (item.HighId == 0)
                    {
                        item.HighId = item.LowId;
                    }

                    item.Quality = (int)quality;
                    if (streamReader.Failed)
                    {
                        break;
                    }

                    p.Items.Add(item);
                }
            }

            if ((p.Flags & InfoPacket.GasFlag) != 0)
            {
                p.GasSeconds = streamReader.ReadInt32();
                p.Gas = streamReader.ReadByte();
            }

            if ((p.Flags & InfoPacket.FactionsFlag) != 0)
            {
                for (var i = 0; i < p.Factions.Length; i++)
                {
                    p.Factions[i] = streamReader.ReadInt32();
                }
            }

            p.InvadersKilled = streamReader.ReadInt32();
            p.KilledByInvaders = streamReader.ReadInt32();
            p.AlienLevel = streamReader.ReadInt32();

            if ((p.Flags & InfoPacket.NoPvpFlag) == 0)
            {
                for (var i = 0; i < p.PvpStats.Length; i++)
                {
                    p.PvpStats[i] = streamReader.ReadInt32();
                }
            }

            return message;
        }

        public Expression DeserializerExpression(
            ParameterExpression streamReaderExpression,
            ParameterExpression serializationContextExpression,
            Expression assignmentTargetExpression,
            PropertyMetaData propertyMetaData)
        {
            var deserializerMethodInfo =
                ReflectionHelper
                    .GetMethodInfo
                        <InfoPacketSerializer, Func<StreamReader, SerializationContext, PropertyMetaData, object>>
                        (o => o.Deserialize);
            var serializerExp = Expression.New(this.GetType());
            var callExp = Expression.Call(
                serializerExp,
                deserializerMethodInfo,
                new Expression[]
                {
                    streamReaderExpression, serializationContextExpression,
                    Expression.Constant(propertyMetaData, typeof(PropertyMetaData))
                });

            var assignmentExp = Expression.Assign(
                assignmentTargetExpression, Expression.TypeAs(callExp, assignmentTargetExpression.Type));
            return assignmentExp;
        }

        public Expression SerializerExpression(
            ParameterExpression streamWriterExpression,
            ParameterExpression serializationContextExpression,
            Expression valueExpression,
            PropertyMetaData propertyMetaData)
        {
            var serializerMethodInfo =
                ReflectionHelper
                    .GetMethodInfo
                        <InfoPacketSerializer, Action<StreamWriter, SerializationContext, object, PropertyMetaData>>
                        (o => o.Serialize);
            var serializerExp = Expression.New(this.GetType());
            var callExp = Expression.Call(
                serializerExp,
                serializerMethodInfo,
                new[]
                {
                    streamWriterExpression, serializationContextExpression, valueExpression,
                    Expression.Constant(propertyMetaData, typeof(PropertyMetaData))
                });
            return callExp;
        }

        public void Serialize(
            StreamWriter streamWriter,
            SerializationContext serializationContext,
            object value,
            PropertyMetaData propertyMetaData = null)
        {
            var message = (InfoPacketMessage)value;
            streamWriter.WriteInt32((int)message.N3MessageType);
            streamWriter.WriteInt32((int)message.Identity.Type);
            streamWriter.WriteInt32(message.Identity.Instance);
            streamWriter.WriteByte(message.Unknown);

            var p = message.Info ?? new InfoPacket();
            streamWriter.WriteByte(p.Flags);
            if ((p.Flags & InfoPacket.Unknown1Flag) != 0)
            {
                streamWriter.WriteByte(p.Unknown1);
            }

            streamWriter.WriteByte(p.Profession);
            streamWriter.WriteByte(p.ProfessionLevel);
            streamWriter.WriteByte(p.TitleLevel);
            if ((p.Flags & InfoPacket.Unknown1Flag) == 0 || p.Unknown1 == 0)
            {
                streamWriter.WriteByte(p.Unknown2);
            }

            streamWriter.WriteByte(p.VisualProfession);
            streamWriter.WriteUInt16(p.SideXpBonus);
            streamWriter.WriteInt32(p.Health);
            streamWriter.WriteInt32(p.MaxHealth);
            streamWriter.WriteInt32(p.Taunt);
            streamWriter.WriteInt32(p.Clan);
            WriteString(streamWriter, p.FirstName);
            WriteString(streamWriter, p.LastName);
            WriteString(streamWriter, p.TitleName);
            WriteString(streamWriter, p.Info);

            if ((p.Flags & InfoPacket.ClanFlag) != 0)
            {
                WriteString(streamWriter, p.ClanLevel);
                if ((p.Flags & InfoPacket.DestinationsFlag) != 0)
                {
                    var destinations = p.Destinations ?? new List<GridDestination>();
                    streamWriter.WriteInt32((destinations.Count + 1) * ListUnit);
                    foreach (var destination in destinations)
                    {
                        streamWriter.WriteInt32(destination.Unknown1);
                        streamWriter.WriteInt32((int)destination.Identity.Type);
                        streamWriter.WriteInt32(destination.Identity.Instance);
                        WriteString(streamWriter, destination.Name);
                        streamWriter.WriteInt32(destination.Unknown2);
                        streamWriter.WriteInt32(destination.Unknown3);
                    }
                }

                streamWriter.WriteInt32(p.ClanCity);
            }

            if ((p.Flags & InfoPacket.ItemsFlag) != 0)
            {
                var items = p.Items ?? new List<AcgItem>();
                streamWriter.WriteInt32((items.Count + 1) * ListUnit);
                foreach (var item in items)
                {
                    streamWriter.WriteInt32(item.LowId);
                    streamWriter.WriteInt32(item.HighId);
                    streamWriter.WriteInt32(item.Quality);
                    streamWriter.WriteInt32(item.Unknown1);
                }
            }

            if ((p.Flags & InfoPacket.GasFlag) != 0)
            {
                streamWriter.WriteInt32(p.GasSeconds);
                streamWriter.WriteByte(p.Gas);
            }

            if ((p.Flags & InfoPacket.FactionsFlag) != 0)
            {
                for (var i = 0; i < 12; i++)
                {
                    streamWriter.WriteInt32(p.Factions != null && i < p.Factions.Length ? p.Factions[i] : 0);
                }
            }

            streamWriter.WriteInt32(p.InvadersKilled);
            streamWriter.WriteInt32(p.KilledByInvaders);
            streamWriter.WriteInt32(p.AlienLevel);

            if ((p.Flags & InfoPacket.NoPvpFlag) == 0)
            {
                for (var i = 0; i < 8; i++)
                {
                    streamWriter.WriteInt32(p.PvpStats != null && i < p.PvpStats.Length ? p.PvpStats[i] : 0);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>The 0x3F1 list count: a multiple of 0x3f1 whose quotient less one is below 0x7531; another fails the read.</summary>
        private const int ListUnit = 0x3f1;

        private static int ReadCount(StreamReader streamReader)
        {
            var n = streamReader.ReadUInt32();
            if (n % ListUnit != 0 || n / ListUnit - 1 >= 0x7531)
            {
                streamReader.Fail();
                return 0;
            }

            return (int)(n / ListUnit - 1);
        }

        /// <summary>A 16-bit length, then the bytes; 0 is empty, 0x8000 and above is empty and fails the read.</summary>
        private static string ReadString(StreamReader streamReader)
        {
            var length = streamReader.ReadUInt16();
            if (length == 0)
            {
                return string.Empty;
            }

            if (length >= 0x8000)
            {
                streamReader.Fail();
                return string.Empty;
            }

            return streamReader.ReadString(length);
        }

        private static void WriteString(StreamWriter streamWriter, string text)
        {
            text = text ?? string.Empty;
            streamWriter.WriteUInt16((ushort)text.Length);
            streamWriter.WriteString(text);
        }

        #endregion
    }
}
