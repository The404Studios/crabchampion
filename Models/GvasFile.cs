using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace UnrealSavEditor.Models
{
    /// <summary>
    /// Compression type detected in save file
    /// </summary>
    public enum SaveCompressionType
    {
        None,
        GZip,
        Zlib,
        Unknown
    }

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

        // Compression info
        public SaveCompressionType OriginalCompression { get; set; } = SaveCompressionType.None;

        /// <summary>
        /// Detect the compression type of a file
        /// </summary>
        public static SaveCompressionType DetectCompression(byte[] header)
        {
            if (header.Length < 4)
                return SaveCompressionType.Unknown;

            // Check for GVAS magic (uncompressed)
            if (header[0] == 'G' && header[1] == 'V' && header[2] == 'A' && header[3] == 'S')
                return SaveCompressionType.None;

            // Check for GZip magic (1F 8B)
            if (header[0] == 0x1F && header[1] == 0x8B)
                return SaveCompressionType.GZip;

            // Check for Zlib magic (78 01, 78 9C, 78 DA)
            if (header[0] == 0x78 && (header[1] == 0x01 || header[1] == 0x9C || header[1] == 0xDA))
                return SaveCompressionType.Zlib;

            return SaveCompressionType.Unknown;
        }

        /// <summary>
        /// Decompress GZip data
        /// </summary>
        private static byte[] DecompressGZip(byte[] compressedData)
        {
            using var compressedStream = new MemoryStream(compressedData);
            using var gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            gzipStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }

        /// <summary>
        /// Decompress Zlib data
        /// </summary>
        private static byte[] DecompressZlib(byte[] compressedData)
        {
            // Skip 2-byte Zlib header
            using var compressedStream = new MemoryStream(compressedData, 2, compressedData.Length - 2);
            using var deflateStream = new DeflateStream(compressedStream, CompressionMode.Decompress);
            using var resultStream = new MemoryStream();
            deflateStream.CopyTo(resultStream);
            return resultStream.ToArray();
        }

        /// <summary>
        /// Compress data with GZip
        /// </summary>
        private static byte[] CompressGZip(byte[] data)
        {
            using var resultStream = new MemoryStream();
            using (var gzipStream = new GZipStream(resultStream, CompressionLevel.Optimal))
            {
                gzipStream.Write(data, 0, data.Length);
            }
            return resultStream.ToArray();
        }

        /// <summary>
        /// Compress data with Zlib
        /// </summary>
        private static byte[] CompressZlib(byte[] data)
        {
            using var resultStream = new MemoryStream();
            // Write Zlib header (78 9C for default compression)
            resultStream.WriteByte(0x78);
            resultStream.WriteByte(0x9C);

            using (var deflateStream = new DeflateStream(resultStream, CompressionLevel.Optimal, leaveOpen: true))
            {
                deflateStream.Write(data, 0, data.Length);
            }
            return resultStream.ToArray();
        }

        /// <summary>
        /// Load a GVAS file from disk (handles compressed files automatically)
        /// </summary>
        public static GvasFile Load(string path)
        {
            var rawData = File.ReadAllBytes(path);

            if (rawData.Length < 4)
                throw new InvalidDataException("File is too small to be a valid save file");

            var compression = DetectCompression(rawData);
            byte[] data = rawData;

            switch (compression)
            {
                case SaveCompressionType.GZip:
                    try
                    {
                        data = DecompressGZip(rawData);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidDataException($"Failed to decompress GZip data: {ex.Message}", ex);
                    }
                    break;

                case SaveCompressionType.Zlib:
                    try
                    {
                        data = DecompressZlib(rawData);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidDataException($"Failed to decompress Zlib data: {ex.Message}", ex);
                    }
                    break;

                case SaveCompressionType.Unknown:
                    // Try to parse anyway - might be a different format
                    break;
            }

            // Verify we now have GVAS data
            if (data.Length >= 4)
            {
                var magic = Encoding.ASCII.GetString(data, 0, 4);
                if (magic != GVAS_MAGIC)
                {
                    throw new InvalidDataException(
                        $"Invalid save file format. Expected 'GVAS' magic, got '{magic}' (0x{data[0]:X2} 0x{data[1]:X2} 0x{data[2]:X2} 0x{data[3]:X2}).\n\n" +
                        "This file may be:\n" +
                        "• Using Oodle compression (not supported)\n" +
                        "• Encrypted\n" +
                        "• A different save format\n\n" +
                        $"Detected compression: {compression}");
                }
            }

            using var stream = new MemoryStream(data);
            using var reader = new GvasBinaryReader(stream);

            var file = new GvasFile
            {
                FilePath = path,
                OriginalCompression = compression
            };
            file.Read(reader);
            return file;
        }

        /// <summary>
        /// Save the GVAS file to disk (preserves original compression)
        /// </summary>
        public void Save(string? path = null)
        {
            path ??= FilePath;
            if (string.IsNullOrEmpty(path))
                throw new InvalidOperationException("No file path specified");

            // Write to memory first
            byte[] data;
            using (var stream = new MemoryStream())
            {
                using (var writer = new GvasBinaryWriter(stream))
                {
                    Write(writer);
                }
                data = stream.ToArray();
            }

            // Apply original compression if any
            byte[] outputData = OriginalCompression switch
            {
                SaveCompressionType.GZip => CompressGZip(data),
                SaveCompressionType.Zlib => CompressZlib(data),
                _ => data
            };

            File.WriteAllBytes(path, outputData);
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

            // Read properties with error handling
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                try
                {
                    var property = GvasProperty.Read(reader);
                    if (property == null || property.Name == "None")
                        break;
                    Properties.Add(property);
                }
                catch (Exception ex)
                {
                    // Store remaining data as unparsed
                    var remaining = reader.BaseStream.Length - reader.BaseStream.Position;
                    if (remaining > 0)
                    {
                        reader.BaseStream.Position -= 4; // Back up a bit
                        UnparsedData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
                    }
                    System.Diagnostics.Debug.WriteLine($"Property parsing stopped: {ex.Message}");
                    break;
                }
            }

            // Store any remaining unparsed data
            if (reader.BaseStream.Position < reader.BaseStream.Length && UnparsedData == null)
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

        /// <summary>
        /// Get information about the file format
        /// </summary>
        public string GetFormatInfo()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Save Game Version: {SaveGameVersion}");
            sb.AppendLine($"Package Version: {PackageVersion}");
            sb.AppendLine($"Engine: {EngineVersion}");
            sb.AppendLine($"Class: {SaveGameClassName}");
            sb.AppendLine($"Properties: {Properties.Count}");
            sb.AppendLine($"Compression: {OriginalCompression}");
            if (UnparsedData != null)
                sb.AppendLine($"Unparsed Data: {UnparsedData.Length} bytes");
            return sb.ToString();
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
