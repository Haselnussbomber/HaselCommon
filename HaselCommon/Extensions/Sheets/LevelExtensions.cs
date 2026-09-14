namespace HaselCommon.Extensions;

public static class LevelExtensions
{
    extension(Level row)
    {
        public Vector3 Position => new(row.X, row.Y, row.Z);
    }
}
