using System;
using UnityEngine;
using UnityEngine.Events;

public enum TimerType
{
    None,
    Countdown,
    Stopwatch
}

public enum TimerDisplaySetting
{
    HourMinSec,
    HourMin,
    MinSec
}

[Serializable]
public struct TimerTime : IEquatable<TimerTime>, IComparable<TimerTime>
{
    public int hours;
    public int minutes;
    public float seconds;

    public TimerTime(int hours, int minutes, float seconds)
    {
        this.hours = hours;
        this.minutes = minutes;
        this.seconds = seconds;
    }

    public bool Equals(TimerTime other) => hours == other.hours && minutes == other.minutes && Mathf.Approximately(seconds, other.seconds);
    
    public int CompareTo(TimerTime other)
    {
        var hoursComparison = hours.CompareTo(other.hours);
        if (hoursComparison != 0) return hoursComparison;
        var minutesComparison = minutes.CompareTo(other.minutes);
        if (minutesComparison != 0) return minutesComparison;
        return seconds.CompareTo(other.seconds);
    }
    public static TimerTime operator +(TimerTime a, TimerTime b)
    {
        var hours = a.hours + b.hours;
        var minutes = a.minutes + b.minutes;
        var seconds = a.seconds + b.seconds;
        
        if (seconds >= 60)
        {
            minutes++;
            seconds -= 60;
        }

        if (minutes >= 60)
        {
            hours++;
            minutes -= 60;
        }
        
        return new TimerTime(hours, minutes, seconds);
    }
    public static TimerTime operator -(TimerTime a, TimerTime b)
    {
        var hours = a.hours - b.hours;
        var minutes = a.minutes - b.minutes;
        var seconds = a.seconds - b.seconds;
        
        if (seconds < 0)
        {
            minutes--;
            seconds += 60;
        }
        if (minutes < 0)
        {
            hours--;
            minutes += 60;
        }

        if (hours < 0) return new TimerTime(0, 0, 0);
        
        
        return new TimerTime(hours, minutes, seconds);
    }
    public static bool operator >(TimerTime a, TimerTime b)
    {
        return a.CompareTo(b) > 0;
    }
    public static bool operator <(TimerTime a, TimerTime b)
    {
        return a.CompareTo(b) < 0;
    }
    public static bool operator >=(TimerTime a, TimerTime b)
    {
        return a.CompareTo(b) >= 0;
    }
    public static bool operator <=(TimerTime a, TimerTime b)
    {
        return a.CompareTo(b) <= 0;
    }
    
}

public class SimpleTimer
{
    private TimerTime _startTime;
    private TimerTime _currentTime;
    private readonly TimerType _type;

    public bool timerStarted = false;
    public bool countdownFinished = false;
    public UnityEvent CountdownFinishedEvent = new();
    public UnityEvent HourEvent = new();
    public UnityEvent MinuteEvent = new();


    public SimpleTimer(TimerType type)
    {
        _type = type;
    }

    public void SetTimer(TimerTime timerTime, UnityEvent countdownFinishedEvent = null)
    {
        SetTimer(timerTime.hours, timerTime.minutes, timerTime.seconds, countdownFinishedEvent);
    }
    
    public void SetTimer(int hours, int minutes, float seconds, UnityEvent countdownFinishedEvent = null)
    {
        float sec = seconds % 60;
        int min = (minutes + Mathf.FloorToInt(seconds) / 60) % 60;
        int hr = hours + min / 60;

        _startTime = new(hr, min, sec);
        _currentTime = _startTime;

        countdownFinished = false;

        if (countdownFinishedEvent != null) CountdownFinishedEvent = countdownFinishedEvent;
    }

    public void UpdateTimer(float deltaTime)
    {
        Stopwatch(deltaTime);
        Countdown(deltaTime);
    }

    public void StopTimer() => timerStarted = false;
    public void StartTimer() => timerStarted = true;
    
    public void ResetTimer()
    {
        _currentTime = _startTime;
        countdownFinished = false;
        timerStarted = true;
    }

    public void ModifyTime(float seconds)
    {
        _currentTime.seconds += seconds;

        if (_currentTime.seconds >= 60)
        {
            _currentTime.minutes++;
            _currentTime.seconds = 0;
            MinuteEvent?.Invoke();
        }

        if (_currentTime.minutes >= 60)
        {
            _currentTime.hours++;
            _currentTime.minutes = 0;
            HourEvent?.Invoke();
        }

        if (_currentTime.seconds <= 0)
        {
            if (_currentTime.minutes <= 0)
            {
                if (_currentTime.hours <= 0)
                {
                    countdownFinished = true;
                    timerStarted = false;
                    CountdownFinishedEvent?.Invoke();
                    return;
                }

                _currentTime.hours--;
                _currentTime.minutes = 59;
                HourEvent?.Invoke();
            }

            _currentTime.minutes--;
            _currentTime.seconds = 59;
            MinuteEvent?.Invoke();
        }
    }


    private void Stopwatch(float deltaTime)
    {
        if (_type != TimerType.Stopwatch) return;
        if (!timerStarted) return;

        _currentTime.seconds += deltaTime;

        if (_currentTime.seconds >= 60)
        {
            _currentTime.minutes++;
            _currentTime.seconds = 0;
            MinuteEvent?.Invoke();
        }

        if (_currentTime.minutes >= 60)
        {
            _currentTime.hours++;
            _currentTime.minutes = 0;
            HourEvent?.Invoke();
        }
    }


    private void Countdown(float deltaTime)
    {
        if (_type != TimerType.Countdown) return;

        if (!timerStarted) return;
        if (countdownFinished) return;

        if (_currentTime.seconds <= 0)
        {
            if (_currentTime.minutes <= 0)
            {
                if (_currentTime.hours <= 0)
                {
                    countdownFinished = true;
                    timerStarted = false;
                    CountdownFinishedEvent?.Invoke();
                    return;
                }

                _currentTime.hours--;
                _currentTime.minutes = 60;
                HourEvent?.Invoke();
            }

            _currentTime.minutes--;
            _currentTime.seconds = 60;
            MinuteEvent?.Invoke();
        }

        _currentTime.seconds -= deltaTime;
    }


    public TimerTime ReadTime()
    {
        return _currentTime;
    }

    public float ReadTimeInSeconds()
    {
        return _currentTime.seconds + _currentTime.minutes * 60 + _currentTime.hours * 3600;
    }

    public string ReadTimeString(TimerDisplaySetting setting)
    {
        return setting switch
        {
            TimerDisplaySetting.HourMinSec => string.Format("{0:00}:{1:00}:{2:00}", _currentTime.hours,
                _currentTime.minutes, _currentTime.seconds),
            TimerDisplaySetting.HourMin => string.Format("{0:00}:{1:00}", _currentTime.hours, _currentTime.minutes),
            TimerDisplaySetting.MinSec => string.Format("{0:00}:{1:00}", _currentTime.minutes, _currentTime.seconds),
            _ => string.Format("{0:00}:{1:00}:{2:00}", _currentTime.hours, _currentTime.minutes, _currentTime.seconds),
        };
    }

    public void Reset()
    {
        _currentTime = _startTime;
    }
}