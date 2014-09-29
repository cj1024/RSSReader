using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RSSReader.Common
{

    public static class TaskExtensions
    {

        public static void RunSynchronously(Func<Task> item)
        {
            SynchronizationContext oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            synch.Post(async _ =>
                             {
                                 try
                                 {
                                     await item();
                                 }
                                 catch (Exception e)
                                 {
                                     synch.InnerException = e;
                                     throw;
                                 }
                                 finally
                                 {
                                     synch.EndMessageLoop();
                                 }
                             }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
        }

        public static T RunSynchronously<T>(Func<Task<T>> item)
        {
            SynchronizationContext oldContext = SynchronizationContext.Current;
            var synch = new ExclusiveSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(synch);
            T ret = default(T);
            synch.Post(async _ =>
                             {
                                 try
                                 {
                                     ret = await item();
                                 }
                                 catch (Exception e)
                                 {
                                     synch.InnerException = e;
                                     throw;
                                 }
                                 finally
                                 {
                                     synch.EndMessageLoop();
                                 }
                             }, null);
            synch.BeginMessageLoop();
            SynchronizationContext.SetSynchronizationContext(oldContext);
            return ret;
        }

        private class ExclusiveSynchronizationContext : SynchronizationContext
        {
            private bool done;
            public Exception InnerException { get; set; }
            private readonly AutoResetEvent workItemsWaiting = new AutoResetEvent(false);

            private readonly Queue<Tuple<SendOrPostCallback, object>> items =
                new Queue<Tuple<SendOrPostCallback, object>>();

            public override void Send(SendOrPostCallback d, object state)
            {
                throw new NotSupportedException("We cannot send to our same thread");
            }

            public override void Post(SendOrPostCallback d, object state)
            {
                lock (items)
                {
                    items.Enqueue(Tuple.Create(d, state));
                }
                workItemsWaiting.Set();
            }

            public void EndMessageLoop()
            {
                Post(_ => done = true, null);
            }

            public void BeginMessageLoop()
            {
                while (!done)
                {
                    Tuple<SendOrPostCallback, object> task = null;
                    lock (items)
                    {
                        if (items.Count > 0)
                        {
                            task = items.Dequeue();
                        }
                    }
                    if (task != null)
                    {
                        task.Item1(task.Item2);
                        if (InnerException != null) // the method threw an exeption
                        {
                            throw new AggregateException("TaskExtentions.RunSynchronously method threw an exception.",
                                InnerException);
                        }
                    }
                    else
                    {
                        workItemsWaiting.WaitOne();
                    }
                }
            }

            public override SynchronizationContext CreateCopy()
            {
                return this;
            }
        }

    }
}
