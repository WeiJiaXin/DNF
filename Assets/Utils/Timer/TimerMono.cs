using System;
using GameTimer;
using UnityEngine;

public class TimerMono : MonoBehaviour
{
    private void Awake()
    {
        Timer.deltaTime = OnTimerOnDeltaTime;
        Timer.unscaledDeltaTime = OnTimerOnUnScaledDeltaTime;
        DontDestroyOnLoad(gameObject);
    }

    private float OnTimerOnDeltaTime()
    {
        return Time.deltaTime;
    }

    private float OnTimerOnUnScaledDeltaTime()
    {
        return Time.unscaledDeltaTime;
    }
    
    private void Update()
    {
        Timer.Update();
    }
}