using System.Buffers;

namespace Cocoar.Json.Mutable;
internal static class JsonStringUnescaper
{
    public static byte[] Unescape(ReadOnlySpan<byte> escapedUtf8)
    {
        // Quick check: if no backslashes, no escaping needed
        if (escapedUtf8.IndexOf((byte)'\\') < 0)
        {
            return escapedUtf8.ToArray();
        }
        
        // Worst case: output is same size as input (most escapes reduce size)
        var buffer = ArrayPool<byte>.Shared.Rent(escapedUtf8.Length);
        int writePos = 0; // hoisted so finally can zero used bytes
        try
        {
            int readPos = 0;
            
            while (readPos < escapedUtf8.Length)
            {
                byte b = escapedUtf8[readPos];
                
                if (b != (byte)'\\')
                {
                    // Regular byte - copy as-is
                    buffer[writePos++] = b;
                    readPos++;
                    continue;
                }
                
                // Found backslash - check what follows
                readPos++; // skip backslash
                
                if (readPos >= escapedUtf8.Length)
                    throw new FormatException("Invalid JSON: escape sequence at end of string");
                
                byte escapeChar = escapedUtf8[readPos++];
                
                switch (escapeChar)
                {
                    case (byte)'"':
                        buffer[writePos++] = (byte)'"';
                        break;
                    
                    case (byte)'\\':
                        buffer[writePos++] = (byte)'\\';
                        break;
                    
                    case (byte)'/':
                        buffer[writePos++] = (byte)'/';
                        break;
                    
                    case (byte)'b':
                        buffer[writePos++] = (byte)'\b';
                        break;
                    
                    case (byte)'f':
                        buffer[writePos++] = (byte)'\f';
                        break;
                    
                    case (byte)'n':
                        buffer[writePos++] = (byte)'\n';
                        break;
                    
                    case (byte)'r':
                        buffer[writePos++] = (byte)'\r';
                        break;
                    
                    case (byte)'t':
                        buffer[writePos++] = (byte)'\t';
                        break;
                    
                    case (byte)'u':
                        // Unicode escape: \uXXXX
                        if (readPos + 4 > escapedUtf8.Length)
                            throw new FormatException("Invalid JSON: incomplete \\uXXXX escape sequence");
                        int codePoint = ParseHexQuad(escapedUtf8.Slice(readPos, 4));
                        readPos += 4;
                        
                        // Handle UTF-16 surrogate pairs for characters outside BMP (U+10000 to U+10FFFF)
                        if (codePoint >= 0xD800 && codePoint <= 0xDBFF)
                        {
                            // High surrogate - must be followed by low surrogate
                            if (readPos + 6 > escapedUtf8.Length || 
                                escapedUtf8[readPos] != (byte)'\\' || 
                                escapedUtf8[readPos + 1] != (byte)'u')
                            {
                                throw new FormatException("Invalid JSON: high surrogate not followed by low surrogate");
                            }
                            
                            readPos += 2; // skip \u
                            int lowSurrogate = ParseHexQuad(escapedUtf8.Slice(readPos, 4));
                            readPos += 4;
                            
                            if (lowSurrogate < 0xDC00 || lowSurrogate > 0xDFFF)
                                throw new FormatException("Invalid JSON: invalid low surrogate value");
                            
                            // Decode surrogate pair to actual code point
                            codePoint = 0x10000 + ((codePoint - 0xD800) << 10) + (lowSurrogate - 0xDC00);
                        }
                        
                        // Encode code point as UTF-8
                        writePos += EncodeUtf8(codePoint, buffer.AsSpan(writePos));
                        break;
                    
                    default:
                        throw new FormatException($"Invalid JSON: unknown escape sequence \\{(char)escapeChar}");
                }
            }
            
            // Return exact-sized array
            var result = new byte[writePos];
            Array.Copy(buffer, result, writePos);
            return result;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    private static int ParseHexQuad(ReadOnlySpan<byte> hexDigits)
    {
        int result = 0;
        for (int i = 0; i < 4; i++)
        {
            result <<= 4;
            byte b = hexDigits[i];
            
            if (b >= (byte)'0' && b <= (byte)'9')
                result |= b - (byte)'0';
            else if (b >= (byte)'A' && b <= (byte)'F')
                result |= b - (byte)'A' + 10;
            else if (b >= (byte)'a' && b <= (byte)'f')
                result |= b - (byte)'a' + 10;
            else
                throw new FormatException($"Invalid JSON: non-hex digit in \\uXXXX escape: {(char)b}");
        }
        return result;
    }
    private static int EncodeUtf8(int codePoint, Span<byte> buffer)
    {
        if (codePoint < 0x80)
        {
            // 1-byte sequence: 0xxxxxxx
            buffer[0] = (byte)codePoint;
            return 1;
        }
        else if (codePoint < 0x800)
        {
            // 2-byte sequence: 110xxxxx 10xxxxxx
            buffer[0] = (byte)(0xC0 | (codePoint >> 6));
            buffer[1] = (byte)(0x80 | (codePoint & 0x3F));
            return 2;
        }
        else if (codePoint < 0x10000)
        {
            // 3-byte sequence: 1110xxxx 10xxxxxx 10xxxxxx
            buffer[0] = (byte)(0xE0 | (codePoint >> 12));
            buffer[1] = (byte)(0x80 | ((codePoint >> 6) & 0x3F));
            buffer[2] = (byte)(0x80 | (codePoint & 0x3F));
            return 3;
        }
        else if (codePoint < 0x110000)
        {
            // 4-byte sequence: 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx
            buffer[0] = (byte)(0xF0 | (codePoint >> 18));
            buffer[1] = (byte)(0x80 | ((codePoint >> 12) & 0x3F));
            buffer[2] = (byte)(0x80 | ((codePoint >> 6) & 0x3F));
            buffer[3] = (byte)(0x80 | (codePoint & 0x3F));
            return 4;
        }
        else
        {
            throw new FormatException($"Invalid Unicode code point: U+{codePoint:X}");
        }
    }
}
