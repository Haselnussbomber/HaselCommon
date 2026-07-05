using System.Globalization;
using System.IO;
using System.IO.Compression;

namespace HaselCommon.Utils;

public static class FileUtils
{
    public static string GetHumanReadableSize(double size, int precision = 2)
    {
        const string suffixes = " KMGTPEZY";
        var i = 0;

        for (; i < suffixes.Length - 1; i++)
        {
            if (size > 1024)
                size /= 1024.0;
            else
                break;
        }

        var finalSize = Math.Max(size, 0.1);
        var formattedSize = finalSize.ToString("F" + precision, CultureInfo.InvariantCulture).Replace(".00", string.Empty);
        var suffix = suffixes[i].ToString().Trim() + "B";

        return $"{formattedSize} {suffix}";
    }

    public static int CountGZippedLines(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException("The specified file was not found.", filePath);

        Span<byte> buffer = stackalloc byte[64 * 1024];
        var lineCount = 0;

        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: buffer.Length);
        using var gzip = new GZipStream(stream, CompressionMode.Decompress);

        int bytesRead;
        while ((bytesRead = gzip.Read(buffer)) > 0)
            lineCount += buffer[..bytesRead].Count((byte)'\n');

        return lineCount;
    }
}
