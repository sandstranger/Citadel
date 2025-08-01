using System.Threading.Tasks;
using UnityEngine;

internal static class Extensions
{
    public static Task ToTask(this AsyncOperation asyncOperation)
    {
        var task = new TaskCompletionSource<object>();
        asyncOperation.completed += _ => task.SetResult(null);
        return task.Task;
    }
}