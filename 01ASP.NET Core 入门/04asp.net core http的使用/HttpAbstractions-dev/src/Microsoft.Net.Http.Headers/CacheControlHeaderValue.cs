// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Primitives;

namespace Microsoft.Net.Http.Headers
{
    public class CacheControlHeaderValue
    {
        public static readonly string PublicString = "public";
        public static readonly string PrivateString = "private";
        public static readonly string MaxAgeString = "max-age";
        public static readonly string SharedMaxAgeString = "s-maxage";
        public static readonly string NoCacheString = "no-cache";
        public static readonly string NoStoreString = "no-store";
        public static readonly string MaxStaleString = "max-stale";
        public static readonly string MinFreshString = "min-fresh";
        public static readonly string NoTransformString = "no-transform";
        public static readonly string OnlyIfCachedString = "only-if-cached";
        public static readonly string MustRevalidateString = "must-revalidate";
        public static readonly string ProxyRevalidateString = "proxy-revalidate";

        // The Cache-Control header is special: It is a header supporting a list of values, but we represent the list
        // as _one_ instance of CacheControlHeaderValue. I.e we set 'SupportsMultipleValues' to 'true' since it is
        // OK to have multiple Cache-Control headers in a request/response message. However, after parsing all
        // Cache-Control headers, only one instance of CacheControlHeaderValue is created (if all headers contain valid
        // values, otherwise we may have multiple strings containing the invalid values).
        private static readonly HttpHeaderParser<CacheControlHeaderValue> Parser
            = new GenericHeaderParser<CacheControlHeaderValue>(true, GetCacheControlLength);

        private static readonly Action<StringSegment> CheckIsValidTokenAction = CheckIsValidToken;

        private bool _noCache;
        private ICollection<StringSegment> _noCacheHeaders;
        private bool _noStore;
        private TimeSpan? _maxAge;
        private TimeSpan? _sharedMaxAge;
        private bool _maxStale;
        private TimeSpan? _maxStaleLimit;
        private TimeSpan? _minFresh;
        private bool _noTransform;
        private bool _onlyIfCached;
        private bool _public;
        private bool _private;
        private ICollection<StringSegment> _privateHeaders;
        private bool _mustRevalidate;
        private bool _proxyRevalidate;
        private IList<NameValueHeaderValue> _extensions;

        public CacheControlHeaderValue()
        {
            // This type is unique in that there is no single required parameter.
        }

        public bool NoCache
        {
            get { return _noCache; }
            set { _noCache = value; }
        }

        public ICollection<StringSegment> NoCacheHeaders
        {
            get
            {
                if (_noCacheHeaders == null)
                {
                    _noCacheHeaders = new ObjectCollection<StringSegment>(CheckIsValidTokenAction);
                }
                return _noCacheHeaders;
            }
        }

        public bool NoStore
        {
            get { return _noStore; }
            set { _noStore = value; }
        }

        public TimeSpan? MaxAge
        {
            get { return _maxAge; }
            set { _maxAge = value; }
        }

        public TimeSpan? SharedMaxAge
        {
            get { return _sharedMaxAge; }
            set { _sharedMaxAge = value; }
        }

        public bool MaxStale
        {
            get { return _maxStale; }
            set { _maxStale = value; }
        }

        public TimeSpan? MaxStaleLimit
        {
            get { return _maxStaleLimit; }
            set { _maxStaleLimit = value; }
        }

        public TimeSpan? MinFresh
        {
            get { return _minFresh; }
            set { _minFresh = value; }
        }

        public bool NoTransform
        {
            get { return _noTransform; }
            set { _noTransform = value; }
        }

        public bool OnlyIfCached
        {
            get { return _onlyIfCached; }
            set { _onlyIfCached = value; }
        }

        public bool Public
        {
            get { return _public; }
            set { _public = value; }
        }

        public bool Private
        {
            get { return _private; }
            set { _private = value; }
        }

        public ICollection<StringSegment> PrivateHeaders
        {
            get
            {
                if (_privateHeaders == null)
                {
                    _privateHeaders = new ObjectCollection<StringSegment>(CheckIsValidTokenAction);
                }
                return _privateHeaders;
            }
        }

        public bool MustRevalidate
        {
            get { return _mustRevalidate; }
            set { _mustRevalidate = value; }
        }

        public bool ProxyRevalidate
        {
            get { return _proxyRevalidate; }
            set { _proxyRevalidate = value; }
        }

        public IList<NameValueHeaderValue> Extensions
        {
            get
            {
                if (_extensions == null)
                {
                    _extensions = new ObjectCollection<NameValueHeaderValue>();
                }
                return _extensions;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            AppendValueIfRequired(sb, _noStore, NoStoreString);
            AppendValueIfRequired(sb, _noTransform, NoTransformString);
            AppendValueIfRequired(sb, _onlyIfCached, OnlyIfCachedString);
            AppendValueIfRequired(sb, _public, PublicString);
            AppendValueIfRequired(sb, _mustRevalidate, MustRevalidateString);
            AppendValueIfRequired(sb, _proxyRevalidate, ProxyRevalidateString);

            if (_noCache)
            {
                AppendValueWithSeparatorIfRequired(sb, NoCacheString);
                if ((_noCacheHeaders != null) && (_noCacheHeaders.Count > 0))
                {
                    sb.Append("=\"");
                    AppendValues(sb, _noCacheHeaders);
                    sb.Append('\"');
                }
            }

            if (_maxAge.HasValue)
            {
                AppendValueWithSeparatorIfRequired(sb, MaxAgeString);
                sb.Append('=');
                sb.Append(((int)_maxAge.Value.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
            }

            if (_sharedMaxAge.HasValue)
            {
                AppendValueWithSeparatorIfRequired(sb, SharedMaxAgeString);
                sb.Append('=');
                sb.Append(((int)_sharedMaxAge.Value.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
            }

            if (_maxStale)
            {
                AppendValueWithSeparatorIfRequired(sb, MaxStaleString);
                if (_maxStaleLimit.HasValue)
                {
                    sb.Append('=');
                    sb.Append(((int)_maxStaleLimit.Value.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
                }
            }

            if (_minFresh.HasValue)
            {
                AppendValueWithSeparatorIfRequired(sb, MinFreshString);
                sb.Append('=');
                sb.Append(((int)_minFresh.Value.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo));
            }

            if (_private)
            {
                AppendValueWithSeparatorIfRequired(sb, PrivateString);
                if ((_privateHeaders != null) && (_privateHeaders.Count > 0))
                {
                    sb.Append("=\"");
                    AppendValues(sb, _privateHeaders);
                    sb.Append('\"');
                }
            }

            NameValueHeaderValue.ToString(_extensions, ',', false, sb);

            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            var other = obj as CacheControlHeaderValue;

            if (other == null)
            {
                return false;
            }

            if ((_noCache != other._noCache) || (_noStore != other._noStore) || (_maxAge != other._maxAge) ||
                (_sharedMaxAge != other._sharedMaxAge) || (_maxStale != other._maxStale) ||
                (_maxStaleLimit != other._maxStaleLimit) || (_minFresh != other._minFresh) ||
                (_noTransform != other._noTransform) || (_onlyIfCached != other._onlyIfCached) ||
                (_public != other._public) || (_private != other._private) ||
                (_mustRevalidate != other._mustRevalidate) || (_proxyRevalidate != other._proxyRevalidate))
            {
                return false;
            }

            if (!HeaderUtilities.AreEqualCollections(_noCacheHeaders, other._noCacheHeaders,
                StringSegmentComparer.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!HeaderUtilities.AreEqualCollections(_privateHeaders, other._privateHeaders,
                StringSegmentComparer.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!HeaderUtilities.AreEqualCollections(_extensions, other._extensions))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            // Use a different bit for bool fields: bool.GetHashCode() will return 0 (false) or 1 (true). So we would
            // end up having the same hash code for e.g. two instances where one has only noCache set and the other
            // only noStore.
            int result = _noCache.GetHashCode() ^ (_noStore.GetHashCode() << 1) ^ (_maxStale.GetHashCode() << 2) ^
                (_noTransform.GetHashCode() << 3) ^ (_onlyIfCached.GetHashCode() << 4) ^
                (_public.GetHashCode() << 5) ^ (_private.GetHashCode() << 6) ^
                (_mustRevalidate.GetHashCode() << 7) ^ (_proxyRevalidate.GetHashCode() << 8);

            // XOR the hashcode of timespan values with different numbers to make sure two instances with the same
            // timespan set on different fields result in different hashcodes.
            result = result ^ (_maxAge.HasValue ? _maxAge.Value.GetHashCode() ^ 1 : 0) ^
                (_sharedMaxAge.HasValue ? _sharedMaxAge.Value.GetHashCode() ^ 2 : 0) ^
                (_maxStaleLimit.HasValue ? _maxStaleLimit.Value.GetHashCode() ^ 4 : 0) ^
                (_minFresh.HasValue ? _minFresh.Value.GetHashCode() ^ 8 : 0);

            if ((_noCacheHeaders != null) && (_noCacheHeaders.Count > 0))
            {
                foreach (var noCacheHeader in _noCacheHeaders)
                {
                    result = result ^ StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(noCacheHeader);
                }
            }

            if ((_privateHeaders != null) && (_privateHeaders.Count > 0))
            {
                foreach (var privateHeader in _privateHeaders)
                {
                    result = result ^ StringSegmentComparer.OrdinalIgnoreCase.GetHashCode(privateHeader);
                }
            }

            if ((_extensions != null) && (_extensions.Count > 0))
            {
                foreach (var extension in _extensions)
                {
                    result = result ^ extension.GetHashCode();
                }
            }

            return result;
        }

        public static CacheControlHeaderValue Parse(StringSegment input)
        {
            int index = 0;
            // Cache-Control is unusual because there are no required values so the parser will succeed for an empty string, but still return null.
            var result = Parser.ParseValue(input, ref index);
            if (result == null)
            {
                throw new FormatException("No cache directives found.");
            }
            return result;
        }

        public static bool TryParse(StringSegment input, out CacheControlHeaderValue parsedValue)
        {
            int index = 0;
            // Cache-Control is unusual because there are no required values so the parser will succeed for an empty string, but still return null.
            if (Parser.TryParseValue(input, ref index, out parsedValue) && parsedValue != null)
            {
                return true;
            }
            parsedValue = null;
            return false;
        }

        private static int GetCacheControlLength(StringSegment input, int startIndex, out CacheControlHeaderValue parsedValue)
        {
            Contract.Requires(startIndex >= 0);

            parsedValue = null;

            if (StringSegment.IsNullOrEmpty(input) || (startIndex >= input.Length))
            {
                return 0;
            }

            // Cache-Control header consists of a list of name/value pairs, where the value is optional. So use an
            // instance of NameValueHeaderParser to parse the string.
            var current = startIndex;
            NameValueHeaderValue nameValue = null;
            var nameValueList = new List<NameValueHeaderValue>();
            while (current < input.Length)
            {
                if (!NameValueHeaderValue.MultipleValueParser.TryParseValue(input, ref current, out nameValue))
                {
                    return 0;
                }

                nameValueList.Add(nameValue);
            }

            // If we get here, we were able to successfully parse the string as list of name/value pairs. Now analyze
            // the name/value pairs.

            // Cache-Control is a header supporting lists of values. However, expose the header as an instance of
            // CacheControlHeaderValue.
            var result = new CacheControlHeaderValue();

            if (!TrySetCacheControlValues(result, nameValueList))
            {
                return 0;
            }

            parsedValue = result;

            // If we get here we successfully parsed the whole string.
            return input.Length - startIndex;
        }

        private static bool TrySetCacheControlValues(
            CacheControlHeaderValue cc,
            List<NameValueHeaderValue> nameValueList)
        {
            for (var i = 0; i < nameValueList.Count; i++)
            {
                var nameValue = nameValueList[i];
                var name = nameValue.Name;
                var success = true;

                switch (name.Length)
                {
                    case 6:
                        if (StringSegment.Equals(PublicString, name, StringComparison.OrdinalIgnoreCase))
                        {
                            success = TrySetTokenOnlyValue(nameValue, ref cc._public);
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    case 7:
                        if (StringSegment.Equals(MaxAgeString, name, StringComparison.OrdinalIgnoreCase))
                        {
                            success = TrySetTimeSpan(nameValue, ref cc._maxAge);
                        }
                        else if(StringSegment.Equals(PrivateString, name, StringComparison.OrdinalIgnoreCase))
                        {
                            success = TrySetOptionalTokenList(nameValue, ref cc._private, ref cc._privateHeaders);
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    case 8:
                        if (StringSegment.Equals(NoCacheString, name, StringComparison.OrdinalIgnoreCase))
                        {
                            success = TrySetOptionalTokenList(nameValue, ref cc._noCache, ref cc._noCacheHeaders);
                        }
                        else if (StringSegment.Equals(NoStoreString, name, StringComparison.OrdinalIgnoreCase))
                        {
                            success = TrySetTokenOnlyValue(nameValue, ref cc._noStore);
                        }
                        else if (StringSegment.Equals(SharedMaxAgeString, name, StringComparison.OrdinalIgnoreCase))
                        {
                            success = TrySetTimeSpan(nameValue, ref cc._sharedMaxAge);
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    case 9:
                        if (StringSegment.Equals(MaxStaleString, name, StringComparison.OrdinalIgnoreCase))
                        {
                            success = ((nameValue.Value == null) || TrySetTimeSpan(nameValue, ref cc._maxStaleLimit));
                            if (success)
                            {
                                cc._maxStale = true;
                            }
                        }
                        else if (StringSegment.Equals(MinFreshString, name, StringComparison.OrdinalIgnoreCase))
                        {
                            success = TrySetTimeSpan(nameValue, ref cc._minFresh);
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    case 12:
                        if (StringSegment.Equals(NoTransformString, name, StringComparison.OrdinalIgnoreCase))
                        {
                            success = TrySetTokenOnlyValue(nameValue, ref cc._noTransform);
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    case 14:
                        if (StringSegment.Equals(OnlyIfCachedString, name, StringComparison.OrdinalIgnoreCase))
                        {
                            success = TrySetTokenOnlyValue(nameValue, ref cc._onlyIfCached);
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    case 15:
                        if (StringSegment.Equals(MustRevalidateString, name, StringComparison.OrdinalIgnoreCase))
                        {
                            success = TrySetTokenOnlyValue(nameValue, ref cc._mustRevalidate);
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    case 16:
                        if (StringSegment.Equals(ProxyRevalidateString, name, StringComparison.OrdinalIgnoreCase))
                        {
                            success = TrySetTokenOnlyValue(nameValue, ref cc._proxyRevalidate);
                        }
                        else
                        {
                            goto default;
                        }
                        break;

                    default:
                        cc.Extensions.Add(nameValue); // success is always true
                        break;
                }

                if (!success)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TrySetTokenOnlyValue(NameValueHeaderValue nameValue, ref bool boolField)
        {
            if (nameValue.Value != null)
            {
                return false;
            }

            boolField = true;
            return true;
        }

        private static bool TrySetOptionalTokenList(
            NameValueHeaderValue nameValue,
            ref bool boolField,
            ref ICollection<StringSegment> destination)
        {
            Contract.Requires(nameValue != null);

            if (nameValue.Value == null)
            {
                boolField = true;
                return true;
            }

            // We need the string to be at least 3 chars long: 2x quotes and at least 1 character. Also make sure we
            // have a quoted string. Note that NameValueHeaderValue will never have leading/trailing whitespaces.
            var valueString = nameValue.Value;
            if ((valueString.Length < 3) || (valueString[0] != '\"') || (valueString[valueString.Length - 1] != '\"'))
            {
                return false;
            }

            // We have a quoted string. Now verify that the string contains a list of valid tokens separated by ','.
            var current = 1; // skip the initial '"' character.
            var maxLength = valueString.Length - 1; // -1 because we don't want to parse the final '"'.
            var separatorFound = false;
            var originalValueCount = destination == null ? 0 : destination.Count;
            while (current < maxLength)
            {
                current = HeaderUtilities.GetNextNonEmptyOrWhitespaceIndex(valueString, current, true,
                    out separatorFound);

                if (current == maxLength)
                {
                    break;
                }

                var tokenLength = HttpRuleParser.GetTokenLength(valueString, current);

                if (tokenLength == 0)
                {
                    // We already skipped whitespaces and separators. If we don't have a token it must be an invalid
                    // character.
                    return false;
                }

                if (destination == null)
                {
                    destination = new ObjectCollection<StringSegment>(CheckIsValidTokenAction);
                }

                destination.Add(valueString.Subsegment(current, tokenLength));

                current = current + tokenLength;
            }

            // After parsing a valid token list, we expect to have at least one value
            if ((destination != null) && (destination.Count > originalValueCount))
            {
                boolField = true;
                return true;
            }

            return false;
        }

        private static bool TrySetTimeSpan(NameValueHeaderValue nameValue, ref TimeSpan? timeSpan)
        {
            Contract.Requires(nameValue != null);

            if (nameValue.Value == null)
            {
                return false;
            }

            int seconds;
            if (!HeaderUtilities.TryParseNonNegativeInt32(nameValue.Value, out seconds))
            {
                return false;
            }

            timeSpan = new TimeSpan(0, 0, seconds);

            return true;
        }

        private static void AppendValueIfRequired(StringBuilder sb, bool appendValue, string value)
        {
            if (appendValue)
            {
                AppendValueWithSeparatorIfRequired(sb, value);
            }
        }

        private static void AppendValueWithSeparatorIfRequired(StringBuilder sb, string value)
        {
            if (sb.Length > 0)
            {
                sb.Append(", ");
            }
            sb.Append(value);
        }

        private static void AppendValues(StringBuilder sb, IEnumerable<StringSegment> values)
        {
            var first = true;
            foreach (var value in values)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    sb.Append(", ");
                }

                sb.Append(value);
            }
        }

        private static void CheckIsValidToken(StringSegment item)
        {
            HeaderUtilities.CheckValidToken(item, nameof(item));
        }
    }
}
