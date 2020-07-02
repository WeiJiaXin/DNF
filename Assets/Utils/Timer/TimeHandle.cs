using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameTimer
{
    /// <summary>
    /// 时间处理器
    /// </summary>
    public class TimeHandle
    {
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="id">唯一ID</param>
        public TimeHandle(int id)
        {
            Id = id;
            state = EHandleState.None;
        }

        //状态
        private EHandleState state;

        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// 是否帧计时
        /// </summary>
        public bool IsFrameTimer { get; private set; }

        /// <summary>
        /// 此计时器的总时间
        /// </summary>
        public float Time { get; private set; }

        /// <summary>
        /// 此计时器当前计算的时间
        /// </summary>
        public float CurrentTime { get; private set; }

        /// <summary>
        /// 循环次数,默认一次.
        /// <para>使用-1来无限循环</para>
        /// </summary>
        public int loopCount = 1;

        //当前循环次数
        public int currentLoopCount { get; private set; }

        /// <summary>
        /// 不缩放时间
        /// </summary>
        public bool unScaleTime = false;

        //暂停
        private bool _pause = false;

        /// <summary>
        /// 暂停
        /// </summary>
        public bool Pause
        {
            get => _pause;
            set
            {
                if (_pause == value)
                    return;
                _pause = value;
                Invoke(onPause,this,_pause);
                state = value ? EHandleState.Pause : EHandleState.Run;
            }
        }

        /// <summary>
        /// 在启动时调用
        /// </summary>
        public event Action<TimeHandle> onStart;

        /// <summary>
        /// 在每帧Timer更新调用
        /// </summary>
        public event Action<TimeHandle, float> onUpdate;

        /// <summary>
        /// 在每整秒调用
        /// <para>在<seealso cref="onUpdate"/>结束后检测</para>
        /// </summary>
        public event Action<TimeHandle, int> onSecond;

        /// <summary>
        /// 在暂停或继续时调用
        /// </summary>
        public event Action<TimeHandle, bool> onPause;

        /// <summary>
        /// 在每次循环点调用
        /// </summary>
        public event Action<TimeHandle, int> onLoop;

        /// <summary>
        /// 在计时器结束调用,<seealso cref="Stop"/>也会执行此事件
        /// </summary>
        public event Action<TimeHandle> onEnd;

        /// <summary>
        /// 启动计时器,并重置相关设置
        /// </summary>
        /// <param name="time">计时器的总时间</param>
        public TimeHandle Start(float time)
        {
            Time = time;
            IsFrameTimer = false;
            CurrentTime = Time;
            loopCount = 1;
            currentLoopCount = 0;
            unScaleTime = false;
            _pause = false;
            Invoke(onStart,this);
            state = EHandleState.Run;
            return this;
        }

        public TimeHandle StartByFrame(int frame)
        {
            Start(frame);
            IsFrameTimer = true;
            return this;
        }

        //在循环点执行
        private void OnLoop()
        {
            Invoke(onLoop,this,currentLoopCount);
            CurrentTime = Time;
        }

        /// <summary>
        /// 每帧更新函数
        /// </summary>
        /// <param name="time">相对于上帧经过的时间</param>
        public void Update()
        {
            if (state == EHandleState.End ||
                state == EHandleState.Pause)
                return;
            if (IsFrameTimer)
            {
                UpdateByFrame();
                return;
            }

            var oldTime = CurrentTime;
            var calTime = unScaleTime ? Timer.deltaTime() : Timer.unscaledDeltaTime();
            CurrentTime -= calTime;
            Invoke(onUpdate,this,calTime);
            if ((int) CurrentTime != (int) oldTime)
                Invoke(onSecond,this,(int) oldTime);
            if (CurrentTime <= 0)
                End();
        }

        private void UpdateByFrame()
        {
            Invoke(onUpdate,this, 1f);
            CurrentTime -= 1;
            if (CurrentTime <= 0)
                End();
        }

        //计时器结束
        private void End()
        {
            currentLoopCount++;
            if (currentLoopCount < loopCount || loopCount < 0)
            {
                OnLoop();
                return;
            }

            Stop();
        }

        /// <summary>
        /// 停止计时器
        /// </summary>
        public void Stop(bool trackEvent = true)
        {
            state = EHandleState.End;
            if (trackEvent)
                Invoke(onEnd, this);
            OnDispose();
            Timer.Recover(this);
        }

        /// <summary>
        /// 重置时间
        /// </summary>
        public void ResetTime()
        {
            if (state == EHandleState.End)
                return;
            CurrentTime = Time;
        }

        /// <summary>
        /// 添加时间(延长计时器时间将影响之后的循环)
        /// </summary>
        /// <param name="add">添加的时间</param>
        /// <param name="applyCurrent">是否应用于当前循环的时间</param>
        public void AddTime(float add, bool applyCurrent = true)
        {
            if (state == EHandleState.End)
                return;
            Time += add;
            if (applyCurrent)
                CurrentTime += add;
        }

        /// <summary>
        /// 添加时间(延长计时器时间将影响之后的循环)
        /// </summary>
        /// <param name="add">添加的时间</param>
        /// <param name="applyCurrent">是否应用于当前循环的时间</param>
        public void AddTimeByFrame(int add, bool applyCurrent = true)
        {
            if (state == EHandleState.End)
                return;
            AddTime(add, applyCurrent);
        }

        //释放
        private void OnDispose()
        {
            onStart = null;
            onUpdate = null;
            onSecond = null;
            onLoop = null;
            onEnd = null;
            IsFrameTimer = false;
            _pause = false;
            state = EHandleState.None;
        }
        

        private static void Invoke(Delegate del,params object[] pars)
        {
            if(del==null)
                return;
            var list = del.GetInvocationList();
            foreach (var item in list)
            {
                if (item.Target == null && !item.Method.IsStatic)
                    continue;
                if(item.Target is UnityEngine.Object o && o == null)
                    continue;
                try
                {
                    item.DynamicInvoke(pars);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
        }

        /// <summary>
        /// 格式化时间(小时:分钟:秒)
        /// </summary>
        /// <param name="c">时间分隔符</param>
        /// <returns>字符串</returns>
        public string ToHHMMSS(char c = ':')
        {
            if (IsFrameTimer)
                return $"-{c}-{c}-";
            return $"{(int) (CurrentTime / (60 * 60))}{c}" +
                   $"{(int) ((CurrentTime / 60) % 60)}{c}" +
                   $"{(int) (CurrentTime % 60)}";
        }

        /// <summary>
        /// 格式化时间(分钟:秒)
        /// </summary>
        /// <param name="c">时间分隔符</param>
        /// <returns>字符串</returns>
        public string ToMMSS(char c = ':')
        {
            if (IsFrameTimer)
                return $"-{c}-";
            return $"{(int) (CurrentTime / 60)}{c}" +
                   $"{(int) (CurrentTime % (60))}";
        }
    }
}