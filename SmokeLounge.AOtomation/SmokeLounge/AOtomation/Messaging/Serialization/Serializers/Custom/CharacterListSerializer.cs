// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CharacterListSerializer.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the CharacterListSerializer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Serialization.Serializers.Custom
{
    using System;
    using System.Linq.Expressions;
    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.GameData;
    using SmokeLounge.AOtomation.Messaging.Messages;
    using SmokeLounge.AOtomation.Messaging.Messages.SystemMessages;
    using System.Collections.Generic;

    public class CharacterListSerializer : ISerializer
    {
        #region Public Properties

        public Type Type
        {
            get
            {
                return typeof(CharacterListMessage);
            }
        }

        #endregion

        #region Public Methods and Operators

        public object Deserialize(
            StreamReader streamReader,
            SerializationContext serializationContext,
            PropertyMetaData propertyMetaData = null)
        {
            var message = new CharacterListMessage();
            message.SystemMessageType = (SystemMessageType)streamReader.ReadInt32();
            var count = streamReader.ReadUInt32();
            var characters = new List<LoginCharacterInfo>();
            for (uint i = 0; i < count && !streamReader.Failed; i++)
            {
                var character = new LoginCharacterInfo();
                if (!LoginStream.ReadCharacter(streamReader, character))
                {
                    return message;
                }

                character.Status = (CharacterStatus)streamReader.ReadUInt32();
                characters.Add(character);
            }

            message.Characters = characters.ToArray();
            message.AllowedCharacters = streamReader.ReadInt32();
            message.Expansions = streamReader.ReadInt32();

            // Servers that leave this off: off, the read not failed (the client zeroes it and reads on).
            if (streamReader.PeekUntilEnd() >= 4)
            {
                message.ShadowlandsProfessionsEnabled = streamReader.ReadInt32() != 0;
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
                        <CharacterListSerializer, Func<StreamReader, SerializationContext, PropertyMetaData, object>>
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
                        <CharacterListSerializer, Action<StreamWriter, SerializationContext, object, PropertyMetaData>>
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
            var message = (CharacterListMessage)value;
            streamWriter.WriteInt32((int)message.SystemMessageType);
            var characters = message.Characters ?? new LoginCharacterInfo[0];
            streamWriter.WriteInt32(characters.Length);
            foreach (var character in characters)
            {
                LoginStream.WriteCharacter(streamWriter, character);
                streamWriter.WriteUInt32((uint)character.Status);
            }

            streamWriter.WriteInt32(message.AllowedCharacters);
            streamWriter.WriteInt32(message.Expansions);
            streamWriter.WriteInt32(message.ShadowlandsProfessionsEnabled ? 1 : 0);
        }

        #endregion
    }
}
