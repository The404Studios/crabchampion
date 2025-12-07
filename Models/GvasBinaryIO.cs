using System;
using System.IO;
using System.Text;

namespace UnrealSavEditor.Models
{
    /// <summary>
    /// Binary reader specialized for GVAS format
    /// </summary>
    public class GvasBinaryReader : BinaryReader
    {
        public GvasBinaryReader(Stream input) : base(input, Encoding.UTF8, leaveOpen: false) { }

        /// <summary>
        /// Reads an FString (length-prefixed string with optional Unicode support)
        /// </summary>
        public string ReadFString()
        {
            var length = ReadInt32();

            if (length == 0)
                return string.Empty;

            bool isUnicode = length < 0;
            if (isUnicode)
                length = -length;

            if (length > 10000000) // Sanity check
                throw new InvalidDataException($"String length too large: {length}");

            string result;
            if (isUnicode)
            {
                var bytes = ReadBytes(length * 2);
                result = Encoding.Unicode.GetString(bytes, 0, bytes.Length - 2); // Remove null terminator
            }
            else
            {
                var bytes = ReadBytes(length);
                result = Encoding.UTF8.GetString(bytes, 0, bytes.Length - 1); // Remove null terminator
            }

            return result;
        }

        /// <summary>
        /// Reads a GUID in Unreal format
        /// </summary>
        public Guid ReadGuid()
        {
            var bytes = ReadBytes(16);
            return new Guid(bytes);
        }
    }

    /// <summary>
    /// Binary writer specialized for GVAS format
    /// </summary>
    public class GvasBinaryWriter : BinaryWriter
    {
        public GvasBinaryWriter(Stream output) : base(output, Encoding.UTF8, leaveOpen: false) { }

        /// <summary>
        /// Writes an FString (length-prefixed string)
        /// </summary>
        public void WriteFString(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Write(0);
                return;
            }

            // Check if we need Unicode
            bool needsUnicode = false;
            foreach (char c in value)
            {
                if (c > 127)
                {
                    needsUnicode = true;
                    break;
                }
            }

            if (needsUnicode)
            {
                var bytes = Encoding.Unicode.GetBytes(value + "\0");
                Write(-(bytes.Length / 2));
                Write(bytes);
            }
            else
            {
                var bytes = Encoding.UTF8.GetBytes(value + "\0");
                Write(bytes.Length);
                Write(bytes);
            }
        }

        /// <summary>
        /// Writes a GUID in Unreal format
        /// </summary>
        public void WriteGuid(Guid guid)
        {
            Write(guid.ToByteArray());
        }
    }
}
