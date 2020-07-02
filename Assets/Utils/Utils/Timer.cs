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
    public static TimeHandle Schedule(float delay, Action task)
    {
        var timer = Timer.Start(delay);
        timer.onEnd += h => task?.Invoke();
        return timer;
    }

    public static TimeHandle ScheduleRealtime(float delay, Action task)
    {
        var timer = Timer.Start(delay);
        timer.unScaleTime = true;
        timer.onEnd += h => task?.Invoke();
        return timer;
    }

    public static TimeHandle ScheduleByFrame(int frame, Action task)
    {
        var timer = Timer.StartByFrame(frame);
        timer.onEnd += h => task?.Invoke();
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

    public static ScoreUpdater ScheduleScoreUpdater(int from, int to, Action<int> onProgress, Action<int> onComplete,
        MonoBehaviour behaviour)
    {
        var updater = new ScoreUpdater(from, to, onProgress, onComplete);
        behaviour.StartCoroutine(DoUpdate(updater));
        return updater;
    }

    private static IEnumerator DoUpdate(IUpdater updater)
    {
        while (!updater.IsCompleted)
        {
            yield return DoTaskEndOfFrame(updater.Update);
        }
    }
}

public interface IUpdater
{
    bool IsCompleted { get; }
    void Update();
    void Complete(bool callsCompletion);
}

public class ScoreUpdater : IUpdater
{
    private static readonly float UpdateInterval = 0.0333f;

    public bool IsCompleted => !isUpdatingScore;

    private int fromValue;
    private int toValue;
    private Action<int> onProgress;
    private Action<int> onComplete;

    private bool isUpdatingScore;
    private int currentValue;
    private float lastTimeSinceStartup;

    public ScoreUpdater(int fromValue, int toValue, Action<int> onProgress, Action<int> onComplete)
    {
        this.fromValue = fromValue;
        this.toValue = toValue;
        this.onProgress = onProgress;
        this.onComplete = onComplete;

        isUpdatingScore = true;
        currentValue = fromValue;
        lastTimeSinceStartup = Time.realtimeSinceStartup;
    }

    public void Update()
    {
        if (isUpdatingScore)
        {
            if (currentValue < toValue)
            {
                var deltaTime = Time.realtimeSinceStartup - lastTimeSinceStartup;
                if (deltaTime < UpdateInterval)
                {
                    return;
                }

                lastTimeSinceStartup += UpdateInterval;

                int delta = toValue - currentValue;
                int step = 1;

                if (delta > 1000000)
                {
                    delta /= 10;
                }
                else if (delta < 100)
                {
                    delta *= 10;
                }

                while (delta >= 100)
                {
                    step = step * 10 + 1;
                    delta /= 10;
                }

                step = step * 3 / 10 * 10;

                if (toValue - currentValue < 30)
                {
                    step = toValue - currentValue;
                }

                currentValue += step;
                onProgress?.Invoke(currentValue);
            }
            else
            {
                isUpdatingScore = false;
                onComplete?.Invoke(toValue);
            }
        }
    }

    public void Complete(bool callsCompletion)
    {
        // ignore if updater is already completed
        if (!isUpdatingScore)
        {
            return;
        }

        isUpdatingScore = false;
        if (callsCompletion && onComplete != null)
        {
            onComplete.Invoke(toValue);
        }
    }
}