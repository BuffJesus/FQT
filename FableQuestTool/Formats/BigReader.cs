using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FableQuestTool.Formats;

internal static class BigReader
{
    public static BigArchive Read(Stream stream)
    {
        if (!stream.CanSeek)
        {
            throw new InvalidOperationException("BIG reader requires a seekable stream.");
        }

        using BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, leaveOpen: true);
        string magic = new string(reader.ReadChars(4));
        if (magic != "BIGB" && magic != "B\0\0\0")
        {
            throw new InvalidDataException($"Unsupported BIG magic '{magic}'.");
        }

        uint version = reader.ReadUInt32();
        uint bankHeaderOffset = reader.ReadUInt32();
        uint contentType = reader.ReadUInt32();

        BigArchive archive = new BigArchive
        {
            Version = version,
            ContentType = contentType
        };

        stream.Position = bankHeaderOffset;
        uint bankCount = reader.ReadUInt32();

        string? sourcePath = (stream is FileStream fileStream) ? fileStream.Name : null;

        for (uint i = 0; i < bankCount; i++)
        {
            string bankName = ReadNullTerminatedString(reader);
            uint bankId = reader.ReadUInt32();
            uint entryCount = reader.ReadUInt32();
            uint entryStartOffset = reader.ReadUInt32();
            uint bankLength = reader.ReadUInt32();
            uint blockSize = reader.ReadUInt32();

            BigBank bank = new BigBank(bankName)
            {
                Id = bankId,
                EntryStartOffset = entryStartOffset,
                Length = bankLength,
                BlockSize = blockSize
            };

            ReadEntries(reader, bank, entryStartOffset, entryCount, sourcePath);
            archive.AddBank(bank);
        }

        return archive;
    }

    private static void ReadEntries(BinaryReader reader, BigBank bank, uint entryStartOffset, uint entryCount, string? sourcePath)
    {
        Stream stream = reader.BaseStream;
        long originalPos = stream.Position;
        stream.Position = entryStartOffset;

        uint typeCount = reader.ReadUInt32();
        stream.Position += typeCount * 8;

        for (uint i = 0; i < entryCount; i++)
        {
            BigEntry entry = ReadEntry(reader, sourcePath);
            bank.AddEntry(entry);
        }

        stream.Position = originalPos;
    }

    private static BigEntry ReadEntry(BinaryReader reader, string? sourcePath)
    {
        uint magic = reader.ReadUInt32();
        uint id = reader.ReadUInt32();
        uint type = reader.ReadUInt32();
        uint length = reader.ReadUInt32();
        uint dataOffset = reader.ReadUInt32();
        uint devFileType = reader.ReadUInt32();
        uint nameLength = reader.ReadUInt32();

        string symbolName = ReadString(reader, nameLength);
        uint devCrc = reader.ReadUInt32();
        uint devSourceCount = reader.ReadUInt32();

        BigEntry entry = new BigEntry(symbolName, id, type)
        {
            Magic = magic,
            DevFileType = devFileType,
            DevCrc = devCrc,
            DataOffset = dataOffset
        };

        for (uint i = 0; i < devSourceCount; i++)
        {
            uint sourceLen = reader.ReadUInt32();
            string source = ReadString(reader, sourceLen);
            entry.DevSources.Add(source);
        }

        uint subHeaderLength = reader.ReadUInt32();
        entry.SubHeader = subHeaderLength == 0
            ? Array.Empty<byte>()
            : reader.ReadBytes((int)subHeaderLength);

        entry.SetSource(sourcePath, dataOffset, length);
        return entry;
    }

    private static string ReadNullTerminatedString(BinaryReader reader)
    {
        List<byte> bytes = new List<byte>();
        while (true)
        {
            int value = reader.BaseStream.ReadByte();
            if (value == -1 || value == 0 || value == 255)
            {
                break;
            }
            bytes.Add((byte)value);
        }
        return Encoding.ASCII.GetString(bytes.ToArray());
    }

    private static string ReadString(BinaryReader reader, uint length)
    {
        if (length == 0)
        {
            return string.Empty;
        }
        byte[] bytes = reader.ReadBytes((int)length);
        return Encoding.ASCII.GetString(bytes);
    }
}
