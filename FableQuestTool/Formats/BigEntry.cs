using FableQuestTool.Core;
using System;
using System.Collections.Generic;
using System.IO;

namespace FableQuestTool.Formats;

public sealed class BigEntry
{
    public uint Magic { get; internal set; } = 42;
    public string SymbolName { get; set; }
    public uint Id { get; set; }
    public uint Type { get; set; }
    public uint Length { get; internal set; }
    public uint DataOffset { get; internal set; }
    public uint DevFileType { get; set; }
    public uint DevCrc { get; set; }
    public List<string> DevSources { get; } = new();
    public byte[] SubHeader { get; set; } = Array.Empty<byte>();
    public bool IsModified { get; private set; }

    internal string? SourcePath { get; private set; }
    internal uint SourceDataOffset { get; private set; }
    internal uint SourceLength { get; private set; }

    private byte[]? _data;

    public BigEntry(string symbolName, uint id, uint type, byte[] data)
    {
        Guard.NotNullOrEmpty(symbolName, nameof(symbolName));
        Guard.NotNull(data, nameof(data));
        SymbolName = symbolName;
        Id = id;
        Type = type;
        ReplaceData(data);
    }

    internal BigEntry(string symbolName, uint id, uint type)
    {
        Guard.NotNullOrEmpty(symbolName, nameof(symbolName));
        SymbolName = symbolName;
        Id = id;
        Type = type;
    }

    public byte[] GetData()
    {
        if (_data != null)
        {
            return _data;
        }

        if (SourcePath == null)
        {
            return Array.Empty<byte>();
        }

        using FileStream stream = File.OpenRead(SourcePath);
        stream.Position = SourceDataOffset;
        byte[] buffer = new byte[SourceLength];
        int read = stream.Read(buffer, 0, buffer.Length);
        if (read != buffer.Length)
        {
            Array.Resize(ref buffer, read);
        }
        _data = buffer;
        return _data;
    }

    public void ReplaceData(byte[] data)
    {
        Guard.NotNull(data, nameof(data));
        _data = data;
        Length = (uint)data.Length;
        IsModified = true;
    }

    public void ExtractTo(string filePath)
    {
        Guard.NotNullOrEmpty(filePath, nameof(filePath));
        using FileStream output = File.Create(filePath);
        WriteDataTo(output);
    }

    public void WriteDataTo(Stream output)
    {
        Guard.NotNull(output, nameof(output));
        if (TryGetInlineData(out byte[]? data) && data != null)
        {
            output.Write(data, 0, data.Length);
            return;
        }

        if (SourcePath != null)
        {
            using FileStream source = File.OpenRead(SourcePath);
            source.Position = SourceDataOffset;
            CopyBytes(source, output, SourceLength);
        }
    }

    internal void SetSource(string? path, uint offset, uint length)
    {
        SourcePath = path;
        SourceDataOffset = offset;
        SourceLength = length;
        Length = length;
    }

    internal bool TryGetInlineData(out byte[]? data)
    {
        data = _data;
        return data != null;
    }

    private static void CopyBytes(Stream source, Stream destination, uint length)
    {
        byte[] buffer = new byte[81920];
        uint remaining = length;
        while (remaining > 0)
        {
            int toRead = (int)Math.Min(buffer.Length, remaining);
            int read = source.Read(buffer, 0, toRead);
            if (read == 0)
            {
                break;
            }
            destination.Write(buffer, 0, read);
            remaining -= (uint)read;
        }
    }
}
