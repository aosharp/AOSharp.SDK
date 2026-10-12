// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ActionSerializer.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the ActionSerializer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Serialization.Serializers.Custom
{
    using System;
    using System.Linq.Expressions;
    using AOSharp.Common.GameData;
    using SmokeLounge.AOtomation.Messaging.Messages;
    using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;

    public class ActionSerializer : ISerializer
    {
        #region Public Properties

        public Type Type
        {
            get
            {
                return typeof(ActionMessage);
            }
        }

        #endregion

        #region Public Methods and Operators

        public object Deserialize(
            StreamReader streamReader,
            SerializationContext serializationContext,
            PropertyMetaData propertyMetaData = null)
        {
            // ActionIIR_t's reader (1009f042): a version; only 1 is followed by the code and the actor. Any other
            // version reads as done, leaving them 0, and the client runs the action so.
            var m = new ActionMessage();
            var r = streamReader;
            m.N3MessageType = (N3MessageType)r.ReadInt32();
            m.Identity = new Identity((IdentityType)r.ReadInt32(), r.ReadInt32());
            m.Unknown = r.ReadByte();
            m.Version = r.ReadInt32();
            if (m.Version == ActionMessage.CurrentVersion)
            {
                m.Code = r.ReadInt32();
                m.Actor = new Identity((IdentityType)r.ReadInt32(), r.ReadInt32());
            }

            return m;
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
                        <ActionSerializer, Func<StreamReader, SerializationContext, PropertyMetaData, object>>
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
                        <ActionSerializer, Action<StreamWriter, SerializationContext, object, PropertyMetaData>>
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
            var m = (ActionMessage)value;
            streamWriter.WriteInt32((int)m.N3MessageType);
            streamWriter.WriteInt32((int)m.Identity.Type);
            streamWriter.WriteInt32(m.Identity.Instance);
            streamWriter.WriteByte(m.Unknown);
            streamWriter.WriteInt32(m.Version);
            if (m.Version == ActionMessage.CurrentVersion)
            {
                streamWriter.WriteInt32(m.Code);
                streamWriter.WriteInt32((int)m.Actor.Type);
                streamWriter.WriteInt32(m.Actor.Instance);
            }
        }

        #endregion
    }
}
