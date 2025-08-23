using System.Threading.Tasks;
using UnityEngine;

namespace Citadel.Game
{
    internal static class Extensions
    {
        internal static Task ToTask(this AsyncOperation asyncOperation)
        {
            var completionSource = new TaskCompletionSource<object>();
            asyncOperation.completed += _ => completionSource.SetResult(null);
            return completionSource.Task;
        }
    }
}