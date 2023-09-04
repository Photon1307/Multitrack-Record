using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Playables;
using UnityEngine.Timeline;

public class TimelinePlayer : MonoBehaviour
{
    public GameObject characterModel;
    public PlayableDirector director;
    private AnimationTrack animationTrack;
    private TimelineAsset timeline;
    private AnimationClip clip;

    private bool isRecording = false;

    public void Start()
    {
        timeline = ScriptableObject.CreateInstance<TimelineAsset>();
        animationTrack = timeline.CreateTrack<AnimationTrack>(null, "Model Animation");

        director.playableAsset = timeline;
        director.SetGenericBinding(animationTrack, characterModel.GetComponent<Animator>());

        clip = new AnimationClip();
        clip.legacy = true;
    }

    public void OnButtonClick()
    {
        if (isRecording)
        {
            // 停止录制
            StopRecording();
        }
        else
        {
            // 开始录制
            StartRecording();
        }
    }

    void StartRecording()
    {
        isRecording = true;
        Debug.Log("StartRecoding");
    }

    void StopRecording()
    {
        isRecording = false;
        animationTrack.CreateClip(clip);
        Debug.Log("StopRecoding");
    }

    void Update()
    {
        if (isRecording)
        {
            // 这里添加录制逻辑，例如将模型的位移信息添加到AnimationTrack
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(Time.time, characterModel.transform.position.x);
            clip.SetCurve("", typeof(Transform), "localPosition.x", curve);
            Debug.Log("Recoding in progress");
        }
    }


}