using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HMX.HASSActron
{
	public static class AsyncExtension
	{
		public static async Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken cancellationToken)
		{
			RegisteredWaitHandle registeredHandle = null;
			CancellationTokenRegistration tokenRegistration = default(CancellationTokenRegistration);
			try
			{
				var tcs = new TaskCompletionSource<bool>();
				registeredHandle = ThreadPool.RegisterWaitForSingleObject(
					handle,
					(state, timedOut) => ((TaskCompletionSource<bool>)state).TrySetResult(!timedOut),
					tcs,
					millisecondsTimeout,
					true);
				tokenRegistration = cancellationToken.Register(
					state => ((TaskCompletionSource<bool>)state).TrySetCanceled(),
					tcs);
				return await tcs.Task;
			}
			finally
			{
				if (registeredHandle != null)
					registeredHandle.Unregister(null);
				tokenRegistration.Dispose();
			}
		}
	}
}
