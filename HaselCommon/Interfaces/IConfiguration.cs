using Dalamud.Configuration;

namespace HaselCommon.Interfaces;

public interface IConfiguration : IPluginConfiguration, IDisposable
{
    public int LastSavedConfigHash { get; set; }

    public void Save();
    public string Serialize();

    void IDisposable.Dispose() => Save();
}
