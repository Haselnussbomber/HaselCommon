using Dalamud.Game.Agent.AgentArgTypes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Extensions;

public static unsafe class IAgentLifecycleExtensions
{
    extension(AgentReceiveEventArgs args)
    {
        public Span<AtkValue> GetAtkValues() => new((void*)args.AtkValues, (int)args.ValueCount);
    }
}
