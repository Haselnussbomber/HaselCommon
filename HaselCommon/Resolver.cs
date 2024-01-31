/*
MIT License

Copyright (c) 2021 aers

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

// Based on https://github.com/aers/FFXIVClientStructs/blob/main/FFXIVClientStructs/Interop/Resolver.cs
// but completely changed to use SigScanner instead

using System.Collections.Generic;
using System.Linq;

namespace HaselCommon.Interop;

public sealed class Resolver
{
    private static readonly Lazy<Resolver> Instance = new(() => new Resolver());
    public static Resolver GetInstance => Instance.Value;

    private Resolver() { }

    private readonly List<Address> _addresses = [];
    private bool _hasResolved = false;

    public unsafe void Resolve()
    {
        if (_hasResolved)
            return;

        // delete old sig caches
        foreach (var file in Service.PluginInterface.ConfigDirectory.EnumerateFiles().Where(fi => fi.Name.StartsWith("SigCache_")))
        {
            try { file.Delete(); }
            catch { }
        }

        foreach (var address in _addresses)
        {
            if (!Service.SigScanner.TryScanText(address.String, out var location))
            {
                Service.PluginLog.Error($"[Resolver] Could not find address for signature {address.String}");
                continue;
            }

            if (address is StaticAddress sAddress)
            {
                location += sAddress.Offset;
                location += 4 + Marshal.ReadInt32(location);
            }

            address.Value = (nuint)location;

            Service.PluginLog.Verbose($"[Resolver] Found address {address.Value:X} (ffxiv_dx11.exe+{address.Value - (nuint)Service.SigScanner.Module.BaseAddress:X}) for signature {address.String}");
        }

        _hasResolved = true;
    }

    public void RegisterAddress(Address address)
    {
        _addresses.Add(address);
    }
}
