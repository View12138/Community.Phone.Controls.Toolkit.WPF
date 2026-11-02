using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;

namespace Microsoft.Phone.Globalization;

public sealed class SortedLocaleGrouping
{
    [SecurityCritical]
    private IntPtr _slhHandle = IntPtr.Zero;

    private ReadOnlyCollection<string> _groupDisplayNames;

    private int _groupCount = -1;

    private CultureInfo _actualCulture;

    private bool? _supportsPhonetics = null;

    public ReadOnlyCollection<string> GroupDisplayNames
    {
        get
        {
            EnsureGroupDisplayNames();
            return _groupDisplayNames;
        }
    }

    public CultureInfo CultureInfo
    {
        [SecuritySafeCritical]
        get
        {
            if (_actualCulture == null)
            {
                _actualCulture = CultureInfo.CurrentCulture;
            }
            return _actualCulture;
        }
    }

    public bool SupportsPhonetics
    {
        [SecuritySafeCritical]
        get
        {
            if (!_supportsPhonetics.HasValue)
            {
                _supportsPhonetics = false;
            }
            return _supportsPhonetics.Value;
        }
    }

    public SortedLocaleGrouping()
        : this(CultureInfo.CurrentCulture)
    {
    }

    [SecuritySafeCritical]
    public SortedLocaleGrouping(CultureInfo culture)
    {
        int hresult = Create(culture.Name, out _groupCount, out _slhHandle);
        _actualCulture = culture;
    }

    [SecuritySafeCritical]
    public int GetGroupIndex(string itemName)
    {
        int groupIndex = -1;
        int groupIndexForString = GetGroupIndexForString(_slhHandle, itemName, out groupIndex);
        if (groupIndexForString == -2147024809)
        {
            groupIndex = -1;
        }
        else
        {
            VerifyHR(groupIndexForString);
        }
        return groupIndex;
    }

    [SecuritySafeCritical]
    private void EnsureGroupDisplayNames()
    {
        if (_groupDisplayNames == null)
        {
            List<string> list = new List<string>(_groupCount);
            for (int i = 0; i < _groupCount; i++)
            {
                int groupDisplayString = GetGroupDisplayString(_slhHandle, i, out var groupName);
                VerifyHR(groupDisplayString);
                list.Add(groupName);
            }
            _groupDisplayNames = new ReadOnlyCollection<string>(list);
        }
    }

    [SecuritySafeCritical]
    private void VerifyHR(int hresult)
    {
        if (hresult < 0)
        {
            Marshal.ThrowExceptionForHR(hresult);
        }
    }

    internal static int GetGroupDisplayString(IntPtr session, int groupIndex, out string groupName)
    {
        if (groupIndex >= 0 && groupIndex < 26)
        {
            groupName = ((char)('A' + groupIndex)).ToString();
        }
        else
        {
            groupName = "#";
        }

        return 0;
    }

    internal static int GetGroupIndexForString(IntPtr session, string value, out int groupIndex)
    {
        if (!string.IsNullOrEmpty(value))
        {
            char c = char.ToUpperInvariant(value[0]);

            if (c >= 'A' && c <= 'Z')
            {
                groupIndex = c - 'A';
                return 0;
            }
        }

        groupIndex = 26;

        return 0;
    }
    internal static int Create(string localeName, out int groupCount, out IntPtr session)
    {
        // A-Z + #
        groupCount = 27;
        // WPF 版本不需要真正的 Native Session
        session = IntPtr.Zero;
        return 0; // S_OK
    }
}
