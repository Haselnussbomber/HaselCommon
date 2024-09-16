using Lumina.Excel.GeneratedSheets;

namespace HaselCommon.Events;

public static class ClientStateEvents
{
    public const string CfPop = "ClientState.CfPop";
    public const string ClassJobChange = "ClientState.ClassJobChange";
    public const string EnterPvP = "ClientState.EnterPvP";
    public const string LeavePvP = "ClientState.LeavePvP";
    public const string LevelChange = "ClientState.LevelChange";
    public const string Login = "ClientState.Login";
    public const string Logout = "ClientState.Logout";
    public const string TerritoryChange = "ClientState.TerritoryChange";

    public class CfPopEventArgs : EventArgs
    {
        public required ContentFinderCondition ContentFinderCondition { get; init; }
    }

    public class ClassJobChangeEventArgs : EventArgs
    {
        public required uint ClassJobId { get; init; }
    }

    public class LevelChangeEventArgs : EventArgs
    {
        public required uint ClassJobId { get; init; }
        public required uint Level { get; init; }
    }

    public class TerritoryChangeEventArgs : EventArgs
    {
        public required ushort TerritoryTypeId { get; init; }
    }
}
