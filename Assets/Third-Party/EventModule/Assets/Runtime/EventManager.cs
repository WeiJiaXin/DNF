using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Lowy.Event
{
    public interface IEvent
    {
        
    }

    public class EventManager
    {
        private static Dictionary<Type, Delegate> _dic2Delegates;

        static EventManager()
        {
            _dic2Delegates=new Dictionary<Type, Delegate>();
        }
        public static void AddListener<T>(Action<T> action) where T:IEvent
        {
            Type t = typeof(T);
            if (_dic2Delegates.ContainsKey(t))
            {
                _dic2Delegates[t] = Delegate.Combine(_dic2Delegates[t], action);
            }
            else
            {
                _dic2Delegates[t] = action;
            }
        }

        public static void RemoveListener<T>(Action<T> action)
        {
            Type t = typeof(T);
            if (_dic2Delegates.ContainsKey(t))
            {
                _dic2Delegates[t] = Delegate.Remove(_dic2Delegates[t], action);
                if (_dic2Delegates[t] == null)
                    _dic2Delegates.Remove(t);
            }
        }

        public static void RemoveAllListener<T>()
        {
            Type t = typeof(T);
            if (_dic2Delegates.ContainsKey(t))
            {
                _dic2Delegates.Remove(t);
            }
        }

        public static void Dispatch<T>(T data)
        {
            Type t = typeof(T);
            if (_dic2Delegates.ContainsKey(t))
            {
                var events = _dic2Delegates[t].GetInvocationList();
                foreach (var e in events)
                {
                    try
                    {
                        (e as Action<T>)?.Invoke(data);
                    }
                    catch (Exception exception)
                    {
                        if (!Debug.unityLogger.logEnabled)
                            continue;
                        //
                        StackTrace source = new StackTrace(true);
                        StackTrace exc=new StackTrace(exception,true);
                        //
                        DebugException(source,exc);
                    }
                }
            }
        }

        private static void DebugException(StackTrace source, StackTrace exc)
        {
            StringBuilder log = new StringBuilder("EventDispatchError:\n");
            log.Append("<b>Event Method throw error.</b>\n");
            //
            StackFrame frame = null;
            MethodBase method = null;
            for (int i = 0; i < exc.FrameCount - 1; i++)
            {
                frame = exc.GetFrame(i);
                method = frame.GetMethod();
                log.Append(
                    $"{method.ReflectedType?.FullName}:{method} ( at {frame.GetFileName()?.Replace(Application.dataPath, "")}:{frame.GetFileLineNumber()})\n");
            }

            frame = exc.GetFrame(exc.FrameCount - 1);
            method = frame.GetMethod();
            log.Append(
                $"<b>{method.ReflectedType?.FullName}:{method} ( at {frame.GetFileName()?.Replace(Application.dataPath, "")}:{frame.GetFileLineNumber()})</b>\n");
            for (int i = 1; i < source.FrameCount; i++)
            {
                frame = source.GetFrame(i);
                method = frame.GetMethod();
                log.Append(
                    $"{method.ReflectedType?.FullName}:{method} ( at {frame.GetFileName()?.Replace(Application.dataPath, "")}:{frame.GetFileLineNumber()})\n");
            }

            //
            StackTraceLogType stackTraceLevel = Application.GetStackTraceLogType(LogType.Error);
            Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.None);
            Debug.LogError(log.ToString());
            Application.SetStackTraceLogType(LogType.Error, stackTraceLevel);
        }
    }
}