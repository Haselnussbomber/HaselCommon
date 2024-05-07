using System.Collections.Generic;
using System.Reflection;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using HaselCommon.Enums;

namespace HaselCommon.Utils.Globals;

public static unsafe class Agent
{
    private static readonly Dictionary<Type, AgentId> AgentIdCache = [];

    public static T* GetAgent<T>(AgentId id) where T : unmanaged
        => (T*)AgentModule.Instance()->GetAgentByInternalId(id);

    public static T* GetAgent<T>() where T : unmanaged
    {
        var type = typeof(T);

        if (!AgentIdCache.TryGetValue(type, out var id))
        {
            var attr = type.GetCustomAttribute<AgentAttribute>(false)
                ?? throw new Exception($"Agent {type.FullName} is missing AgentAttribute");

            AgentIdCache.Add(type, id = attr.ID);
        }

        return GetAgent<T>(id);
    }

    public static nint GetAgentVFuncAddress<T>(AgentInterfaceVfs vf) where T: unmanaged
    {
        var agent = GetAgent<T>();
        if (agent == null)
            return 0;

        var agentVTableAddress = *(nint*)agent;
        if (agentVTableAddress == 0)
            return 0;

        return *(nint*)(agentVTableAddress + 8 * (int)vf);
    }
}
