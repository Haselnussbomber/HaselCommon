using Dalamud.Game.Agent.AgentArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe partial class IAgentLifecycleExtensions
{
    extension(AgentArgs args)
    {
        public T* GetAgent<T>() where T : unmanaged => args.GetAgentPointer<T>();
    }

    extension(AgentReceiveEventArgs args)
    {
        public Span<AtkValue> GetAtkValues() => new((void*)args.AtkValues, (int)args.ValueCount);
    }
}
