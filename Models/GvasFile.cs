using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UnrealSavEditor.Models
{
    /// <summary>
    /// Represents a complete GVAS (Unreal Engine Save Game) file
    /// </summary>
    public class GvasFile
    {
        public const string GVAS_MAGIC = "GVAS";

        // Header information
        public int SaveGameVersion { get; set; }
        public int PackageVersion { get; set; }
        public EngineVersion EngineVersion { get; set; } = new();
        public int CustomVersionFormat { get; set; }
        public List<CustomVersion> CustomVersions { get; set; } = new();
        public string SaveGameClassName { get; set; } = string.Empty;

        // Properties
        public List<GvasProperty> Properties { get; set; } = new();

        // Raw data for portions we can't parse
        public byte[]? UnparsedData { get; set; }

        // File path
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Load a GVAS file from disk
        /// </summary>
        public static GvasFile Load(string path)
        {
            using var stream = File.OpenRead(path);
            using var reader = new GvasBinaryReader(stream);

            var file = new GvasFile { FilePath = path };
            file.Read(reader);
            return file;
        }

        /// <summary>
        /// Save the GVAS file to disk
        /// </summary>
        public void Save(string? path = null)
        {
            path ??= FilePath;
            if (string.IsNullOrEmpty(path))
                throw new InvalidOperationException("No file path specified");

            using var stream = File.Create(path);
            using var writer = new GvasBinaryWriter(stream);
            Write(writer);
        }

        private void Read(GvasBinaryReader reader)
        {
            // Read and validate magic
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (magic != GVAS_MAGIC)
                throw new InvalidDataException($"Invalid GVAS magic: expected '{GVAS_MAGIC}', got '{magic}'");

            // Read header
            SaveGameVersion = reader.ReadInt32();
            PackageVersion = reader.ReadInt32();

            // Read engine version
            EngineVersion = new EngineVersion
            {
                Major = reader.ReadUInt16(),
                Minor = reader.ReadUInt16(),
                Patch = reader.ReadUInt16(),
                Build = reader.ReadUInt32(),
                BuildId = reader.ReadFString()
            };

            // Read custom version format
            CustomVersionFormat = reader.ReadInt32();

            // Read custom versions
            int customVersionCount = reader.ReadInt32();
            for (int i = 0; i < customVersionCount; i++)
            {
                CustomVersions.Add(new CustomVersion
                {
                    Key = reader.ReadGuid(),
                    Version = reader.ReadInt32()
                });
            }

            // Read save game class name
            SaveGameClassName = reader.ReadFString();

            // Read properties
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var property = GvasProperty.Read(reader);
                if (property == null || property.Name == "None")
                    break;
                Properties.Add(property);
            }

            // Store any remaining unparsed data
            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                UnparsedData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            }
        }

        private void Write(GvasBinaryWriter writer)
        {
            // Write magic
            writer.Write(Encoding.ASCII.GetBytes(GVAS_MAGIC));

            // Write header
            writer.Write(SaveGameVersion);
            writer.Write(PackageVersion);

            // Write engine version
            writer.Write(EngineVersion.Major);
            writer.Write(EngineVersion.Minor);
            writer.Write(EngineVersion.Patch);
            writer.Write(EngineVersion.Build);
            writer.WriteFString(EngineVersion.BuildId);

            // Write custom version format
            writer.Write(CustomVersionFormat);

            // Write custom versions
            writer.Write(CustomVersions.Count);
            foreach (var cv in CustomVersions)
            {
                writer.WriteGuid(cv.Key);
                writer.Write(cv.Version);
            }

            // Write save game class name
            writer.WriteFString(SaveGameClassName);

            // Write properties
            foreach (var property in Properties)
            {
                property.Write(writer);
            }

            // Write terminator
            writer.WriteFString("None");

            // Write any unparsed data
            if (UnparsedData != null)
            {
                writer.Write(UnparsedData);
            }
        }
    }

    public class EngineVersion
    {
        public ushort Major { get; set; }
        public ushort Minor { get; set; }
        public ushort Patch { get; set; }
        public uint Build { get; set; }
        public string BuildId { get; set; } = string.Empty;

        public override string ToString() => $"{Major}.{Minor}.{Patch}-{Build}";
    }

    public class CustomVersion
    {
        public Guid Key { get; set; }
        public int Version { get; set; }
    }
}
