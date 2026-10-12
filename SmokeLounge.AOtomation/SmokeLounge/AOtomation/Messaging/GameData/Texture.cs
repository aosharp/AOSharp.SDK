// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Texture.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the Texture type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.GameData
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// A piece of clothing as SimpleCharFullUpdate and CorpseFullUpdate send it: a packed int, then the texture and an
    /// int. A positive packed int carries the place in its low half and <see cref="Unknown1"/> in its high half; a
    /// positive high half adds <see cref="Unknown2"/> and <see cref="Unknown3"/>. Any other packed int is the place.
    /// </summary>
    public class Texture
    {
        #region AoMember Properties

        [AoMember(0)]
        public int Place { get; set; }

        [AoMember(1)]
        public int Id { get; set; }

        [AoMember(2)]
        public int Unknown { get; set; }

        /// <summary>
        /// The packed int's high half. This and the two below are not members: messages read by attribute take the
        /// three ints above; SimpleCharFullUpdate's and CorpseFullUpdate's serializers set them.
        /// </summary>
        public short Unknown1 { get; set; }

        /// <summary>Sent only when <see cref="Unknown1"/> is positive.</summary>
        public int Unknown2 { get; set; }

        public int Unknown3 { get; set; }

        #endregion
    }
}