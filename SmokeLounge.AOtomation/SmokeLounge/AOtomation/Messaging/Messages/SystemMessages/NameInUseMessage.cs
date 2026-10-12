// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NameInUseMessage.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the NameInUseMessage type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Messages.SystemMessages
{
    using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

    /// <summary>
    /// The character could not be created: one int, the reason the client hands to its create window.
    /// </summary>
    [AoContract((int)SystemMessageType.NameInUse)]
    public class NameInUseMessage : SystemMessage
    {
        #region Constructors and Destructors

        public NameInUseMessage()
        {
            this.SystemMessageType = SystemMessageType.NameInUse;
        }

        #endregion

        #region AoMember Properties

        [AoMember(0)]
        public int Reason { get; set; }

        #endregion
    }
}
