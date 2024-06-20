using Dalamud.Interface.Windowing;

namespace HaselCommon.Windowing.Interfaces;

public interface IWindowManager : IDisposable
{
    bool AddWindow(Window window);
    bool RemoveWindow(Window window);
}
