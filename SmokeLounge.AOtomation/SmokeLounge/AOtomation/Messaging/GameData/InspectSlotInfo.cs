// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InspectSlotInfo.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the InspectSlotInfo type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using AOSharp.Common.GameData;

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// An equipped item as InspectIIR_c sends it: its slot, two int16s, its identity, then an ACGItem_t (low id, high
    /// id, quality and an int).
    /// </summary>
    public class InspectSlotInfo
    {
        #region AoMember Properties

        [AoMember(0)]
        public EquipSlot EquipSlot { get; set; }

        [AoMember(1)]
        public short Unknown1 { get; set; }

        [AoMember(2)]
        public short Unknown2 { get; set; }

        [AoMember(3)]
        public Identity UniqueIdentity { get; set; }

        [AoMember(4)]
        public int LowId { get; set; }

        [AoMember(5)]
        public int HighId { get; set; }

        [AoMember(6)]
        public int Ql { get; set; }

        [AoMember(7)]
        public int Unknown3 { get; set; }

        #endregion
    }
}
