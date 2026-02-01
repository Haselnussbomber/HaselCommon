using System.Diagnostics.CodeAnalysis;
using Lumina.Data;

namespace HaselCommon.Extensions;

public static class IDataManagerExtensions
{
    extension(IDataManager dataManager)
    {
        public bool TryGetFile<T>(string path, [NotNullWhen(returnValue: true)] out T? file, out Exception? exception) where T : FileResource
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
    }
}
