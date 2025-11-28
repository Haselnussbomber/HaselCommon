using System.Diagnostics.CodeAnalysis;
using Lumina.Data;

namespace HaselCommon.Extensions.Dalamud;

public static class IDataManagerExtensions
{
    public static bool TryGetFile<T>(
        this IDataManager dataManager,
        string path,
        [NotNullWhen(returnValue: true)] out T? file,
        [NotNullWhen(returnValue: false)] out Exception? exception)
        where T : FileResource
    {
        try
        {
            file = dataManager.GetFile<T>(path);
            exception = null;
            return file != null;
        }
        catch (Exception ex)
        {
            exception = ex;
            file = null;
            return false;
        }
    }

    public static bool TryGetFile<T>(this IDataManager dataManager, string path, [NotNullWhen(returnValue: true)] out T? file) where T : FileResource
    {
        var result = TryGetFile(dataManager, path, out file, out var exception);
        if (!result)
            ServiceLocator.GetService<ILogger<IDataManager>>()?.LogError(exception, "Unexpected exception while getting file {path}", path);
        return result;
    }
}
