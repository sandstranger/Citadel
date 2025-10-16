using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ZLinq;

namespace Citadel.Game
{
    public static class Extensions
    {
        public static T[] GetComponentsInChildren<T>(this Component component, bool includeInactive = false, bool includeSelf = false) where T : Component
        {
            return component.gameObject.GetComponentsInChildren<T>(includeInactive,includeSelf);
        }

        public static T[] GetComponentsInChildren<T>(this GameObject gameObject, bool includeInactive = false, bool includeSelf = false) where T : Component
        {
            var components = gameObject.GetComponentsInChildren<T>(includeInactive);

            if (includeSelf)
            {
                var selfComponent = gameObject.GetComponent<T>();
                return selfComponent!=null ? components.Prepend(selfComponent).ToArray() : components;
            }

            return components;
        }
        
        public static async Task<T> WithCancellationAsync<T>(this Task<T> genericTask, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                return await genericTask;
            }

            cancellationToken.ThrowIfCancellationRequested();
            
            Task baseTask = genericTask;
            await baseTask.WithCancellationAsync(cancellationToken);
            return await genericTask;
        }

        public static async Task WithCancellationAsync(this Task task, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                await task;
            }

            cancellationToken.ThrowIfCancellationRequested();

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