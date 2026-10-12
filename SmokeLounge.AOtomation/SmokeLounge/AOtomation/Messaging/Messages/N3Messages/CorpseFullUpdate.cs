// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CorpseFullUpdate.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the CorpseFullUpdateMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.N3Messages
{
    using System;

    using AOSharp.Common.GameData;

    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// A corpse (CorpseFullUpdateIIR_t): the dynel, its stats and name, an identity list, the spells it carries (event
    /// 10), the dead character and its clothes. Read and written by CorpseFullUpdateSerializer.
    /// </summary>
    [AoContract((int)N3MessageType.CorpseFullUpdate)]
    public class CorpseFullUpdateMessage : N3Message
    {
        #region Fields

        private readonly DateTime receivedAt;

        #endregion

        #region Constructors and Destructors

        public CorpseFullUpdateMessage()
        {
            this.N3MessageType = N3MessageType.CorpseFullUpdate;
            this.receivedAt = DateTime.Now;
            this.Version = 8;
            this.Heading = Quaternion.Identity;
            this.Stats = new GameTuple<Stat, int>[0];
            this.Name = string.Empty;
            this.UnknownArray = new Identity[0];
            this.Spells = new SpellData[0];
            this.Textures = new Texture[0];
        }

        #endregion

        #region Public Properties

        public DateTime DecayTime
        {
            get
            {
                var timeExist = 18000;
                var deadTimer = 60;
                foreach (var tuple in this.Stats ?? new GameTuple<Stat, int>[0])
                {
                    if (tuple.Value1 == Stat.TimeExist)
                    {
                        timeExist = tuple.Value2;
                    }

                    if (tuple.Value1 == Stat.DeadTimer)
                    {
                        deadTimer = tuple.Value2;
                    }
                }

                return this.receivedAt.AddMinutes((double)timeExist / deadTimer / 100);
            }
        }

        #endregion

        #region AoMember Properties

        /// <summary>8; any other fails the read.</summary>
        [AoMember(0)]
        public int Version { get; set; }

        [AoMember(1)]
        public int Unknown2 { get; set; }

        [AoMember(2)]
        public Identity Owner { get; set; }

        /// <summary>Sent only when <see cref="Owner"/>'s type is 0.</summary>
        [AoMember(3)]
        public Vector3 Position { get; set; }

        /// <summary>Sent only when <see cref="Owner"/>'s type is 0.</summary>
        [AoMember(4)]
        public Quaternion Heading { get; set; }

        [AoMember(5)]
        public int PlayfieldId { get; set; }

        [AoMember(6)]
        public Identity StateMachine { get; set; }

        [AoMember(7)]
        public short Unknown3 { get; set; }

        [AoMember(8)]
        public GameTuple<Stat, int>[] Stats { get; set; }

        [AoMember(9)]
        public string Name { get; set; }

        [AoMember(10)]
        public int Unknown4 { get; set; }

        [AoMember(11)]
        public int Unknown5 { get; set; }

        /// <summary>A checked 0x3F1 list of at most 30000.</summary>
        [AoMember(12)]
        public Identity[] UnknownArray { get; set; }

        [AoMember(13)]
        public int Unknown6 { get; set; }

        /// <summary>A checked 0x3F1 list of at most 999.</summary>
        [AoMember(14)]
        public SpellData[] Spells { get; set; }

        /// <summary>The dead character.</summary>
        [AoMember(15)]
        public Identity UnknownIdentity { get; set; }

        /// <summary>The clothes, as SimpleCharFullUpdate sends them.</summary>
        [AoMember(16)]
        public Texture[] Textures { get; set; }

        #endregion
    }
}
