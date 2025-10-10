using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Citadel.Game
{
    internal static class Extensions
    {
        private static SynchronizationContext _unityContext;
    
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            _unityContext = SynchronizationContext.Current;
        }
    
        public static void SpinWait(this Task task)
        {
            if (SynchronizationContext.Current == _unityContext)
            {
                WaitInMainThread(task);
            }
            else
            {
                _unityContext.Send(_ => WaitInMainThread(task), null);
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

            using (cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken))) ;

            var completedTask = await Task.WhenAny(task, tcs.Task);

            if (completedTask == tcs.Task)
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }

        internal static Task ToTask(this AsyncOperation asyncOperation)
        {
            var completionSource = new TaskCompletionSource<object>();
            asyncOperation.completed += _ => completionSource.SetResult(null);
            return completionSource.Task;
        }
        
        private static void WaitInMainThread(Task task)
        {
            var frameCount = 0;
            var spinWait = new SpinWait();
        
            while (!task.IsCompleted)
            {
                spinWait.SpinOnce();
                frameCount++;
            
                if (frameCount % 10 == 0)
                {
                    UnityEngine.EventSystems.ExecuteEvents.Execute(null, null, 
                        UnityEngine.EventSystems.ExecuteEvents.updateSelectedHandler);
                }

                if (frameCount > 10000)
                {
                    throw new TimeoutException("Task wait timeout in main thread");
                }
            }
        }
    }
}