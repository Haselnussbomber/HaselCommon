using System.Threading.Tasks;

namespace HaselCommon.Extensions;

public static class TaskExtensions
{
    public static Task ContinueOnFrameworkThreadWith(this Task previousTask, Action action, TaskContinuationOptions taskContinuationOptions = TaskContinuationOptions.OnlyOnRanToCompletion)
        => previousTask.ContinueWith(_ => Service.Framework.RunOnFrameworkThread(action), taskContinuationOptions);
}
