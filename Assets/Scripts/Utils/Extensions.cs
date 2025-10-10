using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace Citadel.Game
{
    internal static class Extensions
    {
        public static async UniTask<T> WithCancellationAsync<T>(this UniTask<T> genericTask,
            CancellationToken cancellationToken)
        {
            UniTask baseTask = genericTask;
            await baseTask.WithCancellationAsync(cancellationToken);
            return genericTask.GetAwaiter().GetResult();
        }
        
        public static async UniTask WithCancellationAsync(this UniTask task, CancellationToken cancellationToken)
        {
            UniTaskCompletionSource taskCompletion = new UniTaskCompletionSource();
            
            using (cancellationToken.Register(() => taskCompletion.TrySetResult()));

            var completedResult = await UniTask.WhenAny(task, taskCompletion.Task);

            if (completedResult == 1)
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }
        
        public static async Task<T> WithCancellationAsync<T>(this Task<T> genericTask, CancellationToken cancellationToken)
        {
            Task baseTask = genericTask;
            await baseTask.WithCancellationAsync(cancellationToken);
            return await genericTask;
        }

        public static async Task WithCancellationAsync(this Task task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<object>();

            using (cancellationToken.Register(() => tcs.TrySetResult(null))) ;

            var completedTask = await Task.WhenAny(task, tcs.Task);

            if (completedTask == tcs.Task)
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }
    }
}