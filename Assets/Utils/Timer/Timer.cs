using System;
using System.Collections;
using System.Collections.Generic;
using GameTimer;
using UnityEngine;

namespace GameTimer
{
    /// <summary>
    /// 计时器
    /// </summary>
    public static class Timer
    {
        //
        private static TimerMono mono;
        
        //工作组
        private static List<TimeHandle> work_handles;

        //回收组
        private static Queue<TimeHandle> offline_handles;

        //处理器最大ID
        private static int handleMaxId;

        public static Func<float> deltaTime;
        public static Func<float> unscaledDeltaTime;

        //静态构造
        static Timer()
        {
            if(!mono) mono = new GameObject("TimerMono").AddComponent<TimerMono>();
            //
            work_handles = new List<TimeHandle>();
            offline_handles = new Queue<TimeHandle>();
            handleMaxId = 0;
        }

        /// <summary>
        /// 启动一个计时器
        /// </summary>
        /// <param name="time">计时器的时间</param>
        public static TimeHandle Start(float time)
        {
            return Get().Start(time);
        }

        /// <summary>
        /// 启动一个帧计时器
        /// </summary>
        /// <param name="frame">计时帧数</param>
        /// <returns></returns>
        public static TimeHandle StartByFrame(int frame)
        {
            return Get().StartByFrame(frame);
        }

        /// <summary>
        /// 每帧更新
        /// <para>建议在所有帧函数结束后调用(Unity中的LateUpdate)</para>
        /// </summary>
        /// <param name="time">相对于上帧经过的时间</param>
        public static void Update()
        {
            int lastCount = work_handles.Count;
            for (int i = 0; i < work_handles.Count; i++)
            {
                work_handles[i].Update();
                if (work_handles.Count != lastCount)
                {
                    lastCount = work_handles.Count;
                    i--;
                }
            }
        }

        //获取可用计时器
        private static TimeHandle Get()
        {
            var item = offline_handles.Count > 0 ? offline_handles.Dequeue() : new TimeHandle(++handleMaxId);
            work_handles.Add(item);
            return item;
        }

        //在停止时,回收计时器
        public static void Recover(TimeHandle handle)
        {
            if (!work_handles.Contains(handle))
                return;
            work_handles.Remove(handle);
            offline_handles.Enqueue(handle);
        }

        /// <summary>
        /// 计时器是否工作中
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static bool Working(TimeHandle handle)
        {
            return work_handles.Contains(handle);
        }
    }
}