using System;
using System.Collections;
using GameTimer;
using UnityEngine;

public class TimerStatic
{
    private static WaitForEndOfFrame waitForEndOfFrame;

    static TimerStatic()
    {
        waitForEndOfFrame = new WaitForEndOfFrame();
    }

    public static TimeHandle Schedule(float delay, Action<TimeHandle> task)
    {
        var timer = Timer.Start(delay);
        timer.onEnd += task;
        return timer;
    }

    public static TimeHandle ScheduleRealtime(float delay, Action<TimeHandle> task)
    {
        var timer = Timer.Start(delay);
        timer.unScaleTime = true;
        timer.onEnd += task;
        return timer;
    }

    public static TimeHandle ScheduleByFrame(int frame, Action<TimeHandle> task)
    {
        var timer = Timer.StartByFrame(frame);
        timer.onEnd += task;
        return timer;
    }

    public static void ScheduleEndOfFrame(Action task, MonoBehaviour behaviour)
    {
        behaviour.StartCoroutine(DoTaskEndOfFrame(task));
    }

    private static IEnumerator DoTaskEndOfFrame(Action task)
    {
        yield return waitForEndOfFrame;
        task();
    }
}