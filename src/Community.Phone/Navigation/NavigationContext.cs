// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides navigation parameters and query-string values to a
    /// <see cref="PhoneApplicationPage"/>.
    /// </summary>
    /// <remarks>
    /// Windows Phone exposes this type from the platform as
    /// <c>System.Windows.Navigation.NavigationContext</c>. WPF has no
    /// equivalent type, so the port exposes the same shape in the toolkit
    /// namespace.
    /// </remarks>
    public sealed class NavigationContext
    {
        private readonly Dictionary<string, string> _queryString;
        private readonly Uri _uri;
        private readonly object _extraData;

        internal NavigationContext(Uri uri, IDictionary<string, string> queryString, object extraData)
        {
            _uri = uri;
            _extraData = extraData;
            _queryString = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (queryString != null)
            {
                foreach (KeyValuePair<string, string> pair in queryString)
                {
                    _queryString[pair.Key] = pair.Value;
                }
            }
        }

        /// <summary>
        /// Gets an empty navigation context.
        /// </summary>
        public static NavigationContext Empty
        {
            get { return new NavigationContext(null, null, null); }
        }

        /// <summary>
        /// Gets the URI used for the navigation, if any.
        /// </summary>
        public Uri Uri
        {
            get { return _uri; }
        }

        /// <summary>
        /// Gets the additional data supplied to the navigation call.
        /// </summary>
        public object ExtraData
        {
            get { return _extraData; }
        }

        /// <summary>
        /// Gets the query-string values for the navigation.
        /// </summary>
        public IDictionary<string, string> QueryString
        {
            get { return _queryString; }
        }

        internal static NavigationContext FromNavigationEventArgs(Uri uri, object extraData)
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            string queryString = GetQueryString(uri);
            if (!string.IsNullOrEmpty(queryString))
            {
                string query = queryString.TrimStart('?');
                foreach (string part in query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    int separator = part.IndexOf('=');
                    string key = separator < 0 ? part : part.Substring(0, separator);
                    string value = separator < 0 ? string.Empty : part.Substring(separator + 1);
                    if (key.Length > 0)
                    {
                        values[Decode(key)] = Decode(value);
                    }
                }
            }

            IDictionary<string, string> extraValues = extraData as IDictionary<string, string>;
            if (extraValues != null)
            {
                foreach (KeyValuePair<string, string> pair in extraValues)
                {
                    values[pair.Key] = pair.Value;
                }
            }

            return new NavigationContext(uri, values, extraData);
        }

        private static string GetQueryString(Uri uri)
        {
            if (uri == null)
            {
                return null;
            }

            if (uri.IsAbsoluteUri)
            {
                return uri.Query;
            }

            // WPF hands a relative navigation URI through unchanged. Uri.Query
            // throws for a relative Uri, so parse the original string instead.
            string text = uri.OriginalString;
            int separator = text.IndexOf('?');
            if (separator < 0)
            {
                return null;
            }

            int fragment = text.IndexOf('#', separator);
            return fragment < 0
                ? text.Substring(separator)
                : text.Substring(separator, fragment - separator);
        }

        private static string Decode(string value)
        {
            return Uri.UnescapeDataString(value.Replace("+", " "));
        }
    }
}
