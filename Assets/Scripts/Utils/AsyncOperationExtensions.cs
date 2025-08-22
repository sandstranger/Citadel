using System.Threading.Tasks;
using UnityEngine;

namespace Citadel.Extensions
{
    internal static class AsyncOperationExtensions
    {
        internal static Task ToTask(this AsyncOperation asyncOperation)
        {
            var completionSource = new TaskCompletionSource<object>();
            asyncOperation.completed += _ => completionSource.SetResult(null);
            return completionSource.Task;
        }
    }
}