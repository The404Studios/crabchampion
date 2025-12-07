using System;
using System.Collections.Generic;
using System.Numerics;

namespace UnrealSavEditor.Models
{
    /// <summary>
    /// Base class for all GVAS properties
    /// </summary>
    public abstract class GvasProperty
    {
        public string Name { get; set; } = string.Empty;
        public string TypeName { get; set; } = string.Empty;
        public long Size { get; set; }

        public abstract object? GetValue();
        public abstract void SetValue(object? value);
        public abstract void Write(GvasBinaryWriter writer);

        public static GvasProperty? Read(GvasBinaryReader reader)
        {
            var name = reader.ReadFString();
            if (string.IsNullOrEmpty(name) || name == "None")
                return null;

            var typeName = reader.ReadFString();
            var size = reader.ReadInt64();

            GvasProperty property = typeName switch
            {
                "IntProperty" => new IntProperty(),
                "UInt32Property" => new UInt32Property(),
                "Int64Property" => new Int64Property(),
                "UInt64Property" => new UInt64Property(),
                "FloatProperty" => new FloatProperty(),
                "DoubleProperty" => new DoubleProperty(),
                "BoolProperty" => new BoolProperty(),
                "ByteProperty" => new ByteProperty(),
                "StrProperty" => new StrProperty(),
                "NameProperty" => new NameProperty(),
                "TextProperty" => new TextProperty(),
                "EnumProperty" => new EnumProperty(),
                "StructProperty" => new StructProperty(),
                "ArrayProperty" => new ArrayProperty(),
                "MapProperty" => new MapProperty(),
                "SetProperty" => new SetProperty(),
                "ObjectProperty" => new ObjectProperty(),
                "SoftObjectProperty" => new SoftObjectProperty(),
                _ => new UnknownProperty()
            };

            property.Name = name;
            property.TypeName = typeName;
            property.Size = size;
            property.ReadValue(reader);

            return property;
        }

        protected abstract void ReadValue(GvasBinaryReader reader);
    }

    public class IntProperty : GvasProperty
    {
        public int Value { get; set; }

        public override object GetValue() => Value;
        public override void SetValue(object? value) => Value = Convert.ToInt32(value);

        protected override void ReadValue(GvasBinaryReader reader)
        {
            reader.ReadByte(); // terminator
            Value = reader.ReadInt32();
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);
            writer.Write(4L);
            writer.Write((byte)0);
            writer.Write(Value);
        }
    }

    public class UInt32Property : GvasProperty
    {
        public uint Value { get; set; }

        public override object GetValue() => Value;
        public override void SetValue(object? value) => Value = Convert.ToUInt32(value);

        protected override void ReadValue(GvasBinaryReader reader)
        {
            reader.ReadByte();
            Value = reader.ReadUInt32();
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);
            writer.Write(4L);
            writer.Write((byte)0);
            writer.Write(Value);
        }
    }

    public class Int64Property : GvasProperty
    {
        public long Value { get; set; }

        public override object GetValue() => Value;
        public override void SetValue(object? value) => Value = Convert.ToInt64(value);

        protected override void ReadValue(GvasBinaryReader reader)
        {
            reader.ReadByte();
            Value = reader.ReadInt64();
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);
            writer.Write(8L);
            writer.Write((byte)0);
            writer.Write(Value);
        }
    }

    public class UInt64Property : GvasProperty
    {
        public ulong Value { get; set; }

        public override object GetValue() => Value;
        public override void SetValue(object? value) => Value = Convert.ToUInt64(value);

        protected override void ReadValue(GvasBinaryReader reader)
        {
            reader.ReadByte();
            Value = reader.ReadUInt64();
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);
            writer.Write(8L);
            writer.Write((byte)0);
            writer.Write(Value);
        }
    }

    public class FloatProperty : GvasProperty
    {
        public float Value { get; set; }

        public override object GetValue() => Value;
        public override void SetValue(object? value) => Value = Convert.ToSingle(value);

        protected override void ReadValue(GvasBinaryReader reader)
        {
            reader.ReadByte();
            Value = reader.ReadSingle();
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);
            writer.Write(4L);
            writer.Write((byte)0);
            writer.Write(Value);
        }
    }

    public class DoubleProperty : GvasProperty
    {
        public double Value { get; set; }

        public override object GetValue() => Value;
        public override void SetValue(object? value) => Value = Convert.ToDouble(value);

        protected override void ReadValue(GvasBinaryReader reader)
        {
            reader.ReadByte();
            Value = reader.ReadDouble();
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);
            writer.Write(8L);
            writer.Write((byte)0);
            writer.Write(Value);
        }
    }

    public class BoolProperty : GvasProperty
    {
        public bool Value { get; set; }

        public override object GetValue() => Value;
        public override void SetValue(object? value) => Value = Convert.ToBoolean(value);

        protected override void ReadValue(GvasBinaryReader reader)
        {
            Value = reader.ReadByte() != 0;
            reader.ReadByte(); // terminator
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);
            writer.Write(0L);
            writer.Write(Value ? (byte)1 : (byte)0);
            writer.Write((byte)0);
        }
    }

    public class ByteProperty : GvasProperty
    {
        public string EnumType { get; set; } = "None";
        public byte ByteValue { get; set; }
        public string EnumValue { get; set; } = string.Empty;

        public override object GetValue() => EnumType == "None" ? ByteValue : EnumValue;
        public override void SetValue(object? value)
        {
            if (EnumType == "None")
                ByteValue = Convert.ToByte(value);
            else
                EnumValue = value?.ToString() ?? string.Empty;
        }

        protected override void ReadValue(GvasBinaryReader reader)
        {
            EnumType = reader.ReadFString();
            reader.ReadByte();
            if (EnumType == "None")
                ByteValue = reader.ReadByte();
            else
                EnumValue = reader.ReadFString();
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);
            writer.Write(EnumType == "None" ? 1L : Size);
            writer.WriteFString(EnumType);
            writer.Write((byte)0);
            if (EnumType == "None")
                writer.Write(ByteValue);
            else
                writer.WriteFString(EnumValue);
        }
    }

    public class StrProperty : GvasProperty
    {
        public string Value { get; set; } = string.Empty;

        public override object GetValue() => Value;
        public override void SetValue(object? value) => Value = value?.ToString() ?? string.Empty;

        protected override void ReadValue(GvasBinaryReader reader)
        {
            reader.ReadByte();
            Value = reader.ReadFString();
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);

            using var ms = new System.IO.MemoryStream();
            using var tempWriter = new GvasBinaryWriter(ms);
            tempWriter.WriteFString(Value);
            var data = ms.ToArray();

            writer.Write((long)data.Length);
            writer.Write((byte)0);
            writer.Write(data);
        }
    }

    public class NameProperty : GvasProperty
    {
        public string Value { get; set; } = string.Empty;

        public override object GetValue() => Value;
        public override void SetValue(object? value) => Value = value?.ToString() ?? string.Empty;

        protected override void ReadValue(GvasBinaryReader reader)
        {
            reader.ReadByte();
            Value = reader.ReadFString();
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);

            using var ms = new System.IO.MemoryStream();
            using var tempWriter = new GvasBinaryWriter(ms);
            tempWriter.WriteFString(Value);
            var data = ms.ToArray();

            writer.Write((long)data.Length);
            writer.Write((byte)0);
            writer.Write(data);
        }
    }

    public class TextProperty : GvasProperty
    {
        public byte[] RawData { get; set; } = Array.Empty<byte>();
        public string DisplayText { get; set; } = string.Empty;

        public override object GetValue() => DisplayText;
        public override void SetValue(object? value) => DisplayText = value?.ToString() ?? string.Empty;

        protected override void ReadValue(GvasBinaryReader reader)
        {
            reader.ReadByte();
            RawData = reader.ReadBytes((int)Size);
            // Try to extract readable text from the raw data
            try
            {
                using var ms = new System.IO.MemoryStream(RawData);
                using var tempReader = new GvasBinaryReader(ms);
                tempReader.ReadInt32(); // flags
                tempReader.ReadByte(); // history type
                // Skip namespace if present
                if (ms.Position < ms.Length - 4)
                {
                    DisplayText = tempReader.ReadFString();
                }
            }
            catch { DisplayText = "[Complex Text]"; }
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);
            writer.Write((long)RawData.Length);
            writer.Write((byte)0);
            writer.Write(RawData);
        }
    }

    public class EnumProperty : GvasProperty
    {
        public string EnumType { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;

        public override object GetValue() => Value;
        public override void SetValue(object? value) => Value = value?.ToString() ?? string.Empty;

        protected override void ReadValue(GvasBinaryReader reader)
        {
            EnumType = reader.ReadFString();
            reader.ReadByte();
            Value = reader.ReadFString();
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);

            using var ms = new System.IO.MemoryStream();
            using var tempWriter = new GvasBinaryWriter(ms);
            tempWriter.WriteFString(Value);
            var data = ms.ToArray();

            writer.Write((long)data.Length);
            writer.WriteFString(EnumType);
            writer.Write((byte)0);
            writer.Write(data);
        }
    }

    public class StructProperty : GvasProperty
    {
        public string StructType { get; set; } = string.Empty;
        public Guid StructGuid { get; set; }
        public List<GvasProperty> Properties { get; set; } = new();
        public byte[]? RawData { get; set; }

        // Specific struct types
        public Vector3? Vector { get; set; }
        public Vector4? Quat { get; set; }
        public Guid? GuidValue { get; set; }
        public DateTime? DateTime { get; set; }

        public override object? GetValue()
        {
            if (Vector.HasValue) return Vector.Value;
            if (Quat.HasValue) return Quat.Value;
            if (GuidValue.HasValue) return GuidValue.Value;
            if (DateTime.HasValue) return DateTime.Value;
            return Properties;
        }

        public override void SetValue(object? value) { }

        protected override void ReadValue(GvasBinaryReader reader)
        {
            StructType = reader.ReadFString();
            StructGuid = reader.ReadGuid();
            reader.ReadByte();

            var startPos = reader.BaseStream.Position;
            var endPos = startPos + Size;

            try
            {
                switch (StructType)
                {
                    case "Vector":
                    case "Rotator":
                        Vector = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        break;
                    case "Quat":
                        Quat = new Vector4(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        break;
                    case "Guid":
                        GuidValue = reader.ReadGuid();
                        break;
                    case "DateTime":
                        DateTime = System.DateTime.FromBinary(reader.ReadInt64());
                        break;
                    case "LinearColor":
                        RawData = reader.ReadBytes((int)Size);
                        break;
                    default:
                        // Try to read as property list
                        while (reader.BaseStream.Position < endPos)
                        {
                            var prop = GvasProperty.Read(reader);
                            if (prop == null || prop.Name == "None") break;
                            Properties.Add(prop);
                        }
                        // Read remaining bytes
                        if (reader.BaseStream.Position < endPos)
                        {
                            RawData = reader.ReadBytes((int)(endPos - reader.BaseStream.Position));
                        }
                        break;
                }
            }
            catch
            {
                // If parsing fails, read remaining raw data
                reader.BaseStream.Position = startPos;
                RawData = reader.ReadBytes((int)Size);
            }
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);

            using var ms = new System.IO.MemoryStream();
            using var tempWriter = new GvasBinaryWriter(ms);
            WriteStructData(tempWriter);
            var data = ms.ToArray();

            writer.Write((long)data.Length);
            writer.WriteFString(StructType);
            writer.WriteGuid(StructGuid);
            writer.Write((byte)0);
            writer.Write(data);
        }

        private void WriteStructData(GvasBinaryWriter writer)
        {
            switch (StructType)
            {
                case "Vector":
                case "Rotator":
                    if (Vector.HasValue)
                    {
                        writer.Write(Vector.Value.X);
                        writer.Write(Vector.Value.Y);
                        writer.Write(Vector.Value.Z);
                    }
                    break;
                case "Quat":
                    if (Quat.HasValue)
                    {
                        writer.Write(Quat.Value.X);
                        writer.Write(Quat.Value.Y);
                        writer.Write(Quat.Value.Z);
                        writer.Write(Quat.Value.W);
                    }
                    break;
                case "Guid":
                    if (GuidValue.HasValue)
                        writer.WriteGuid(GuidValue.Value);
                    break;
                case "DateTime":
                    if (DateTime.HasValue)
                        writer.Write(DateTime.Value.ToBinary());
                    break;
                default:
                    foreach (var prop in Properties)
                        prop.Write(writer);
                    writer.WriteFString("None");
                    if (RawData != null)
                        writer.Write(RawData);
                    break;
            }
        }
    }

    public class ArrayProperty : GvasProperty
    {
        public string InnerType { get; set; } = string.Empty;
        public List<object> Items { get; set; } = new();
        public string? StructType { get; set; }
        public Guid StructGuid { get; set; }

        public override object GetValue() => Items;
        public override void SetValue(object? value) { }

        protected override void ReadValue(GvasBinaryReader reader)
        {
            InnerType = reader.ReadFString();
            reader.ReadByte();

            var count = reader.ReadInt32();
            var startPos = reader.BaseStream.Position;

            if (InnerType == "StructProperty")
            {
                // Read struct array header
                var itemName = reader.ReadFString();
                var itemType = reader.ReadFString();
                var itemSize = reader.ReadInt64();
                StructType = reader.ReadFString();
                StructGuid = reader.ReadGuid();
                reader.ReadByte();

                for (int i = 0; i < count; i++)
                {
                    var structProp = new StructProperty
                    {
                        Name = itemName,
                        TypeName = itemType,
                        Size = itemSize / count,
                        StructType = StructType
                    };
                    // Read struct properties
                    var props = new List<GvasProperty>();
                    while (true)
                    {
                        var prop = GvasProperty.Read(reader);
                        if (prop == null || prop.Name == "None") break;
                        props.Add(prop);
                    }
                    structProp.Properties = props;
                    Items.Add(structProp);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    var item = InnerType switch
                    {
                        "IntProperty" => (object)reader.ReadInt32(),
                        "UInt32Property" => reader.ReadUInt32(),
                        "Int64Property" => reader.ReadInt64(),
                        "UInt64Property" => reader.ReadUInt64(),
                        "FloatProperty" => reader.ReadSingle(),
                        "DoubleProperty" => reader.ReadDouble(),
                        "BoolProperty" => reader.ReadByte() != 0,
                        "ByteProperty" => reader.ReadByte(),
                        "StrProperty" => reader.ReadFString(),
                        "NameProperty" => reader.ReadFString(),
                        "ObjectProperty" => reader.ReadFString(),
                        "SoftObjectProperty" => reader.ReadFString(),
                        _ => reader.ReadBytes((int)(Size - 4) / Math.Max(count, 1))
                    };
                    Items.Add(item);
                }
            }
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);

            using var ms = new System.IO.MemoryStream();
            using var tempWriter = new GvasBinaryWriter(ms);

            tempWriter.Write(Items.Count);

            if (InnerType == "StructProperty" && Items.Count > 0 && Items[0] is StructProperty firstStruct)
            {
                // Write struct array header
                tempWriter.WriteFString(firstStruct.Name);
                tempWriter.WriteFString("StructProperty");

                using var structMs = new System.IO.MemoryStream();
                using var structWriter = new GvasBinaryWriter(structMs);
                foreach (StructProperty item in Items)
                {
                    foreach (var prop in item.Properties)
                        prop.Write(structWriter);
                    structWriter.WriteFString("None");
                }
                var structData = structMs.ToArray();

                tempWriter.Write((long)structData.Length);
                tempWriter.WriteFString(StructType ?? firstStruct.StructType);
                tempWriter.WriteGuid(StructGuid);
                tempWriter.Write((byte)0);
                tempWriter.Write(structData);
            }
            else
            {
                foreach (var item in Items)
                {
                    switch (InnerType)
                    {
                        case "IntProperty": tempWriter.Write(Convert.ToInt32(item)); break;
                        case "UInt32Property": tempWriter.Write(Convert.ToUInt32(item)); break;
                        case "Int64Property": tempWriter.Write(Convert.ToInt64(item)); break;
                        case "UInt64Property": tempWriter.Write(Convert.ToUInt64(item)); break;
                        case "FloatProperty": tempWriter.Write(Convert.ToSingle(item)); break;
                        case "DoubleProperty": tempWriter.Write(Convert.ToDouble(item)); break;
                        case "BoolProperty": tempWriter.Write(Convert.ToBoolean(item) ? (byte)1 : (byte)0); break;
                        case "ByteProperty": tempWriter.Write(Convert.ToByte(item)); break;
                        case "StrProperty":
                        case "NameProperty":
                        case "ObjectProperty":
                        case "SoftObjectProperty":
                            tempWriter.WriteFString(item.ToString() ?? "");
                            break;
                        default:
                            if (item is byte[] bytes) tempWriter.Write(bytes);
                            break;
                    }
                }
            }

            var data = ms.ToArray();
            writer.Write((long)data.Length);
            writer.WriteFString(InnerType);
            writer.Write((byte)0);
            writer.Write(data);
        }
    }

    public class MapProperty : GvasProperty
    {
        public string KeyType { get; set; } = string.Empty;
        public string ValueType { get; set; } = string.Empty;
        public Dictionary<object, object> Entries { get; set; } = new();
        public byte[]? RawData { get; set; }

        public override object GetValue() => Entries;
        public override void SetValue(object? value) { }

        protected override void ReadValue(GvasBinaryReader reader)
        {
            KeyType = reader.ReadFString();
            ValueType = reader.ReadFString();
            reader.ReadByte();
            reader.ReadInt32(); // num keys to remove
            var count = reader.ReadInt32();

            // For complex maps, store raw data
            RawData = reader.ReadBytes((int)Size - 8);
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);
            writer.Write(Size);
            writer.WriteFString(KeyType);
            writer.WriteFString(ValueType);
            writer.Write((byte)0);
            writer.Write(0); // num keys to remove
            writer.Write(Entries.Count);
            if (RawData != null)
                writer.Write(RawData);
        }
    }

    public class SetProperty : GvasProperty
    {
        public string ElementType { get; set; } = string.Empty;
        public List<object> Items { get; set; } = new();
        public byte[]? RawData { get; set; }

        public override object GetValue() => Items;
        public override void SetValue(object? value) { }

        protected override void ReadValue(GvasBinaryReader reader)
        {
            ElementType = reader.ReadFString();
            reader.ReadByte();
            reader.ReadInt32(); // num elements to remove
            var count = reader.ReadInt32();
            RawData = reader.ReadBytes((int)Size - 8);
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);
            writer.Write(Size);
            writer.WriteFString(ElementType);
            writer.Write((byte)0);
            writer.Write(0);
            writer.Write(Items.Count);
            if (RawData != null)
                writer.Write(RawData);
        }
    }

    public class ObjectProperty : GvasProperty
    {
        public string Value { get; set; } = string.Empty;

        public override object GetValue() => Value;
        public override void SetValue(object? value) => Value = value?.ToString() ?? string.Empty;

        protected override void ReadValue(GvasBinaryReader reader)
        {
            reader.ReadByte();
            Value = reader.ReadFString();
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);

            using var ms = new System.IO.MemoryStream();
            using var tempWriter = new GvasBinaryWriter(ms);
            tempWriter.WriteFString(Value);
            var data = ms.ToArray();

            writer.Write((long)data.Length);
            writer.Write((byte)0);
            writer.Write(data);
        }
    }

    public class SoftObjectProperty : GvasProperty
    {
        public string AssetPath { get; set; } = string.Empty;
        public string SubPath { get; set; } = string.Empty;

        public override object GetValue() => AssetPath;
        public override void SetValue(object? value) => AssetPath = value?.ToString() ?? string.Empty;

        protected override void ReadValue(GvasBinaryReader reader)
        {
            reader.ReadByte();
            AssetPath = reader.ReadFString();
            SubPath = reader.ReadFString();
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);

            using var ms = new System.IO.MemoryStream();
            using var tempWriter = new GvasBinaryWriter(ms);
            tempWriter.WriteFString(AssetPath);
            tempWriter.WriteFString(SubPath);
            var data = ms.ToArray();

            writer.Write((long)data.Length);
            writer.Write((byte)0);
            writer.Write(data);
        }
    }

    public class UnknownProperty : GvasProperty
    {
        public byte[] RawData { get; set; } = Array.Empty<byte>();

        public override object GetValue() => RawData;
        public override void SetValue(object? value) { }

        protected override void ReadValue(GvasBinaryReader reader)
        {
            reader.ReadByte();
            RawData = reader.ReadBytes((int)Size);
        }

        public override void Write(GvasBinaryWriter writer)
        {
            writer.WriteFString(Name);
            writer.WriteFString(TypeName);
            writer.Write((long)RawData.Length);
            writer.Write((byte)0);
            writer.Write(RawData);
        }
    }
}
