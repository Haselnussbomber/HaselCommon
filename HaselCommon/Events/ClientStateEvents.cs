using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Events;

public static class ClientStateEvents
{
    public static readonly string CfPop = "ClientState.CfPop";
    public static readonly string ClassJobChange = "ClientState.ClassJobChange";
    public static readonly string EnterPvP = "ClientState.EnterPvP";
    public static readonly string LeavePvP = "ClientState.LeavePvP";
    public static readonly string LevelChange = "ClientState.LevelChange";
    public static readonly string Login = "ClientState.Login";
    public static readonly string Logout = "ClientState.Logout";
    public static readonly string TerritoryChange = "ClientState.TerritoryChange";

    public class CfPopEventArgs : EventArgs
    {
        private static readonly CfPopEventArgs Instance = new();
        private CfPopEventArgs() { }

        public ContentFinderCondition? ContentFinderCondition { get; private set; }

        internal static CfPopEventArgs With(ContentFinderCondition cfc)
        {
            Instance.ContentFinderCondition = cfc;
            return Instance;
        }
    }

    public class ClassJobChangeEventArgs : EventArgs
    {
        private static readonly ClassJobChangeEventArgs Instance = new();
        private ClassJobChangeEventArgs() { }

        public uint ClassJobId { get; private set; }

        internal static ClassJobChangeEventArgs With(uint classJobId)
        {
            Instance.ClassJobId = classJobId;
            return Instance;
        }
    }

    public class LevelChangeEventArgs : EventArgs
    {
        private static readonly LevelChangeEventArgs Instance = new();
        private LevelChangeEventArgs() { }

        public uint ClassJobId { get; private set; }
        public uint Level { get; private set; }

        internal static LevelChangeEventArgs With(uint classJobId, uint level)
        {
            Instance.ClassJobId = classJobId;
            Instance.Level = level;
            return Instance;
        }
    }

    public class TerritoryChangeEventArgs : EventArgs
    {
        private static readonly TerritoryChangeEventArgs Instance = new();
        private TerritoryChangeEventArgs() { }

        public ushort TerritoryTypeId { get; private set; }

        internal static TerritoryChangeEventArgs With(ushort territoryTypeId)
        {
            Instance.TerritoryTypeId = territoryTypeId;
            return Instance;
        }
    }
}
