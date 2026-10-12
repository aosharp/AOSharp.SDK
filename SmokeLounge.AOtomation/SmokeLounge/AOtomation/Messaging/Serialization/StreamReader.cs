// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StreamReader.cs" company="SmokeLounge">
//   Copyright © 2013 SmokeLounge.
//   This program is free software. It comes without any warranty, to
//   the extent permitted by applicable law. You can redistribute it
//   and/or modify it under the terms of the Do What The Fuck You Want
//   To Public License, Version 2, as published by Sam Hocevar. See
//   http://www.wtfpl.net/ for more details.
// </copyright>
// <summary>
//   Defines the StreamReader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SmokeLounge.AOtomation.Messaging.Serialization
{
    using System;
    using System.IO;
    using System.Net;
    using System.Text;

    /// <summary>
    /// Reads a message as the AO client's <c>BinaryStream</c> does: big-endian numbers; a read past the end
    /// gives 0 and sets <see cref="Failed"/> instead of throwing (the client zeroes the value and only marks
    /// the stream's error state, and servers that leave off a message's last field rely on it); text is one
    /// character per byte (Latin-1), a fixed-length string cut at its first NUL.
    /// </summary>
    public sealed class StreamReader : IDisposable
    {
        #region Fields

        private readonly BinaryReader reader;

        private readonly Stream stream;

        #endregion

        #region Constructors and Destructors

        public StreamReader(Stream stream)
        {
            this.stream = stream;
            this.reader = new BinaryReader(stream);
        }

        #endregion

        #region Public Properties

        /// <summary>A read ran past the end of the message (what it read was 0, or empty).</summary>
        public bool Failed { get; private set; }

        public long Position
        {
            get
            {
                return this.stream.Position;
            }

            set
            {
                this.stream.Position = value;
            }
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>Marks the read failed, as the client's reader does with what it rejects (the message is dropped).</summary>
        public void Fail()
        {
            this.Failed = true;
        }

        public void Dispose()
        {
            this.reader.Dispose();
            this.stream.Dispose();
        }

        public bool ReadBool()
        {
            return this.Has(1) && this.reader.ReadBoolean();
        }

        public byte ReadByte()
        {
            return this.Has(1) ? this.reader.ReadByte() : (byte)0;
        }

        /// <summary>The next bytes; asked for more than are left, the ones left (and the read fails).</summary>
        public byte[] ReadBytes(int count)
        {
            if (count <= 0)
            {
                if (count < 0)
                {
                    this.Failed = true;
                }

                return new byte[0];
            }

            var left = this.stream.Length - this.stream.Position;
            if (count > left)
            {
                // Read what is there, never allocating what a bad length asks for.
                this.Failed = true;
                count = (int)Math.Max(0, left);
            }

            return this.reader.ReadBytes(count);
        }

        public short ReadInt16()
        {
            return this.Has(2) ? IPAddress.NetworkToHostOrder(this.reader.ReadInt16()) : (short)0;
        }

        public int ReadInt32()
        {
            return this.Has(4) ? IPAddress.NetworkToHostOrder(this.reader.ReadInt32()) : 0;
        }

        public long ReadInt64()
        {
            return this.Has(8) ? IPAddress.NetworkToHostOrder(this.reader.ReadInt64()) : 0;
        }

        public float ReadSingle()
        {
            if (!this.Has(4))
            {
                return 0;
            }

            var single = this.reader.ReadBytes(4);
            Array.Reverse(single);
            return BitConverter.ToSingle(single, 0);
        }

        public double ReadDouble()
        {
            return this.Has(8) ? this.reader.ReadDouble() : 0;
        }

        /// <summary>A fixed-length string: its bytes, one character each, cut at the first NUL.</summary>
        public string ReadString(int length)
        {
            var bytes = this.ReadBytes(length);
            var end = Array.IndexOf(bytes, (byte)0);
            return Latin1(bytes, end < 0 ? bytes.Length : end);
        }

        public ushort ReadUInt16()
        {
            return unchecked((ushort)this.ReadInt16());
        }

        public uint ReadUInt32()
        {
            return unchecked((uint)this.ReadInt32());
        }

        public string ReadNullTerminatedString()
        {
            var bytes = new System.Collections.Generic.List<byte>();
            while (this.Has(1))
            {
                var ch = this.reader.ReadByte();
                if (ch == 0)
                {
                    break;
                }

                bytes.Add(ch);
            }

            return Latin1(bytes.ToArray(), bytes.Count);
        }

        public int PeekNullTermStringLength()
        {
            long origin = this.stream.Position;
            long length = 0;
            while (this.stream.Position < this.stream.Length)
            {
                length++;
                if (this.reader.ReadByte() == 0)
                {
                    break;
                }
            }

            this.stream.Position = origin;
            return (int)length;
        }

        public int PeekUntilEnd()
        {
            return (int)Math.Max(0, this.stream.Length - this.stream.Position);
        }

        public byte[] ReadAll()
        {
            this.stream.Position = 0;
            return this.reader.ReadBytes((int)this.stream.Length);
        }

        #endregion

        #region Methods

        private static string Latin1(byte[] bytes, int length)
        {
            var chars = new char[length];
            for (var i = 0; i < length; i++)
            {
                chars[i] = (char)bytes[i];
            }

            return new string(chars);
        }

        /// <summary>Whether <paramref name="count"/> more bytes are there; when not, the read fails and the stream ends.</summary>
        private bool Has(int count)
        {
            if (this.stream.Length - this.stream.Position >= count)
            {
                return true;
            }

            this.Failed = true;
            this.stream.Position = this.stream.Length;
            return false;
        }

        #endregion
    }
}
