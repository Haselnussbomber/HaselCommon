namespace HaselCommon.Events;

public static class PlayerEvents
{
    public const string Login = $"{nameof(PlayerEvents)}.Login";
    public const string Logout = $"{nameof(PlayerEvents)}.Logout";
    public const string ClassJobChanged = $"{nameof(PlayerEvents)}.ClassJobChanged";
    public const string LevelChanged = $"{nameof(PlayerEvents)}.LevelChanged";
    public const string UnlocksChanged = $"{nameof(PlayerEvents)}.UnlocksChanged";

    public struct LoginArgs
    {
        public ulong ContentId;
        public override string ToString() => $"LoginArgs {{ ContentId = {ContentId:X} }}";
    }

    public struct LogoutArgs
    {
        public int Type; // 2 opens the Dialogue box
        public int Code; // 10000 for normal logout, 90002 for disconnect (lost connection)
        public override string ToString() => $"LogoutArgs {{ Type = {Type}, Code = {Code} }}";
    }

    public struct ClassJobChangedArgs
    {
        public uint ClassJobId;
        public override string ToString() => $"ClassJobChangedArgs {{ ClassJobId = {ClassJobId} }}";
    }

    public struct LevelChangedArgs
    {
        public uint ClassJobId;
        public uint Level;
        public override string ToString() => $"LevelChangedArgs {{ ClassJobId = {ClassJobId}, Level = {Level} }}";
    }
}
