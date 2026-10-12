// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpellStream.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the SpellStream type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Serialization.Serializers.Custom
{
    using System.Collections.Generic;

    using AOSharp.Common.GameData;

    using SmokeLounge.AOtomation.Messaging.GameData;

    /// <summary>The client's SpellData_t, read and written as it does. Where it rejects the data, the read fails.</summary>
    internal static class SpellStream
    {
        #region Public Methods and Operators

        public static SpellData ReadSpell(StreamReader r)
        {
            var spell = new SpellData
                            {
                                Function = (SpellFunction)r.ReadInt32(),
                                Unknown1 = r.ReadInt32(),
                                Version = r.ReadInt32()
                            };
            if (r.Failed)
            {
                return spell;
            }

            var criteriaCount = r.ReadInt32();
            if (criteriaCount > 1000)
            {
                r.Fail();
                return spell;
            }

            spell.Criteria = ReadCriteria(r, criteriaCount);
            if (r.Failed)
            {
                return spell;
            }

            var arguments = SpellFormats.GetArguments(spell.Function);
            if (arguments == null || spell.Version != SpellFormats.Version)
            {
                r.Fail();
                return spell;
            }

            ReadArguments(r, SpellFormats.Common, spell);
            ReadArguments(r, arguments, spell);
            if (spell.Function == SpellFunction.ApplyTexture)
            {
                ReadTexture(r, spell);
            }

            if (r.Failed)
            {
                return spell;
            }

            if (spell.Function == SpellFunction.Criteria)
            {
                var raw = r.ReadInt32();
                if (raw % 0x3f1 != 0 || raw < 1)
                {
                    r.Fail();
                    return spell;
                }

                spell.SpecialCriteria = ReadCriteria(r, raw / 0x3f1 - 1);
            }
            else if (spell.Function == SpellFunction.Expression)
            {
                spell.Expression = ReadExpression(r);
            }

            // A hit that neither takes nor gives is not valid.
            if (!r.Failed && spell.Function == SpellFunction.Hit && spell.Get(SpellPropertyOperator.Min) == 0
                && spell.Get(SpellPropertyOperator.Max) == 0)
            {
                r.Fail();
            }

            return spell;
        }

        public static void WriteSpell(StreamWriter w, SpellData spell)
        {
            w.WriteInt32((int)spell.Function);
            w.WriteInt32(spell.Unknown1);
            w.WriteInt32(spell.Version);
            WriteCriteria(w, spell.Criteria);
            WriteArguments(w, SpellFormats.Common, spell);
            WriteArguments(w, SpellFormats.GetArguments(spell.Function) ?? new SpellArgument[0], spell);
            if (spell.Function == SpellFunction.ApplyTexture)
            {
                WriteTexture(w, spell);
            }

            if (spell.Function == SpellFunction.Criteria)
            {
                var special = spell.SpecialCriteria ?? new SpellCriterion[0];
                w.WriteInt32((special.Length + 1) * 0x3f1);
                foreach (var criterion in special)
                {
                    WriteCriterion(w, criterion);
                }
            }
            else if (spell.Function == SpellFunction.Expression)
            {
                WriteExpression(w, spell.Expression ?? new SpellExpression { Kind = SpellExpression.ValueKind });
            }
        }

        #endregion

        #region Methods

        private static SpellCriterion[] ReadCriteria(StreamReader r, int count)
        {
            var criteria = new List<SpellCriterion>();
            for (var i = 0; i < count && !r.Failed; i++)
            {
                var criterion = new SpellCriterion { Value1 = r.ReadInt32(), Value2 = r.ReadInt32(), Operator = r.ReadInt32() };
                if ((uint)criterion.Operator >= 146)
                {
                    r.Fail();
                }

                criteria.Add(criterion);
            }

            return criteria.ToArray();
        }

        private static void WriteCriteria(StreamWriter w, SpellCriterion[] criteria)
        {
            criteria = criteria ?? new SpellCriterion[0];
            w.WriteInt32(criteria.Length);
            foreach (var criterion in criteria)
            {
                WriteCriterion(w, criterion);
            }
        }

        private static void WriteCriterion(StreamWriter w, SpellCriterion criterion)
        {
            w.WriteInt32(criterion.Value1);
            w.WriteInt32(criterion.Value2);
            w.WriteInt32(criterion.Operator);
        }

        private static void ReadArguments(StreamReader r, SpellArgument[] arguments, SpellData spell)
        {
            foreach (var argument in arguments)
            {
                if (r.Failed)
                {
                    return;
                }

                if (argument.IsString)
                {
                    var length = r.ReadInt32();
                    spell.Strings[argument.StringIndex] = length > 0 ? r.ReadString(length) : string.Empty;
                }
                else
                {
                    spell.Properties[argument.Property] = r.ReadInt32();
                }
            }
        }

        private static void WriteArguments(StreamWriter w, SpellArgument[] arguments, SpellData spell)
        {
            foreach (var argument in arguments)
            {
                if (argument.IsString)
                {
                    var text = (spell.Strings ?? new string[SpellFormats.StringCount])[argument.StringIndex] ?? string.Empty;
                    w.WriteInt32(text.Length);
                    w.WriteString(text);
                }
                else
                {
                    w.WriteInt32(spell.Get(argument.Property));
                }
            }
        }

        /// <summary>The texture, the marker, then by the marker either SeeSkin, or BodyPart, SeeSkin, Arg1 and Arg2.</summary>
        private static void ReadTexture(StreamReader r, SpellData spell)
        {
            var properties = spell.Properties;
            properties[SpellPropertyOperator.Texture] = r.ReadInt32();
            spell.TextureMarker = r.ReadInt32();
            if (spell.TextureMarker < SpellFormats.TextureMarker)
            {
                properties[SpellPropertyOperator.BodyPart] = spell.TextureMarker;
                properties[SpellPropertyOperator.SeeSkin] = r.ReadInt32();
                properties[SpellPropertyOperator.Arg1] = 0;
                properties[SpellPropertyOperator.Arg2] = 0;
                return;
            }

            properties[SpellPropertyOperator.BodyPart] = r.ReadInt32();
            properties[SpellPropertyOperator.SeeSkin] = r.ReadInt32();
            properties[SpellPropertyOperator.Arg1] = r.ReadInt32();
            properties[SpellPropertyOperator.Arg2] = r.ReadInt32();
        }

        private static void WriteTexture(StreamWriter w, SpellData spell)
        {
            w.WriteInt32(spell.Get(SpellPropertyOperator.Texture));
            if (spell.TextureMarker < SpellFormats.TextureMarker)
            {
                w.WriteInt32(spell.Get(SpellPropertyOperator.BodyPart));
                w.WriteInt32(spell.Get(SpellPropertyOperator.SeeSkin));
                return;
            }

            w.WriteInt32(spell.TextureMarker);
            w.WriteInt32(spell.Get(SpellPropertyOperator.BodyPart));
            w.WriteInt32(spell.Get(SpellPropertyOperator.SeeSkin));
            w.WriteInt32(spell.Get(SpellPropertyOperator.Arg1));
            w.WriteInt32(spell.Get(SpellPropertyOperator.Arg2));
        }

        private static SpellExpression ReadExpression(StreamReader r)
        {
            var node = new SpellExpression { Kind = r.ReadInt32() };
            switch (node.Kind)
            {
                case SpellExpression.ValueKind:
                    node.Value = r.ReadSingle();
                    break;
                case SpellExpression.StatKind:
                    node.Stat = r.ReadInt32();
                    node.Flag = r.ReadInt32();
                    break;
                case SpellExpression.CompoundKind:
                case SpellExpression.FunctionKind:
                    node.Operator = r.ReadInt32();
                    node.Left = ReadExpression(r);
                    node.Right = r.Failed ? null : ReadExpression(r);
                    break;
                default:
                    r.Fail();
                    break;
            }

            return node;
        }

        private static void WriteExpression(StreamWriter w, SpellExpression node)
        {
            w.WriteInt32(node.Kind);
            switch (node.Kind)
            {
                case SpellExpression.ValueKind:
                    w.WriteSingle(node.Value);
                    break;
                case SpellExpression.StatKind:
                    w.WriteInt32(node.Stat);
                    w.WriteInt32(node.Flag);
                    break;
                default:
                    w.WriteInt32(node.Operator);
                    WriteExpression(w, node.Left ?? new SpellExpression { Kind = SpellExpression.ValueKind });
                    WriteExpression(w, node.Right ?? new SpellExpression { Kind = SpellExpression.ValueKind });
                    break;
            }
        }

        #endregion
    }
}
