using System;
using System.Collections.Generic;
using UnityEngine;
public delegate void AudioEndHandler();
public delegate void AudioUpdateHandler<T>(T obj);
public class AudioCreator
{

    #region Variable
    private static List<AudioCreator> MyAudiosList = new List<AudioCreator>();

    private AudioUpdateHandler<float> UpdateEvent;

    private AudioEndHandler CompleteEvent;

    private bool _loop;

    private AudioSource _source;

    private AudioClip _clip;

    private string _flag;

    public static AudioManager manager = null;

    private float currentTime;

    private float passedTime;

    private float _clipLength;

    private bool _isFinish = false;

    private bool _isPause = false;

    private static bool showLog = true;
    #endregion


    #region 构造函数
    private AudioCreator(AudioClip clip, string flag = "",bool loop = false)
    {
        if (null == manager) manager = AudioManager.Instance;

        if (clip)
        {
            _source = new GameObject(string.Format("{0}", manager.AudioCounts)).AddComponent<AudioSource>();
            manager.AudioCounts++;
            _source.transform.parent = manager.transform;
            _flag = flag;
            _clip = clip;
            _source.clip = clip;
            _source.loop = loop;
            _loop = loop;
            _clipLength = clip.length;
            _source.playOnAwake = false;

            _source.Play();
        }

    }

    /// <summary>
    /// 请将Clip放在Resources/Audios下
    /// </summary>
    /// <param name="clipName"></param>
    /// <param name="loop"></param>
    private AudioCreator(string clipName,string flag = "", bool loop = false)
    {
        if (null == manager) manager = AudioManager.Instance;

        AudioClip clip = Resources.Load(string.Format("Audios/{0}", clipName)) as AudioClip;

        if (clip)
        {
            _source = new GameObject(string.Format("{0}", manager.AudioCounts)).AddComponent<AudioSource>();
            manager.AudioCounts++;
            _source.transform.parent = manager.transform;

            _flag = flag;
            _clip = clip;
            _source.clip = clip;
            _source.loop = loop;
            _loop = loop;
            _clipLength = clip.length;
            _source.playOnAwake = false;

            _source.Play();
        }
        else
        {
            Debug.LogError("音频路径错误！确认放在Resources/Audios下");
        }

    }
    #endregion


    #region 基本操作
    private void Pause()
    {
        if (_isFinish)
        {
            if (showLog) Debug.LogWarning("音频已经结束！");
        }
        else
        {
            _isPause = true;
            _source.Pause();
        }
    }

    private void Resum()
    {
        if (_isFinish)
        {
            if (showLog) Debug.LogWarning("音频已经结束！");
        }
        else
        {
            if (_isPause)
            {
                _source.UnPause();
                _isPause = false;
            }
            else
            {
                if (showLog) Debug.Log("当前音频正在播放！");
            }
        }
    }

    private void Update()
    {
        if (!_isFinish && !_isPause)
        {
            currentTime += Time.deltaTime;

            if (null != UpdateEvent) UpdateEvent(passedTime);

            if (currentTime >= _clipLength)
            {
                if (null != CompleteEvent) CompleteEvent();
                if (_loop)
                {
                    currentTime = 0;
                }
                else
                {
                    Stop();
                }
            }
        }
    }

    private void Stop()
    {
        if (MyAudiosList.Contains(this))
        {
            MyAudiosList.Remove(this);
            GameObject.Destroy(_source.gameObject);
        }

    }

    public static void UpdateAllAudio()
    {
        for (int i = 0; i < MyAudiosList.Count; i++)
        {
            if (null != MyAudiosList[i])
            {
                MyAudiosList[i].Update();
            }
        }
    }

    public static AudioCreator AddAudio(AudioClip clip,string flag = "", bool loop = false)
    {
        AudioCreator creator = new AudioCreator(clip,flag,loop);
        MyAudiosList.Add(creator);
        return creator;
    }

    /// <summary>
    /// 请将Clip放在Resources/Audios下
    /// </summary>
    /// <param name="clipName"></param>
    /// <param name="loop"></param>
    /// <returns></returns>
    public static AudioCreator PlayAudio(string clipName,string flag = "" ,bool loop = false)
    {
        AudioCreator creator = new AudioCreator(clipName,flag, loop);
        MyAudiosList.Add(creator);
        return creator;
    }

    public static bool Exist(string flag)
    {
        return MyAudiosList.Exists((v) => { return v._flag == flag; });
    }

    public static bool Exist(AudioCreator creator)
    {
        return MyAudiosList.Contains(creator);
    }

    public static AudioCreator GetAudio(string flag)
    {
        return MyAudiosList.Find((v) => { return v._flag == flag; });
    }

    public static void Pause(string flag)
    {
        AudioCreator creator = GetAudio(flag);
        if(null != creator)
        {
            creator.Pause();
        }
        else
        {
            Debug.Log("确认Audio存在！或者已播放完毕！");
        }
    }

    public static void Resum(string flag)
    {

        AudioCreator creator = GetAudio(flag);
        if (null != creator)
        {
            creator.Resum();
        }
        else
        {
            Debug.Log("确认Audio存在！或者已播放完毕！");
        }
    }

    public static void Resum(AudioCreator creator)
    {
        if (Exist(creator))
        {
            creator._source.UnPause();
        }
        else
        {
            Debug.Log("确认Audio存在！或者已播放完毕！");
        }
    }

    public static void Delete(string flag)
    {
        AudioCreator creator = GetAudio(flag);
        if(null != creator)
        {
            creator.Stop();
        }
        else
        {
            Debug.Log("确认Audio存在！或者已播放完毕！");
        }
    }

    public static void Delete(AudioCreator creator)
    {
        if (Exist(creator))
        {
            creator.Stop();
        }
        else
        {
            Debug.Log("确认Audio存在！或者已播放完毕！");
        }
    }

    public static void RemoveAll()
    {
        MyAudiosList.ForEach((item) => { item.Stop(); });
        MyAudiosList.Clear();
    }
    #endregion


    #region EventRegister
    public void AddEvent(AudioEndHandler completeEvent)
    {
        // GetInvocationList()必须有一个event才能调用，否则报错
        if(null == CompleteEvent)
        {
            CompleteEvent = completeEvent;
        }
        else
        {
            Delegate[] delegates = CompleteEvent.GetInvocationList();

            if (!Array.Exists(delegates, (item) => { return item == (Delegate)completeEvent; }))
            {
                CompleteEvent += completeEvent;
            }


        }
    }

    public void AddEvent(AudioUpdateHandler<float> updateEvent)
    {
        if (null == UpdateEvent)
        {
            UpdateEvent = updateEvent;
        }
        else
        {
            Delegate[] delegates = UpdateEvent.GetInvocationList();

            if (!Array.Exists(delegates, (item) => { return item == (Delegate)updateEvent; }))
            {
                UpdateEvent += UpdateEvent;
            }
        }
    }
    #endregion


}


public class AudioManager : Singleton<AudioManager> {
    [SerializeField]
    private int audioCounts = 0;
    public int AudioCounts { get { return audioCounts; } set { audioCounts = value; } }

    private void Update()
    {
        AudioCreator.UpdateAllAudio();
    }
}

public static class AudioExtension
{
    public static AudioCreator OnComplete(this AudioCreator creator,AudioEndHandler completeEvent)
    {
        if (null == creator)
            return null;
        creator.AddEvent(completeEvent);
        return creator;
    }

    public static AudioCreator OnUpdate(this AudioCreator creator, AudioUpdateHandler<float> updateEvent)
    {
        if (null == creator)
            return null;
        creator.AddEvent(updateEvent);
        return creator;
    }


}
