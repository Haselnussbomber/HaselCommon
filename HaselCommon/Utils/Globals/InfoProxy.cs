using System.Collections.Generic;
using System.Reflection;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace HaselCommon.Utils.Globals;

public static unsafe class InfoProxy
{
    private static readonly Dictionary<Type, InfoProxyId> InfoProxyIdCache = [];

    public static T* GetInfoProxy<T>(InfoProxyId id) where T : unmanaged
        => (T*)InfoModule.Instance()->GetInfoProxyById(id);

    public static T* GetInfoProxy<T>() where T : unmanaged
    {
        var type = typeof(T);

        if (!InfoProxyIdCache.TryGetValue(type, out var id))
        {
            var attr = type.GetCustomAttribute<InfoProxyAttribute>(false)
                ?? throw new Exception($"Agent {type.FullName} is missing InfoProxyAttribute");

            InfoProxyIdCache.Add(type, id = attr.InfoProxyId);
        }

        return GetInfoProxy<T>(id);
    }
}
