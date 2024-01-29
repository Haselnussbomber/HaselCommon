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

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using FFXIVClientStructs.FFXIV.Client.System.Framework;

namespace HaselCommon.Interop;

#pragma warning disable

public sealed partial class Resolver
{
    private static readonly Lazy<Resolver> Instance = new(() => new Resolver());

    private Resolver() { }

    public static Resolver GetInstance => Instance.Value;

    private readonly List<Address>?[] _preResolveArray = new List<Address>[256];
    private int _totalBuckets;

    private readonly List<Address> _addresses = new();
    public IReadOnlyList<Address> Addresses => _addresses.AsReadOnly();

    private ConcurrentDictionary<string, long>? _textCache;
    private FileInfo? _cacheFile;
    private bool _cacheChanged = false;

    private nint _baseAddress;
    private nint _targetSpace;
    private int _targetLength;
    private int _textSectionOffset;
    private int _textSectionSize;

    private bool _hasResolved = false;
    private bool _isSetup = false;

    private void LoadCache()
    {
        try
        {
            var json = File.ReadAllText(_cacheFile.FullName);
            _textCache = JsonSerializer.Deserialize<ConcurrentDictionary<string, long>>(json) ?? new ConcurrentDictionary<string, long>();
        }
        catch { }
    }

    private void SaveCache()
    {
        if (_cacheFile == null || _textCache == null || _cacheChanged == false)
            return;
        var json = JsonSerializer.Serialize(_textCache, new JsonSerializerOptions { WriteIndented = true });
        if (string.IsNullOrWhiteSpace(json))
            return;
        if (_cacheFile.Directory is { Exists: false })
            Directory.CreateDirectory(_cacheFile.Directory.FullName);
        File.WriteAllText(_cacheFile.FullName, json);
    }

    private bool ResolveFromCache()
    {
        foreach (Address address in _addresses)
        {
            var str = address is StaticAddress sAddress
                ? $"{sAddress.String}+0x{sAddress.Offset:X}"
                : address.String;
            if (_textCache!.TryGetValue(str, out var offset))
            {
                address.Value = (nuint)(offset + _baseAddress);
                Service.PluginLog.Verbose($"[SigCache] Using cached address {address.Value:X} (ffxiv_dx11.exe+{address.Value - (nuint)_baseAddress:X}) for {str}");
                byte firstByte = (byte)address.Bytes[0];
                _preResolveArray[firstByte]!.Remove(address);
                if (_preResolveArray[firstByte]!.Count == 0)
                {
                    _preResolveArray[firstByte] = null;
                    _totalBuckets--;
                }
            }
        }

        return _addresses.All(a => a.Value != 0);
    }

    // This function is a bit messy, but everything to make it cleaner is slower, so don't bother.
    public unsafe void Resolve()
    {
        if (!_isSetup)
        {
            _baseAddress = Service.SigScanner.Module.BaseAddress;
            _targetSpace = Service.SigScanner.Module.BaseAddress;
            _targetLength = Service.SigScanner.Module.ModuleMemorySize;
            _textSectionOffset = (int)Service.SigScanner.TextSectionOffset;
            _textSectionSize = Service.SigScanner.TextSectionSize;

            string gameVersion;
            unsafe { gameVersion = Framework.Instance()->GameVersion.Base; }
            if (string.IsNullOrEmpty(gameVersion))
                throw new Exception("Unable to read game version.");

            var currentSigCacheName = $"SigCache_{gameVersion}.json";

            // delete old sig caches
            foreach (var file in Service.PluginInterface.ConfigDirectory.EnumerateFiles()
                .Where(fi => fi.Name.StartsWith("SigCache_") && fi.Name != currentSigCacheName))
            {
                try { file.Delete(); }
                catch { }
            }

            _cacheFile = new FileInfo(Path.Join(Service.PluginInterface.ConfigDirectory.FullName, currentSigCacheName));
            if (_cacheFile is { Exists: true })
                LoadCache();

            _isSetup = true;
        }

        if (_hasResolved)
            return;

        if (_targetSpace == 0)
            throw new Exception("[Resolver] Attempted to call Resolve() without initializing the search space.");

        if (_textCache != null)
        {
            if (ResolveFromCache())
                return;
        }

        ReadOnlySpan<byte> targetSpan = new ReadOnlySpan<byte>(_targetSpace.ToPointer(), _targetLength)[_textSectionOffset..];

        for (int location = 0; location < _textSectionSize; location++)
        {
            if (_preResolveArray[targetSpan[location]] is not null)
            {
                List<Address> availableAddresses = _preResolveArray[targetSpan[location]]!.ToList();

                ReadOnlySpan<ulong> targetLocationAsUlong = MemoryMarshal.Cast<byte, ulong>(targetSpan[location..]);

                int avLen = availableAddresses.Count;

                for (int i = 0; i < avLen; i++)
                {
                    Address address = availableAddresses[i];

                    int count;
                    int length = address.Bytes.Length;

                    for (count = 0; count < length; count++)
                    {
                        if ((address.Mask[count] & address.Bytes[count]) != (address.Mask[count] & targetLocationAsUlong[count]))
                            break;
                    }

                    if (count == length)
                    {
                        int outLocation = location;

                        byte firstByte = (byte)address.Bytes[0];
                        if (firstByte is 0xE8 or 0xE9)
                        {
                            var jumpOffset = BitConverter.ToInt32(targetSpan.Slice(outLocation + 1, 4));
                            outLocation = outLocation + 5 + jumpOffset;
                        }

                        if (address is StaticAddress staticAddress)
                        {
                            int accessOffset =
                                BitConverter.ToInt32(targetSpan.Slice(outLocation + staticAddress.Offset, 4));
                            outLocation = outLocation + staticAddress.Offset + 4 + accessOffset;
                        }

                        address.Value = (nuint)(_baseAddress + _textSectionOffset + outLocation);

                        var str = address is StaticAddress sAddress
                            ? $"{sAddress.String}+0x{sAddress.Offset:X}"
                            : address.String;

                        Service.PluginLog.Verbose($"[SigCache] Caching address {address.Value:X} (ffxiv_dx11.exe+{address.Value - (nuint)_baseAddress:X}) for {str}");

                        _textCache ??= [];
                        if (_textCache.TryAdd(str, outLocation + _textSectionOffset) == true)
                            _cacheChanged = true;

                        _preResolveArray[targetSpan[location]].Remove(address);

                        if (availableAddresses.Count == 0)
                        {
                            _preResolveArray[targetSpan[location]] = null;
                            _totalBuckets--;
                            if (_totalBuckets == 0)
                                goto outLoop;
                        }
                    }
                }
            }
        }
outLoop:;

        SaveCache();
        _hasResolved = true;
    }

    public void RegisterAddress(Address address)
    {
        _addresses.Add(address);

        byte firstByte = (byte)address.Bytes[0];

        if (_preResolveArray[firstByte] is null)
        {
            _preResolveArray[firstByte] = new List<Address>();
            _totalBuckets++;
        }

        _preResolveArray[firstByte]!.Add(address);
    }
}

#pragma warning restore
