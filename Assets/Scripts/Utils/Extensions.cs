using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Citadel.Game
{
    internal static class Extensions
    {
        public static async Task<T> WithCancellationAsync<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>();

            using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken))) ;

            var completedTask = await Task.WhenAny(task, tcs.Task);

            if (completedTask == tcs.Task)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            return await task;
        }

        public static async Task WithCancellationAsync(this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>();

            using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken))) ;

            var completedTask = await Task.WhenAny(task, tcs.Task);

            if (completedTask == tcs.Task)
            {
                throw new OperationCanceledException(cancellationToken);
            }

            await task;
        }

        internal static Task ToTask(this AsyncOperation asyncOperation)
        {
            var completionSource = new TaskCompletionSource<object>();
            asyncOperation.completed += _ => completionSource.SetResult(null);
            return completionSource.Task;
        }
    }
}