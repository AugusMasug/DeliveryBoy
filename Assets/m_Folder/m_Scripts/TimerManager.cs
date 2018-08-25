using System;
using System.Collections.Generic;
using UnityEngine;
public delegate void TimerUpdateHandler<T>(T obj);
public delegate void TimerEndHandler();
public class Timer
{

    #region Variable
    /// <summary>
    /// 所有的计时器
    /// </summary>
    private static List<Timer> MyTimers = new List<Timer>();

    /// <summary>
    /// 帧事件
    /// </summary>
    private TimerUpdateHandler<float> UpdateEvent;

    /// <summary>
    /// 结束事件
    /// </summary>
    private TimerEndHandler CompleteEvent;

    /// <summary>
    /// 时长
    /// </summary>
    private float _time;
    /// <summary>
    /// 循环
    /// </summary>
    private bool _loop;

    /// <summary>
    /// 计时器标志
    /// </summary>
    private string _flag;

    public static TimerManager driver = null;

    /// <summary>
    /// 当前时间
    /// </summary>
    private float CurrentTime { get { return Time.realtimeSinceStartup; } }
    /// <summary>
    /// 缓存时间,暂停使用
    /// </summary>
    private float cachedTime;
    /// <summary>
    /// 经历时间
    /// </summary>
    float timePassed;
    /// <summary>
    /// 是否结束
    /// </summary>
    private bool _isFinish = false;
    /// <summary>
    /// 是否暂停
    /// </summary>
    private bool _isPause = false;

    /// <summary>
    /// Log信息
    /// </summary>
    private static bool showLog = true;


    /// <summary>
    /// 当前计时器时间
    /// </summary>
    public float Duration { get { return _time; } }
    #endregion


    private Timer(float time, string flag, bool loop = false)
    {
        if (null == driver) driver = TimerManager.Instance;
        _time = time;
        _loop = loop;
       
        cachedTime = CurrentTime;
        if (MyTimers.Exists((v) => { return v._flag == flag; }))
        {
            if (showLog)
                Debug.LogWarningFormat("存在相同计时器：{0}", flag);
        }
        _flag =  flag;
    }


    #region Pause,Resum,Update,Stop
    /// <summary>  
    /// 暂停
    /// </summary>  
    private void Pause()
    {
        if (_isFinish)
        {
            if (showLog) Debug.LogWarning("计时已经结束！");
        }
        else
        {
            _isPause = true;
        }
    }
    /// <summary>  
    /// 继续 
    /// </summary>  
    private void Resum()
    {
        if (_isFinish)
        {
            if (showLog) Debug.LogWarning("计时已经结束！");
        }
        else
        {
            if (_isPause)
            {
                cachedTime = CurrentTime - timePassed;
                _isPause = false;
            }
            else
            {
                if (showLog) Debug.LogWarning("当前计时非暂停状态！");
            }
        }
    }

    /// <summary>
    /// 刷新
    /// </summary>
    private void Update()
    {
        if (!_isFinish && !_isPause) 
        {
            timePassed = CurrentTime - cachedTime;

            if (null != UpdateEvent) UpdateEvent.Invoke(Mathf.Clamp01(timePassed / _time));
            if (timePassed >= _time)
            {
                if (null != CompleteEvent) CompleteEvent();
                if (_loop)
                {
                    cachedTime = CurrentTime;
                }
                else
                {
                    Stop();
                }
            }
        }
    }

    /// <summary>
    /// 停止并初始化
    /// </summary>
    private void Stop()
    {
        if (MyTimers.Contains(this))
        {
            MyTimers.Remove(this);
        }
        _time = -1;
        _isFinish = true;
        _isPause = false;
        UpdateEvent = null;
        CompleteEvent = null;
    }
    #endregion


    #region 增删查改
    /// <summary>
    /// 添加
    /// </summary>
    /// <param name="time"></param>
    /// <param name="flag"></param>
    /// <param name="loop"></param>
    /// <returns></returns>
    public static Timer AddTimer(float time, string flag = "", bool loop = false)
    {
        Timer timer = new Timer(time, flag, loop);
        MyTimers.Add(timer);
        return timer;
    }

    /// <summary>
    /// 驱动计时器
    /// </summary>
    public static void UpdateAllTimer()
    {
        for (int i = 0; i < MyTimers.Count; i++)
        {
            if (null != MyTimers[i])
            {
                MyTimers[i].Update();
            }
        }
    }

    /// <summary>
    /// 是否存在计时器
    /// </summary>
    /// <param name="flag"></param>
    public static bool Exist(string flag)
    {
        return MyTimers.Exists((v) => { return v._flag == flag; });
    }
    /// <summary>
    /// 是否存在计时器
    /// </summary>
    /// <param name="flag"></param>
    public static bool Exist(Timer timer)
    {
        return MyTimers.Contains(timer);
    }

    /// <summary>
    /// 获取
    /// </summary>
    /// <param name="flag"></param>
    public static Timer GetTimer(string flag)
    {
        return MyTimers.Find((v) => { return v._flag == flag; });
    }


    /// <summary>
    /// 暂停
    /// </summary>
    /// <param name="flag"></param>
    public static void Pause(string flag)
    {
        Timer timer = GetTimer(flag);
        if (null != timer)
        {
            timer.Pause();
        }
        else
        {
            if (showLog) Debug.LogFormat("检查此计时器：{0}是否存在！或者已经完成计时!" + flag);
        }
    }
    /// <summary>
    /// 暂停
    /// </summary>
    /// <param name="timer"></param>
    public static void Pause(Timer timer)
    {
        if (Exist(timer))
        {
            timer.Pause();
        }
        else
        {
            if (showLog) Debug.LogFormat("检查此计时器是否存在！或者已经完成计时!");
        }
    }
    /// <summary>
    /// 恢复
    /// </summary>
    /// <param name="flag"></param>
    public static void Resum(string flag)
    {
        Timer timer = GetTimer(flag);
        if (null != timer)
        {
            timer.Resum();
        }
        else
        {
            if (showLog) Debug.LogFormat("检查此计时器：{0}是否存在！或者已经完成计时!", flag);
        }
    }
    /// <summary>
    /// 恢复
    /// </summary>
    /// <param name="timer"></param>
    public static void Resum(Timer timer)
    {
        if (Exist(timer))
        {
            timer.Resum();
        }
        else
        {
            if (showLog) Debug.LogFormat("检查此计时器是否存在！或者已经完成计时!");
        }
    }


    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="flag"></param>
    public static void Delete(string flag)
    {
        Timer timer = GetTimer(flag);
        if (null != timer)
        {
            timer.Stop();
        }
        else
        {
            if (showLog) Debug.LogFormat("检查此计时器是否存在！或者已经完成计时!");
        }
    }
    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="flag"></param>
    public static void Delete(Timer timer)
    {
        if (Exist(timer))
        {
            timer.Stop();
        }
        else
        {
            if (showLog) Debug.LogFormat("检查此计时器是否存在！或者已经完成计时!");
        }
    }


    /// <summary>
    /// 删除所有计时器
    /// </summary>
    public static void RemoveAll()
    {
        MyTimers.ForEach((v) => { v.Stop(); });
        MyTimers.Clear();
    }
    #endregion


    #region EventRegister
    /// <summary>
    /// 添加结束事件
    /// </summary>
    /// <param name="completedEvent"></param>
    public void AddEvent(TimerEndHandler completedEvent)
    {
        if (null == CompleteEvent)
        {
            CompleteEvent = completedEvent;
        }
        else
        {
            //防止多次注册同一事件
            Delegate[] delegates = CompleteEvent.GetInvocationList();
            if (!Array.Exists(delegates, (v) => { return v == (Delegate)completedEvent; }))
            {
                CompleteEvent += completedEvent;
            }
        }
    }
    public void AddEvent(TimerUpdateHandler<float> updateEvent)
    {
        if (null == UpdateEvent)
        {
            UpdateEvent = updateEvent;
        }
        else
        {
            Delegate[] delegates = UpdateEvent.GetInvocationList();

            if (!Array.Exists(delegates, (v) => { return v == (Delegate)updateEvent; }))
            {
                UpdateEvent += updateEvent;
            }
        }
    }
    #endregion
}

/// <summary>
/// 创建一个GameObject挂载脚本驱动计时器
/// </summary>
public class TimerManager : Singleton<TimerManager>
{
    void Update()
    {
        //每帧驱动
        Timer.UpdateAllTimer();
    }
}

/// <summary>
/// 扩展，链式编程
/// </summary>
public static class TimerExtension
{
    /// <summary>
    /// 结束事件 
    /// </summary>
    /// <param name="timer"></param>
    /// <param name="completedEvent"></param>
    /// <returns></returns>
    public static Timer OnComplete(this Timer timer, TimerEndHandler completedEvent)
    {
        if (null == timer)
        {
            return null;
        }
        timer.AddEvent(completedEvent);
        return timer;
    }
    /// <summary>
    /// 帧事件
    /// </summary>
    /// <param name="timer"></param>
    /// <param name="updateEvent"></param>
    /// <returns></returns>
    public static Timer OnUpdate(this Timer timer, TimerUpdateHandler<float> updateEvent)
    {
        if (null == timer)
        {
            return null;
        }
        timer.AddEvent(updateEvent);
        return timer;
    }
}