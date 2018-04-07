// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers
{
    // Note this is for use both in HTTP (https://tools.ietf.org/html/rfc6266) and MIME (https://tools.ietf.org/html/rfc2183)
    public class ContentDispositionHeaderValue
    {
        private const string FileNameString = "filename";
        private const string NameString = "name";
        private const string FileNameStarString = "filename*";
        private const string CreationDateString = "creation-date";
        private const string ModificationDateString = "modification-date";
        private const string ReadDateString = "read-date";
        private const string SizeString = "size";
        private static readonly char[] QuestionMark = new char[] { '?' };
        private static readonly char[] SingleQuote = new char[] { '\'' };

        private static readonly HttpHeaderParser<ContentDispositionHeaderValue> Parser
            = new GenericHeaderParser<ContentDispositionHeaderValue>(false, GetDispositionTypeLength);

        // Use list instead of dictionary since we may have multiple parameters with the same name.
        private ObjectCollection<NameValueHeaderValue> _parameters;
        private StringSegment _dispositionType;

        private ContentDispositionHeaderValue()
        {
            // Used by the parser to create a new instance of this type.
        }

        public ContentDispositionHeaderValue(StringSegment dispositionType)
        {
            CheckDispositionTypeFormat(dispositionType, "dispositionType");
            _dispositionType = dispositionType;
        }

        public StringSegment DispositionType
        {
            get { return _dispositionType; }
            set
            {
                CheckDispositionTypeFormat(value, "value");
                _dispositionType = value;
            }
        }

        public IList<NameValueHeaderValue> Parameters
        {
            get
            {
                if (_parameters == null)
                {
                    _parameters = new ObjectCollection<NameValueHeaderValue>();
                }
                return _parameters;
            }
        }

        // Helpers to access specific parameters in the list

        public StringSegment Name
        {
            get { return GetName(NameString); }
            set { SetName(NameString, value); }
        }


        public StringSegment FileName
        {
            get { return GetName(FileNameString); }
            set { SetName(FileNameString, value); }
        }

        public StringSegment FileNameStar
        {
            get { return GetName(FileNameStarString); }
            set { SetName(FileNameStarString, value); }
        }

        public DateTimeOffset? CreationDate
        {
            get { return GetDate(CreationDateString); }
            set { SetDate(CreationDateString, value); }
        }

        public DateTimeOffset? ModificationDate
        {
            get { return GetDate(ModificationDateString); }
            set { SetDate(ModificationDateString, value); }
        }

        public DateTimeOffset? ReadDate
        {
            get { return GetDate(ReadDateString); }
            set { SetDate(ReadDateString, value); }
        }

        public long? Size
        {
            get
            {
                var sizeParameter = NameValueHeaderValue.Find(_parameters, SizeString);
                long value;
                if (sizeParameter != null)
                {
                    var sizeString = sizeParameter.Value;
                    if (HeaderUtilities.TryParseNonNegativeInt64(sizeString, out value))
                    {
                        return value;
                    }
                }
                return null;
            }
            set
            {
                var sizeParameter = NameValueHeaderValue.Find(_parameters, SizeString);
                if (value == null)
                {
                    // Remove parameter
                    if (sizeParameter != null)
                    {
                        _parameters.Remove(sizeParameter);
                    }
                }
                else if (value < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }
                else if (sizeParameter != null)
                {
                    sizeParameter.Value = value.Value.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    string sizeString = value.Value.ToString(CultureInfo.InvariantCulture);
                    _parameters.Add(new NameValueHeaderValue(SizeString, sizeString));
                }
            }
        }

        /// <summary>
        /// Sets both FileName and FileNameStar using encodings appropriate for HTTP headers.
        /// </summary>
        /// <param name="fileName"></param>
        public void SetHttpFileName(StringSegment fileName)
        {
            if (!StringSegment.IsNullOrEmpty(fileName))
            {
                FileName = Sanatize(fileName);
            }
            else
            {
                FileName = fileName;
            }
            FileNameStar = fileName;
        }

        /// <summary>
        /// Sets the FileName parameter using encodings appropriate for MIME headers.
        /// The FileNameStar paraemter is removed.
        /// </summary>
        /// <param name="fileName"></param>
        public void SetMimeFileName(StringSegment fileName)
        {
            FileNameStar = null;
            FileName = fileName;
        }

        public override string ToString()
        {
            return _dispositionType + NameValueHeaderValue.ToString(_parameters, ';', true);
        }

        public override bool Equals(object obj)
        {
            var other = obj as ContentDispositionHeaderValue;

            if (other == null)
            {
                return false;
            }

            return _dispositionType.Equals(other._dispositionType, StringComparison.OrdinalIgnoreCase) &&
                HeaderUtilities.AreEqualCollections(_parameters, other._parameters);
        }

        public override int GetHashCode()
        {
            // The dispositionType string is case-insensitive.
            return StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(_dispositionType) ^ NameValueHeaderValue.GetHashCode(_parameters);
        }

        public static ContentDispositionHeaderValue Parse(StringSegment input)
        {
            var index = 0;
            return Parser.ParseValue(input, ref index);
        }

        public static bool TryParse(StringSegment input, out ContentDispositionHeaderValue parsedValue)
        {
            var index = 0;
            return Parser.TryParseValue(input, ref index, out parsedValue);
        }

        private static int GetDispositionTypeLength(StringSegment input, int startIndex, out ContentDispositionHeaderValue parsedValue)
        {
            Contract.Requires(startIndex >= 0);

            parsedValue = null;

            if (StringSegment.IsNullOrEmpty(input) || (startIndex >= input.Length))
            {
                return 0;
            }

            // Caller must remove leading whitespaces. If not, we'll return 0.
            var dispositionTypeLength = GetDispositionTypeExpressionLength(input, startIndex, out var dispositionType);

            if (dispositionTypeLength == 0)
            {
                return 0;
            }

            var current = startIndex + dispositionTypeLength;
            current = current + HttpRuleParser.GetWhitespaceLength(input, current);
            var contentDispositionHeader = new ContentDispositionHeaderValue();
            contentDispositionHeader._dispositionType = dispositionType;

            // If we're not done and we have a parameter delimiter, then we have a list of parameters.
            if ((current < input.Length) && (input[current] == ';'))
            {
                current++; // skip delimiter.
                int parameterLength = NameValueHeaderValue.GetNameValueListLength(input, current, ';',
                    contentDispositionHeader.Parameters);

                parsedValue = contentDispositionHeader;
                return current + parameterLength - startIndex;
            }

            // We have a ContentDisposition header without parameters.
            parsedValue = contentDispositionHeader;
            return current - startIndex;
        }

        private static int GetDispositionTypeExpressionLength(StringSegment input, int startIndex, out StringSegment dispositionType)
        {
            Contract.Requires((input != null) && (input.Length > 0) && (startIndex < input.Length));

            // This method just parses the disposition type string, it does not parse parameters.
            dispositionType = null;

            // Parse the disposition type, i.e. <dispositiontype> in content-disposition string
            // "<dispositiontype>; param1=value1; param2=value2"
            var typeLength = HttpRuleParser.GetTokenLength(input, startIndex);

            if (typeLength == 0)
            {
                return 0;
            }

            dispositionType = input.Subsegment(startIndex, typeLength);
            return typeLength;
        }

        private static void CheckDispositionTypeFormat(StringSegment dispositionType, string parameterName)
        {
            if (StringSegment.IsNullOrEmpty(dispositionType))
            {
                throw new ArgumentException("An empty string is not allowed.", parameterName);
            }

            // When adding values using strongly typed objects, no leading/trailing LWS (whitespaces) are allowed.
            var dispositionTypeLength = GetDispositionTypeExpressionLength(dispositionType, 0, out var tempDispositionType);
            if ((dispositionTypeLength == 0) || (tempDispositionType.Length != dispositionType.Length))
            {
                throw new FormatException(string.Format(CultureInfo.InvariantCulture,
                    "Invalid disposition type '{0}'.", dispositionType));
            }
        }

        // Gets a parameter of the given name and attempts to extract a date.
        // Returns null if the parameter is not present or the format is incorrect.
        private DateTimeOffset? GetDate(string parameter)
        {
            var dateParameter = NameValueHeaderValue.Find(_parameters, parameter);
            if (dateParameter != null)
            {
                var dateString = dateParameter.Value;
                // Should have quotes, remove them.
                if (IsQuoted(dateString))
                {
                    dateString = dateString.Subsegment(1, dateString.Length - 2);
                }
                DateTimeOffset date;
                if (HttpRuleParser.TryStringToDate(dateString, out date))
                {
                    return date;
                }
            }
            return null;
        }

        // Add the given parameter to the list. Remove if date is null.
        private void SetDate(string parameter, DateTimeOffset? date)
        {
            var dateParameter = NameValueHeaderValue.Find(_parameters, parameter);
            if (date == null)
            {
                // Remove parameter
                if (dateParameter != null)
                {
                    _parameters.Remove(dateParameter);
                }
            }
            else
            {
                // Must always be quoted
                var dateString = HeaderUtilities.FormatDate(date.Value, quoted: true);
                if (dateParameter != null)
                {
                    dateParameter.Value = dateString;
                }
                else
                {
                    Parameters.Add(new NameValueHeaderValue(parameter, dateString));
                }
            }
        }

        // Gets a parameter of the given name and attempts to decode it if necessary.
        // Returns null if the parameter is not present or the raw value if the encoding is incorrect.
        private StringSegment GetName(string parameter)
        {
            var nameParameter = NameValueHeaderValue.Find(_parameters, parameter);
            if (nameParameter != null)
            {
                string result;
                // filename*=utf-8'lang'%7FMyString
                if (parameter.EndsWith("*", StringComparison.Ordinal))
                {
                    if (TryDecode5987(nameParameter.Value, out result))
                    {
                        return result;
                    }
                    return null; // Unrecognized encoding
                }

                // filename="=?utf-8?B?BDFSDFasdfasdc==?="
                if (TryDecodeMime(nameParameter.Value, out result))
                {
                    return result;
                }
                // May not have been encoded
                return HeaderUtilities.RemoveQuotes(nameParameter.Value);
            }
            return null;
        }

        // Add/update the given parameter in the list, encoding if necessary.
        // Remove if value is null/Empty
        private void SetName(StringSegment parameter, StringSegment value)
        {
            var nameParameter = NameValueHeaderValue.Find(_parameters, parameter);
            if (StringSegment.IsNullOrEmpty(value))
            {
                // Remove parameter
                if (nameParameter != null)
                {
                    _parameters.Remove(nameParameter);
                }
            }
            else
            {
                var processedValue = StringSegment.Empty;
                if (parameter.EndsWith("*", StringComparison.Ordinal))
                {
                    processedValue = Encode5987(value);
                }
                else
                {
                    processedValue = EncodeAndQuoteMime(value);
                }

                if (nameParameter != null)
                {
                    nameParameter.Value = processedValue;
                }
                else
                {
                    Parameters.Add(new NameValueHeaderValue(parameter, processedValue));
                }
            }
        }

        // Returns input for decoding failures, as the content might not be encoded
        private StringSegment EncodeAndQuoteMime(StringSegment input)
        {
            var result = input;
            var needsQuotes = false;
            // Remove bounding quotes, they'll get re-added later
            if (IsQuoted(result))
            {
                result = result.Subsegment(1, result.Length - 2);
                needsQuotes = true;
            }

            if (RequiresEncoding(result))
            {
                needsQuotes = true; // Encoded data must always be quoted, the equals signs are invalid in tokens
                result = EncodeMime(result); // =?utf-8?B?asdfasdfaesdf?=
            }
            else if (!needsQuotes && HttpRuleParser.GetTokenLength(result, 0) != result.Length)
            {
                needsQuotes = true;
            }

            if (needsQuotes)
            {
                // '\' and '"' must be escaped in a quoted string
                result = result.ToString().Replace(@"\", @"\\").Replace(@"""", @"\""");
                // Re-add quotes "value"
                result = string.Format(CultureInfo.InvariantCulture, "\"{0}\"", result);
            }
            return result;
        }

        // Replaces characters not suitable for HTTP headers with '_' rather than MIME encoding them.
        private StringSegment Sanatize(StringSegment input)
        {
            var result = input;

            if (RequiresEncoding(result))
            {
                var builder = new StringBuilder(result.Length);
                for (int i = 0; i < result.Length; i++)
                {
                    var c = result[i];
                    if ((int)c > 0x7f)
                    {
                        c = '_'; // Replace out-of-range characters
                    }
                    builder.Append(c);
                }
                result = builder.ToString();
            }

            return result;
        }

        // Returns true if the value starts and ends with a quote
        private bool IsQuoted(StringSegment value)
        {
            Contract.Assert(value != null);

            return value.Length > 1 && value.StartsWith("\"", StringComparison.Ordinal)
                && value.EndsWith("\"", StringComparison.Ordinal);
        }

        // tspecials are required to be in a quoted string.  Only non-ascii needs to be encoded.
        private bool RequiresEncoding(StringSegment input)
        {
            Contract.Assert(input != null);

            for (int i = 0; i < input.Length; i++)
            {
                if ((int)input[i] > 0x7f)
                {
                    return true;
                }
            }
            return false;
        }

        // Encode using MIME encoding
        private unsafe string EncodeMime(StringSegment input)
        {
            fixed (char* chars = input.Buffer)
            {
                var byteCount = Encoding.UTF8.GetByteCount(chars + input.Offset, input.Length);
                var buffer = new byte[byteCount];
                fixed (byte* bytes = buffer)
                {
                    Encoding.UTF8.GetBytes(chars + input.Offset, input.Length, bytes, byteCount);
                }
                var encodedName = Convert.ToBase64String(buffer);
                return "=?utf-8?B?" + encodedName + "?=";
            }
        }

        // Attempt to decode MIME encoded strings
        private bool TryDecodeMime(StringSegment input, out string output)
        {
            Contract.Assert(input != null);

            output = null;
            var processedInput = input;
            // Require quotes, min of "=?e?b??="
            if (!IsQuoted(processedInput) || processedInput.Length < 10)
            {
                return false;
            }

            var parts = processedInput.Split(QuestionMark).ToArray();
            // "=, encodingName, encodingType, encodedData, ="
            if (parts.Length != 5 || parts[0] != "\"=" || parts[4] != "=\""
                || !parts[2].Equals("b", StringComparison.OrdinalIgnoreCase))
            {
                // Not encoded.
                // This does not support multi-line encoding.
                // Only base64 encoding is supported, not quoted printable
                return false;
            }

            try
            {
                var encoding = Encoding.GetEncoding(parts[1].ToString());
                var bytes = Convert.FromBase64String(parts[3].ToString());
                output = encoding.GetString(bytes, 0, bytes.Length);
                return true;
            }
            catch (ArgumentException)
            {
                // Unknown encoding or bad characters
            }
            catch (FormatException)
            {
                // Bad base64 decoding
            }
            return false;
        }

        // Encode a string using RFC 5987 encoding
        // encoding'lang'PercentEncodedSpecials
        private string Encode5987(StringSegment input)
        {
            var builder = new StringBuilder("UTF-8\'\'");
            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                // attr-char = ALPHA / DIGIT / "!" / "#" / "$" / "&" / "+" / "-" / "." / "^" / "_" / "`" / "|" / "~"
                //      ; token except ( "*" / "'" / "%" )
                if (c > 0x7F) // Encodes as multiple utf-8 bytes
                {
                    var bytes = Encoding.UTF8.GetBytes(c.ToString());
                    foreach (byte b in bytes)
                    {
                        HexEscape(builder, (char)b);
                    }
                }
                else if (!HttpRuleParser.IsTokenChar(c) || c == '*' || c == '\'' || c == '%')
                {
                    // ASCII - Only one encoded byte
                    HexEscape(builder, c);
                }
                else
                {
                    builder.Append(c);
                }
            }
            return builder.ToString();
        }

        private static readonly char[] HexUpperChars = {
                                   '0', '1', '2', '3', '4', '5', '6', '7',
                                   '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        private static void HexEscape(StringBuilder builder, char c)
        {
            builder.Append('%');
            builder.Append(HexUpperChars[(c & 0xf0) >> 4]);
            builder.Append(HexUpperChars[c & 0xf]);
        }

        // Attempt to decode using RFC 5987 encoding.
        // encoding'language'my%20string
        private bool TryDecode5987(StringSegment input, out string output)
        {
            output = null;

            var parts = input.Split(SingleQuote).ToArray();
            if (parts.Length != 3)
            {
                return false;
            }

            var decoded = new StringBuilder();
            byte[] unescapedBytes = null;
            try
            {
                var encoding = Encoding.GetEncoding(parts[0].ToString());

                var dataString = parts[2];
                unescapedBytes = ArrayPool<byte>.Shared.Rent(dataString.Length);
                var unescapedBytesCount = 0;
                for (var index = 0; index < dataString.Length; index++)
                {
                    if (IsHexEncoding(dataString, index)) // %FF
                    {
                        // Unescape and cache bytes, multi-byte characters must be decoded all at once
                        unescapedBytes[unescapedBytesCount++] = HexUnescape(dataString, ref index);
                        index--; // HexUnescape did +=3; Offset the for loop's ++
                    }
                    else
                    {
                        if (unescapedBytesCount > 0)
                        {
                            // Decode any previously cached bytes
                            decoded.Append(encoding.GetString(unescapedBytes, 0, unescapedBytesCount));
                            unescapedBytesCount = 0;
                        }
                        decoded.Append(dataString[index]); // Normal safe character
                    }
                }

                if (unescapedBytesCount > 0)
                {
                    // Decode any previously cached bytes
                    decoded.Append(encoding.GetString(unescapedBytes, 0, unescapedBytesCount));
                }
            }
            catch (ArgumentException)
            {
                return false; // Unknown encoding or bad characters
            }
            finally
            {
                if (unescapedBytes != null)
                {
                    ArrayPool<byte>.Shared.Return(unescapedBytes);
                }
            }

            output = decoded.ToString();
            return true;
        }

        private static bool IsHexEncoding(StringSegment pattern, int index)
        {
            if ((pattern.Length - index) < 3)
            {
                return false;
            }
            if ((pattern[index] == '%') && IsEscapedAscii(pattern[index + 1], pattern[index + 2]))
            {
                return true;
            }
            return false;
        }

        private static bool IsEscapedAscii(char digit, char next)
        {
            if (!(((digit >= '0') && (digit <= '9'))
                || ((digit >= 'A') && (digit <= 'F'))
                || ((digit >= 'a') && (digit <= 'f'))))
            {
                return false;
            }

            if (!(((next >= '0') && (next <= '9'))
                || ((next >= 'A') && (next <= 'F'))
                || ((next >= 'a') && (next <= 'f'))))
            {
                return false;
            }

            return true;
        }

        private static byte HexUnescape(StringSegment pattern, ref int index)
        {
            if ((index < 0) || (index >= pattern.Length))
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if ((pattern[index] == '%')
                && (pattern.Length - index >= 3))
            {
                var ret = UnEscapeAscii(pattern[index + 1], pattern[index + 2]);
                index += 3;
                return ret;
            }
            return (byte)pattern[index++];
        }

        internal static byte UnEscapeAscii(char digit, char next)
        {
            if (!(((digit >= '0') && (digit <= '9'))
                || ((digit >= 'A') && (digit <= 'F'))
                || ((digit >= 'a') && (digit <= 'f'))))
            {
                throw new ArgumentException();
            }

            var res = (digit <= '9')
                ? ((int)digit - (int)'0')
                : (((digit <= 'F')
                ? ((int)digit - (int)'A')
                : ((int)digit - (int)'a'))
                   + 10);

            if (!(((next >= '0') && (next <= '9'))
                || ((next >= 'A') && (next <= 'F'))
                || ((next >= 'a') && (next <= 'f'))))
            {
                throw new ArgumentException();
            }

            return (byte)((res << 4) + ((next <= '9')
                    ? ((int)next - (int)'0')
                    : (((next <= 'F')
                        ? ((int)next - (int)'A')
                        : ((int)next - (int)'a'))
                       + 10)));
        }
    }
}