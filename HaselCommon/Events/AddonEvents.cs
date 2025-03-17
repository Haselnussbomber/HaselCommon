using FFXIVClientStructs.FFXIV.Component.GUI;

namespace HaselCommon.Events;

public static class AddonEvents
{
    public const string Open = $"{nameof(AddonEvents)}.Open";
    public const string Close = $"{nameof(AddonEvents)}.Close";
    public const string PreSetup = $"{nameof(AddonEvents)}.PreSetup";
    public const string PostSetup = $"{nameof(AddonEvents)}.PostSetup";
    public const string PreFinalize = $"{nameof(AddonEvents)}.PreFinalize";
    public const string PreRequestedUpdate = $"{nameof(AddonEvents)}.PreRequestedUpdate";
    public const string PostRequestedUpdate = $"{nameof(AddonEvents)}.PostRequestedUpdate";
    public const string PreRefresh = $"{nameof(AddonEvents)}.PreRefresh";
    public const string PostRefresh = $"{nameof(AddonEvents)}.PostRefresh";
    public const string PreReceiveEvent = $"{nameof(AddonEvents)}.PreReceiveEvent";
    public const string PostReceiveEvent = $"{nameof(AddonEvents)}.PostReceiveEvent";

    public struct AddonEventArgs
    {
        public string AddonName;
        public override string ToString() => $"AddonEventArgs {{ AddonName = \"{AddonName}\" }}";
    }

    public struct RefreshArgs
    {
        public string AddonName;
        public uint AtkValueCount;
        public nint AtkValues;
        public unsafe Span<AtkValue> AtkValueSpan => new((void*)AtkValues, (int)AtkValueCount);
        public override string ToString() => $"RefreshArgs {{ AddonName = \"{AddonName}\", AtkValueCount = {AtkValueCount}, AtkValues = 0x{AtkValues:X} }}";
    }

    public struct EventArgs
    {
        public string AddonName;
        public AtkEventType EventType;
        public Pointer<AtkEventData> Data;
        public override unsafe string ToString() => $"EventArgs {{ AddonName = \"{AddonName}\", EventType = {EventType}, Data = 0x{(nint)Data.Value:X} }}";
    }
}
