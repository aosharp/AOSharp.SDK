// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PlayfieldStream.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the PlayfieldStream type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Serialization.Serializers.Custom
{
    using System.Collections.Generic;

    using AOSharp.Common.GameData;

    using SmokeLounge.AOtomation.Messaging.GameData;

    /// <summary>The objects a PlayfieldAnarchyF carries inline, read and written as the client does.</summary>
    internal static class PlayfieldStream
    {
        #region Public Methods and Operators

        /// <summary>RDBPlayfield_t's ReadBlob body, after the identity and its int.</summary>
        public static PlayfieldRecord ReadPlayfield(StreamReader r, int recordId)
        {
            var p = new PlayfieldRecord
                        {
                            Version = r.ReadInt32(),
                            Id = r.ReadInt32(),
                            Name = r.ReadString(32),
                            TilemapId = r.ReadInt32(),
                            ZoneSize = r.ReadInt32()
                        };
            var zoneCount = r.ReadInt32();
            if (p.Version > 8)
            {
                p.Flags = r.ReadInt32();
                p.UnknownV9B = r.ReadInt32();
                p.PlayfieldTypeBits = r.ReadInt32();
                p.UnknownV9C = r.ReadInt32();
                p.UnknownV9D = r.ReadInt32();
                p.Reserved = r.ReadBytes(24);
            }

            for (var i = 0; i < zoneCount && !r.Failed; i++)
            {
                if (p.IsIndoor)
                {
                    p.Rooms.Add(ReadRoom(r, p.Version));
                }
                else
                {
                    p.Zones.Add(p.Version > 4 ? ReadAttractors(r, p.Version, r.ReadInt32()) : new List<CameraAttractor>());
                }
            }

            var water = r.ReadInt32();
            for (var i = 0; i < water && !r.Failed; i++)
            {
                p.Water.Add(ReadWater(r));
            }

            p.Unknown3 = r.ReadInt32();
            p.Unknown4 = r.ReadInt32();
            p.EnvironmentVersion = r.ReadInt32();
            if (p.EnvironmentVersion > 0)
            {
                p.Environment = r.ReadBytes(EnvironmentLength(p.EnvironmentVersion));
            }

            var destinations = r.ReadInt32();
            for (var i = 0; i < destinations && !r.Failed; i++)
            {
                p.Destinations.Add(
                    new PlayfieldDestination { Id = r.ReadInt32(), Start = ReadVector3(r), End = ReadVector3(r) });
            }

            // Format versions below 7, and an id other than the record's, are refused.
            if (p.Version < PlayfieldRecord.MinFormatVersion || p.Id != recordId)
            {
                r.Fail();
            }

            return p;
        }

        public static void WritePlayfield(StreamWriter w, PlayfieldRecord p)
        {
            w.WriteInt32(p.Version);
            w.WriteInt32(p.Id);
            w.WriteString(p.Name ?? string.Empty, 32);
            w.WriteInt32(p.TilemapId);
            w.WriteInt32(p.ZoneSize);
            w.WriteInt32(p.IsIndoor ? p.Rooms.Count : p.Zones.Count);
            if (p.Version > 8)
            {
                w.WriteInt32(p.Flags);
                w.WriteInt32(p.UnknownV9B);
                w.WriteInt32(p.PlayfieldTypeBits);
                w.WriteInt32(p.UnknownV9C);
                w.WriteInt32(p.UnknownV9D);
                WriteFixed(w, p.Reserved, 24);
            }

            if (p.IsIndoor)
            {
                foreach (var room in p.Rooms)
                {
                    WriteRoom(w, room, p.Version);
                }
            }
            else
            {
                foreach (var zone in p.Zones)
                {
                    if (p.Version > 4)
                    {
                        w.WriteInt32(zone.Count);
                        WriteAttractors(w, zone, p.Version);
                    }
                }
            }

            w.WriteInt32(p.Water.Count);
            foreach (var water in p.Water)
            {
                WriteWater(w, water);
            }

            w.WriteInt32(p.Unknown3);
            w.WriteInt32(p.Unknown4);
            w.WriteInt32(p.EnvironmentVersion);
            if (p.EnvironmentVersion > 0)
            {
                WriteFixed(w, p.Environment, EnvironmentLength(p.EnvironmentVersion));
            }

            w.WriteInt32(p.Destinations.Count);
            foreach (var destination in p.Destinations)
            {
                w.WriteInt32(destination.Id);
                WriteVector3(w, destination.Start);
                WriteVector3(w, destination.End);
            }
        }

        /// <summary>ACGBuildingGeneratorData_t's ReadBlob body, after the identity and its int.</summary>
        public static AcgBuildingGenerator ReadGenerator(StreamReader r)
        {
            var g = new AcgBuildingGenerator { Version = r.ReadInt16() };
            if (g.Version != AcgBuildingGenerator.CurrentVersion)
            {
                r.Fail();
                return g;
            }

            g.Width = r.ReadUInt16();
            g.Height = r.ReadUInt16();
            g.FloorHeight = r.ReadUInt16();
            g.TemplatePlayfield = r.ReadInt32();
            g.AmbientRed = r.ReadByte();
            g.AmbientGreen = r.ReadByte();
            g.AmbientBlue = r.ReadByte();
            var count = r.ReadInt32();
            if (g.Width == 0 || g.Height == 0 || g.FloorHeight > 1000 || g.TemplatePlayfield == 0
                || (uint)(count - 1) > 9999)
            {
                r.Fail();
                return g;
            }

            for (var i = 0; i < count && !r.Failed; i++)
            {
                var room = new AcgRoom
                               {
                                   RoomIndex = r.ReadUInt16(),
                                   Floor = (sbyte)r.ReadByte(),
                                   GridX = r.ReadByte(),
                                   GridZ = r.ReadByte(),
                                   Rotation = r.ReadByte()
                               };
                if (!room.IsValid)
                {
                    r.Fail();
                    return g;
                }

                g.Rooms.Add(room);
            }

            return g;
        }

        public static void WriteGenerator(StreamWriter w, AcgBuildingGenerator g)
        {
            w.WriteInt16(g.Version);
            w.WriteUInt16(g.Width);
            w.WriteUInt16(g.Height);
            w.WriteUInt16(g.FloorHeight);
            w.WriteInt32(g.TemplatePlayfield);
            w.WriteByte(g.AmbientRed);
            w.WriteByte(g.AmbientGreen);
            w.WriteByte(g.AmbientBlue);
            w.WriteInt32(g.Rooms.Count);
            foreach (var room in g.Rooms)
            {
                w.WriteUInt16(room.RoomIndex);
                w.WriteByte((byte)room.Floor);
                w.WriteByte(room.GridX);
                w.WriteByte(room.GridZ);
                w.WriteByte(room.Rotation);
            }
        }

        /// <summary>A proxy instance after its identity and first int: 0xC77B's block, then the dynels.</summary>
        public static ProxyInstance ReadProxyInstance(StreamReader r, bool hasData)
        {
            var p = new ProxyInstance { Unknown1 = r.ReadInt32() };
            if (hasData)
            {
                p.Data = new ProxyInstanceData
                             {
                                 Unknown1 = r.ReadInt32(),
                                 Unknown2 = ReadIdentity(r),
                                 Unknown3 = r.ReadInt32(),
                                 Unknown4 = ReadVector3(r),
                                 Unknown5 = r.ReadInt32(),
                                 Unknown6 = r.ReadInt32(),
                                 Unknown7 = r.ReadSingle(),
                                 Unknown8 = r.ReadInt32(),
                                 Unknown9 = r.ReadInt32()
                             };
            }

            var count = r.ReadInt32();
            for (var i = 0; i < count && !r.Failed; i++)
            {
                p.Dynels.Add(
                    new ProxyDynel
                        {
                            IdentityType = (IdentityType)r.ReadInt32(),
                            Unknown1 = r.ReadInt32(),
                            Unknown2 = r.ReadInt32(),
                            Unknown3 = r.ReadInt32(),
                            Instance = r.ReadInt32()
                        });
            }

            return p;
        }

        public static void WriteProxyInstance(StreamWriter w, ProxyInstance p, bool hasData)
        {
            w.WriteInt32(p.Unknown1);
            if (hasData)
            {
                var d = p.Data ?? new ProxyInstanceData();
                w.WriteInt32(d.Unknown1);
                WriteIdentity(w, d.Unknown2);
                w.WriteInt32(d.Unknown3);
                WriteVector3(w, d.Unknown4);
                w.WriteInt32(d.Unknown5);
                w.WriteInt32(d.Unknown6);
                w.WriteSingle(d.Unknown7);
                w.WriteInt32(d.Unknown8);
                w.WriteInt32(d.Unknown9);
            }

            w.WriteInt32(p.Dynels.Count);
            foreach (var dynel in p.Dynels)
            {
                w.WriteInt32((int)dynel.IdentityType);
                w.WriteInt32(dynel.Unknown1);
                w.WriteInt32(dynel.Unknown2);
                w.WriteInt32(dynel.Unknown3);
                w.WriteInt32(dynel.Instance);
            }
        }

        public static Identity ReadIdentity(StreamReader r)
        {
            return new Identity((IdentityType)r.ReadInt32(), r.ReadInt32());
        }

        public static void WriteIdentity(StreamWriter w, Identity identity)
        {
            w.WriteInt32((int)identity.Type);
            w.WriteInt32(identity.Instance);
        }

        public static Vector3 ReadVector3(StreamReader r)
        {
            return new Vector3(r.ReadSingle(), r.ReadSingle(), r.ReadSingle());
        }

        public static void WriteVector3(StreamWriter w, Vector3 vector)
        {
            w.WriteSingle(vector.X);
            w.WriteSingle(vector.Y);
            w.WriteSingle(vector.Z);
        }

        #endregion

        #region Methods

        private static int EnvironmentLength(int version)
        {
            return version == 26 ? 22 : version == 29 ? 25 : 26;
        }

        private static void WriteFixed(StreamWriter w, byte[] bytes, int length)
        {
            var fixedBytes = new byte[length];
            if (bytes != null)
            {
                System.Array.Copy(bytes, fixedBytes, System.Math.Min(length, bytes.Length));
            }

            w.WriteBytes(fixedBytes);
        }

        private static PlayfieldRoom ReadRoom(StreamReader r, int version)
        {
            var room = new PlayfieldRoom
                           {
                               Flags = r.ReadByte(),
                               Unused = r.ReadByte(),
                               TileX1 = r.ReadInt16(),
                               TileY1 = r.ReadInt16(),
                               TileX2 = r.ReadInt16(),
                               TileY2 = r.ReadInt16(),
                               TemplatePosition = ReadVector3(r)
                           };
            var doors = r.ReadInt16();
            for (var i = 0; i < doors && !r.Failed; i++)
            {
                room.DoorConnections.Add(new DoorConnection { ZoneLink = r.ReadInt16(), PosRot = r.ReadInt16() });
            }

            if ((room.Flags & 0x80) != 0)
            {
                room.Name = r.ReadString(32);
            }

            // The lightmap: its length with the flag after it, the flag, then the packed bytes.
            var lightmapLength = r.ReadInt32() - 4;
            room.LightmapPacked = r.ReadInt32();
            room.Lightmap = lightmapLength > 0 ? r.ReadBytes(lightmapLength) : new byte[0];

            var water = r.ReadInt32();
            if (water == 0 || water % 0x3f1 != 0)
            {
                r.Fail();
                return room;
            }

            for (var i = 0; i < water / 0x3f1 - 1 && !r.Failed; i++)
            {
                room.Water.Add(ReadWater(r));
            }

            if (version > 4)
            {
                room.CameraAttractors = ReadAttractors(r, version, r.ReadInt32());
            }

            return room;
        }

        private static void WriteRoom(StreamWriter w, PlayfieldRoom room, int version)
        {
            w.WriteByte(room.Flags);
            w.WriteByte(room.Unused);
            w.WriteInt16(room.TileX1);
            w.WriteInt16(room.TileY1);
            w.WriteInt16(room.TileX2);
            w.WriteInt16(room.TileY2);
            WriteVector3(w, room.TemplatePosition);
            w.WriteInt16((short)room.DoorConnections.Count);
            foreach (var door in room.DoorConnections)
            {
                w.WriteInt16(door.ZoneLink);
                w.WriteInt16(door.PosRot);
            }

            if ((room.Flags & 0x80) != 0)
            {
                w.WriteString(room.Name ?? string.Empty, 32);
            }

            var lightmap = room.Lightmap ?? new byte[0];
            w.WriteInt32(lightmap.Length + 4);
            w.WriteInt32(room.LightmapPacked);
            w.WriteBytes(lightmap);
            w.WriteInt32((room.Water.Count + 1) * 0x3f1);
            foreach (var water in room.Water)
            {
                WriteWater(w, water);
            }

            if (version > 4)
            {
                w.WriteInt32(room.CameraAttractors.Count);
                WriteAttractors(w, room.CameraAttractors, version);
            }
        }

        private static List<CameraAttractor> ReadAttractors(StreamReader r, int version, int count)
        {
            var attractors = new List<CameraAttractor>();
            for (var i = 0; i < count && !r.Failed; i++)
            {
                var attractor = new CameraAttractor
                                    {
                                        Position = ReadVector3(r),
                                        Orientation =
                                            new Quaternion(r.ReadSingle(), r.ReadSingle(), r.ReadSingle(), r.ReadSingle())
                                    };
                attractor.Target = version < 6 ? attractor.Position : ReadVector3(r);
                attractor.Range = r.ReadSingle();
                attractors.Add(attractor);
            }

            return attractors;
        }

        private static void WriteAttractors(StreamWriter w, List<CameraAttractor> attractors, int version)
        {
            foreach (var attractor in attractors)
            {
                WriteVector3(w, attractor.Position);
                w.WriteSingle(attractor.Orientation.X);
                w.WriteSingle(attractor.Orientation.Y);
                w.WriteSingle(attractor.Orientation.Z);
                w.WriteSingle(attractor.Orientation.W);
                if (version >= 6)
                {
                    WriteVector3(w, attractor.Target);
                }

                w.WriteSingle(attractor.Range);
            }
        }

        private static WaterMesh ReadWater(StreamReader r)
        {
            var water = new WaterMesh { Flags = r.ReadInt32() };
            var vertices = r.ReadInt32();
            for (var i = 0; i < vertices && !r.Failed; i++)
            {
                water.Vertices.Add(ReadVector3(r));
            }

            var triangles = r.ReadInt32();
            for (var i = 0; i < triangles * 3 && !r.Failed; i++)
            {
                water.Triangles.Add(r.ReadInt16());
            }

            return water;
        }

        private static void WriteWater(StreamWriter w, WaterMesh water)
        {
            w.WriteInt32(water.Flags);
            w.WriteInt32(water.Vertices.Count);
            foreach (var vertex in water.Vertices)
            {
                WriteVector3(w, vertex);
            }

            w.WriteInt32(water.Triangles.Count / 3);
            foreach (var index in water.Triangles)
            {
                w.WriteInt16(index);
            }
        }

        #endregion
    }
}
