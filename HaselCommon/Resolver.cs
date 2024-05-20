using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Reloaded.Memory.Sigscan;

namespace HaselCommon.Interop;

public sealed class Resolver
{
    private static readonly Lazy<Resolver> Instance = new(() => new Resolver());
    public static Resolver GetInstance => Instance.Value;

    private readonly List<Address> Addresses = [];
    private bool HasResolved = false;

    public unsafe void Resolve()
    {
        if (HasResolved)
            return;

        var pluginLog = Service.PluginLog;
        var sigScanner = Service.SigScanner;

        using var scanner = new Scanner((byte*)sigScanner.SearchBase, sigScanner.Module.ModuleMemorySize);
        var moduleCopyOffset = sigScanner.SearchBase - sigScanner.Module.BaseAddress;

        var sw = new Stopwatch();
        sw.Start();

        var results = scanner.FindPatternsCached(Addresses.Select(a => a.String).ToArray());
        foreach (var (address, result) in Addresses.Zip(results))
        {
            if (!result.Found)
            {
                pluginLog.Error($"[Resolver] Could not find address for signature {address.String}");
                continue;
            }

            var location = sigScanner.SearchBase + result.Offset;

            // resolve call/jmp
            if (*(byte*)location is 0xE8 or 0xE9)
            {
                location += 5 + *(int*)(location + 1);
            }

            // resolve static address pointer
            if (address is StaticAddress sAddress)
            {
                location += sAddress.Offset;
                location += 4 + *(int*)location;
            }

            address.Value = (nuint)(location - moduleCopyOffset);

            pluginLog.Verbose($"[Resolver] Found address {address.Value:X} (ffxiv_dx11.exe+{address.Value - (nuint)sigScanner.Module.BaseAddress:X}) for signature {address.String}");
        }

        sw.Stop();
        pluginLog.Verbose($"[Resolver] Finished scanning {Addresses.Count} addresses in {sw.ElapsedMilliseconds}ms");

        HasResolved = true;
    }

    public void RegisterAddress(Address address)
    {
        Addresses.Add(address);
    }
}
