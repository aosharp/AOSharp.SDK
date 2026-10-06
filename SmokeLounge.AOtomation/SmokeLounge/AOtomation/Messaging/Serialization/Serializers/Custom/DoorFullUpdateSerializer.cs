using System;
using System.Linq.Expressions;
using AOSharp.Common.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;

namespace SmokeLounge.AOtomation.Messaging.Serialization.Serializers.Custom
{
    // Shared fields via DynelFullUpdateCodec, then the door fields read at Gamecode.dll 0x100a0a61 and 0x1009fd5b.
    class DoorFullUpdateSerializer : ISerializer
    {
        public Type Type => typeof(DoorFullUpdateMessage);

        public object Deserialize(StreamReader streamReader, SerializationContext serializationContext, PropertyMetaData propertyMetaData = null)
        {
            DoorFullUpdateMessage message = new DoorFullUpdateMessage();
            DynelFullUpdateCodec.Read(streamReader, message);

            message.Version2 = streamReader.ReadInt32();
            message.Unknown3 = streamReader.ReadInt32();

            message.Identities = new Identity[DynelFullUpdateCodec.ReadCount(streamReader)];
            for (int i = 0; i < message.Identities.Length; i++)
                message.Identities[i] = DynelFullUpdateCodec.ReadIdentity(streamReader);

            message.Version3 = streamReader.ReadInt32();
            message.DoorValue = streamReader.ReadInt32();

            return message;
        }

        public void Serialize(StreamWriter streamWriter, SerializationContext serializationContext, object value, PropertyMetaData propertyMetaData = null)
        {
            DoorFullUpdateMessage message = (DoorFullUpdateMessage)value;
            DynelFullUpdateCodec.Write(streamWriter, message);

            streamWriter.WriteInt32(message.Version2);
            streamWriter.WriteInt32(message.Unknown3);

            Identity[] identities = message.Identities ?? new Identity[0];
            DynelFullUpdateCodec.WriteCount(streamWriter, identities.Length);
            foreach (Identity identity in identities)
                DynelFullUpdateCodec.WriteIdentity(streamWriter, identity);

            streamWriter.WriteInt32(message.Version3);
            streamWriter.WriteInt32(message.DoorValue);
        }

        public Expression DeserializerExpression(ParameterExpression streamReaderExpression,
            ParameterExpression serializationContextExpression, Expression assignmentTargetExpression,
            PropertyMetaData propertyMetaData)
        {
            var deserializerMethodInfo =
                ReflectionHelper
                    .GetMethodInfo
                        <DoorFullUpdateSerializer, Func<StreamReader, SerializationContext, PropertyMetaData, object>>
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

        public Expression SerializerExpression(ParameterExpression streamWriterExpression,
            ParameterExpression serializationContextExpression, Expression valueExpression, PropertyMetaData propertyMetaData)
        {
            var serializerMethodInfo =
                ReflectionHelper
                    .GetMethodInfo
                    <DoorFullUpdateSerializer,
                        Action<StreamWriter, SerializationContext, object, PropertyMetaData>>(o => o.Serialize);
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
    }
}
