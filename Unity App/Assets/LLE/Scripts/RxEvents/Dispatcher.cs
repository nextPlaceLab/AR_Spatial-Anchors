
using Microsoft.Azure.SpatialAnchors.Unity;
using System;
using System.Collections.Generic;
using System.Threading;


using UniRx;
using UnityEngine;

namespace LLE.Rx
{
    internal class Dispatcher
    {
        private const bool isDebug = false;

        public enum ThreadType { UI, Current, Pool }

        private static readonly Dictionary<ThreadType, IScheduler> scheduler;

        static Dispatcher()
        {
            // setup RX configuration.
            UnityScheduler.SetDefaultForUnity();
            scheduler = new Dictionary<ThreadType, IScheduler>()
                {
                    {ThreadType.UI,Scheduler.MainThread},
                    {ThreadType.Current,Scheduler.CurrentThread },
                    {ThreadType.Pool, Scheduler.ThreadPool}
                };

        }

        #region Func<T> -> Callback(T)

        public static void ProcessOnUI<T>(Func<T> unityAPIJob, Action<T> callback = null, ThreadType notifyOn = ThreadType.Pool)
        {
            //if (Thread.CurrentThread.ManagedThreadId == 1)
            //    Execute(unityAPIJob, callback, notifyOn);
            //else
            ScheduleParameteredJob(unityAPIJob, ThreadType.UI, callback, notifyOn);
        }

        public static void Process<T>(Func<T> unityAPIJob, Action<T> callback = null, ThreadType notifyOn = ThreadType.Current)
        {
            ScheduleParameteredJob(unityAPIJob, ThreadType.Pool, callback, notifyOn);
        }

        private static void Execute<T>(Func<T> unityAPIJob, Action<T> callback, ThreadType notifyOn)
        {
#if DEBUG
            if (isDebug) Debug.Log("Invoke UI job");
#endif
            var t = unityAPIJob.Invoke();
            Notify(callback, notifyOn, t);
        }

        #endregion Func<T> -> Callback(T)

        #region Action -> Action()

        public static void ProcessOnUI(Action unityAPIJob, Action callback = null, ThreadType notifyOn = ThreadType.Pool)
        {
            //if (Thread.CurrentThread.ManagedThreadId == 1)
            //    Execute(unityAPIJob, callback, notifyOn);
            //else
            ScheduleJob(unityAPIJob, ThreadType.UI, callback, notifyOn);
        }

        /// <summary>
        /// Executes given action on ThreadPool. Callback gets notified on the same thread.
        /// </summary>
        /// <param name="unityAPIJob"></param>
        /// <param name="callback">Optional: callback on finished</param>
        /// <param name="notifyOn">Optional: defines thread for callback</param>
        public static void Process(Action unityAPIJob, Action callback = null, ThreadType notifyOn = ThreadType.Current)
        {
            ScheduleJob(unityAPIJob, ThreadType.Pool, callback, notifyOn);
        }

        private static void Execute(Action unityAPIJob, Action callback, ThreadType notifyOn)
        {
#if DEBUG
            if (isDebug) Debug.Log("Invoke UI job");
#endif
            unityAPIJob.Invoke();
            Notify(callback, notifyOn);
        }

        #endregion Action -> Action()

        #region Func<T> -> void Callback()

        public static void ProcessOnUI<T>(Func<T> unityAPIJob, Action callback, ThreadType notifyOn = ThreadType.Pool)
        {
            Process(unityAPIJob, ThreadType.UI, callback, notifyOn);
        }

        public static void Process<T>(Func<T> unityAPIJob, Action callback = null, ThreadType notifyOn = ThreadType.Current)
        {
            Process(unityAPIJob, ThreadType.Pool, callback, notifyOn);
        }

        public static void Process<T>(Func<T> unityAPIJob, ThreadType workOn, Action callback = null, ThreadType notifyOn = ThreadType.Pool)
        {
            ScheduleJob(() => { unityAPIJob(); }, workOn, callback, notifyOn);
        }

        #endregion Func<T> -> void Callback()

        #region Scheduling

        public static void ScheduleJob(Action unityAPIJob, ThreadType workOn, Action callback = null, ThreadType notifyOn = ThreadType.Pool)
        {
            Schedule(workOn, () =>
             {
                 Execute(unityAPIJob, callback, notifyOn);
             });

            //scheduler[workOn].Schedule(() =>
            //{
            //    try
            //    {
            //        Execute(unityAPIJob, callback, notifyOn);
            //    }
            //    catch (Exception e)
            //    {
            //        Log.Error("Exception: {0}", e);
            //    }
            //});
        }

        public static void ScheduleParameteredJob<T>(Func<T> unityAPIJob, ThreadType workOn, Action<T> callback = null, ThreadType notifyOn = ThreadType.Pool)
        {
            //scheduler[workOn].Schedule(() =>
            //{
            //    Execute(unityAPIJob, callback, notifyOn);
            //});
            Schedule(workOn, () =>
             {
                 Execute(unityAPIJob, callback, notifyOn);
             });
        }

        private static void Schedule(ThreadType workOn, Action task)
        {
            switch (workOn)
            {
                case ThreadType.UI:
                    if (System.Threading.Thread.CurrentThread.ManagedThreadId == 1)
                    {
                        task.Invoke();
                    }
                    else
                    {
                        //UIWorker.AddJob(task);
                        UnityDispatcher.InvokeOnAppThread(task);
                    }
                    //UIWorker.AddJob(task);
                    break;

                case ThreadType.Current:
                case ThreadType.Pool:
                default:
                    ExecuteOnScheduler(workOn, task);
                    break;
            }
        }

        private static void ExecuteOnScheduler(ThreadType workOn, Action task)
        {
            scheduler[workOn].Schedule(() =>
            {
                try
                {
                    task.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError("Exception: "+ e);
                }
            });
        }

        #endregion Scheduling

        #region notify

        private static void Notify<T>(Action<T> callback, ThreadType notifyOn, T t)
        {
            if (callback == null) return;
#if DEBUG
            if (isDebug) Debug.LogFormat("Notify callback using scheduler = {0}, T = {1}", notifyOn, t);
#endif
            if (notifyOn == ThreadType.Current)
                callback(t);
            else
                scheduler[notifyOn].Schedule(() => callback(t));
        }

        private static void Notify(Action callback, ThreadType notifyOn)
        {
            if (callback == null) return;
#if DEBUG
            if (isDebug) Debug.LogFormat("Notify callback using scheduler = {0}", notifyOn);
#endif
            if (notifyOn == ThreadType.Current)
                callback.Invoke();
            else
                scheduler[notifyOn].Schedule(callback);
        }

        #endregion notify
    }
}