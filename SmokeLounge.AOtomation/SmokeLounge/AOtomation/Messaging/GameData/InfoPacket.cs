// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InfoPacket.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the InfoPacket type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using System.Collections.Generic;
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// The client's InfoPacket_t as InfoPacketIIR_t's reader fills it: a byte of flags saying which blocks follow,
    /// then a character's info. Where a field sets a stat the client's handler names it after the stat.
    /// </summary>
    public class InfoPacket
    {
        #region Constants

        /// <summary>The value a stat field holds when the packet does not say.</summary>
        public const int NoValue = 1234567890;

        /// <summary>Flag: the clan level, then (with <see cref="DestinationsFlag"/>) the destinations, then the clan's city.</summary>
        public const byte ClanFlag = 0x01;

        public const byte DestinationsFlag = 0x02;

        public const byte ItemsFlag = 0x04;

        public const byte GasFlag = 0x08;

        /// <summary>Flag: the PvP stats are left out.</summary>
        public const byte NoPvpFlag = 0x10;

        public const byte FactionsFlag = 0x20;

        public const byte Unknown1Flag = 0x40;

        #endregion

        #region Constructors and Destructors

        public InfoPacket()
        {
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.TitleName = string.Empty;
            this.Info = string.Empty;
            this.ClanLevel = string.Empty;
            this.Destinations = new List<GridDestination>();
            this.Items = new List<AcgItem>();
            this.Factions = new int[12];
            this.PvpStats = new int[8];
            this.InvadersKilled = NoValue;
            this.KilledByInvaders = NoValue;
            this.AlienLevel = NoValue;
        }

        #endregion

        #region AoMember Properties

        /// <summary>Which blocks follow (the *Flag constants).</summary>
        [AoMember(0)]
        public byte Flags { get; set; }

        /// <summary>Read only with <see cref="Unknown1Flag"/>, else 0; when 0, <see cref="Unknown2"/> follows TitleLevel.</summary>
        [AoMember(1)]
        public byte Unknown1 { get; set; }

        [AoMember(2)]
        public byte Profession { get; set; }

        [AoMember(3)]
        public byte ProfessionLevel { get; set; }

        [AoMember(4)]
        public byte TitleLevel { get; set; }

        /// <summary>Sent after TitleLevel only when <see cref="Unknown1"/> is 0; not stored by the client.</summary>
        [AoMember(5)]
        public byte Unknown2 { get; set; }

        [AoMember(6)]
        public byte VisualProfession { get; set; }

        /// <summary>Read by the client as an unsigned short (its side XP bonus).</summary>
        [AoMember(7)]
        public ushort SideXpBonus { get; set; }

        [AoMember(8)]
        public int Health { get; set; }

        [AoMember(9)]
        public int MaxHealth { get; set; }

        /// <summary>An NPC's Taunt.</summary>
        [AoMember(10)]
        public int Taunt { get; set; }

        /// <summary>An NPC's Clan.</summary>
        [AoMember(11)]
        public int Clan { get; set; }

        [AoMember(12)]
        public string FirstName { get; set; }

        [AoMember(13)]
        public string LastName { get; set; }

        [AoMember(14)]
        public string TitleName { get; set; }

        /// <summary>The info text.</summary>
        [AoMember(15)]
        public string Info { get; set; }

        /// <summary>With <see cref="ClanFlag"/>.</summary>
        [AoMember(16)]
        public string ClanLevel { get; set; }

        /// <summary>With <see cref="ClanFlag"/> and <see cref="DestinationsFlag"/>: a GridDestinationList_t.</summary>
        [AoMember(17)]
        public List<GridDestination> Destinations { get; set; }

        /// <summary>With <see cref="ClanFlag"/>: the clan member's city.</summary>
        [AoMember(18)]
        public int ClanCity { get; set; }

        /// <summary>With <see cref="ItemsFlag"/>: the ACG items.</summary>
        [AoMember(19)]
        public List<AcgItem> Items { get; set; }

        /// <summary>With <see cref="GasFlag"/>.</summary>
        [AoMember(20)]
        public int GasSeconds { get; set; }

        /// <summary>With <see cref="GasFlag"/>.</summary>
        [AoMember(21)]
        public byte Gas { get; set; }

        /// <summary>
        /// With <see cref="FactionsFlag"/>: ClanSentinels, OTMed, ClanGaia, OTTrans, ClanVanguards, GOS, OTFollowers,
        /// OTOperator, OTUnredeemed, ClanDevoted, ClanConserver, ClanRedeemed.
        /// </summary>
        [AoMember(22)]
        public int[] Factions { get; set; }

        [AoMember(23)]
        public int InvadersKilled { get; set; }

        [AoMember(24)]
        public int KilledByInvaders { get; set; }

        [AoMember(25)]
        public int AlienLevel { get; set; }

        /// <summary>
        /// Without <see cref="NoPvpFlag"/>: PVPDuelKills, PVPDuelDeaths, PVPProfessionDuelKills, PVPRankedSoloKills,
        /// PVPRankedTeamKills, PVPSoloScore, PVPTeamScore, PVPDuelScore.
        /// </summary>
        [AoMember(26)]
        public int[] PvpStats { get; set; }

        #endregion
    }
}
