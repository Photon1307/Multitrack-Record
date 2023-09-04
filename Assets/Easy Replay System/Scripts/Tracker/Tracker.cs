using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

namespace BaldAndBold
{
    public class Tracker : MonoBehaviour
    {
        protected static Tracker instance;
        private static object _lock = new object();
        private static bool applicationIsQuitting = false;

        //Returns the instance of this singleton.
        public static Tracker Instance
        {
            get
            {

                if (applicationIsQuitting)
                {
                    //If application is quiting we won't create a new instance
                    return null;
                }

                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = (Tracker)FindObjectOfType(typeof(Tracker));
                        if (FindObjectsOfType(typeof(Tracker)).Length > 1)
                        {
                            //there should never be more than 1 singleton!
                            return instance;
                        }

                        if (instance == null)
                        {
                            GameObject singleton = new GameObject("ERS Tracker");
                            instance = singleton.AddComponent<Tracker>();

                            DontDestroyOnLoad(singleton);
                        }
                    }

                    return instance;
                }
            }
        }

        public float PlaybackSpeed
        {
            get
            {
                return playbackSpeed;
            }

            set
            {
                playbackSpeed = value;
            }
        }

        public float TotalTime
        {
            get
            {
                return totalTime;
            }

            set
            {
                totalTime = value;
            }
        }

        public float CurrentTime
        {
            get
            {
                return currentTime;
            }

            set
            {
                currentTime = value;
            }
        }

        public bool DebugMode = false;
        public enum TrackerState
        {
            IDLE = 0,
            RECORD,
            PLAYBACK,
            PAUSE,
            SEEK
        }

        /// <summary>
        /// Record started
        /// </summary>
        public static UnityEvent onRecordStarted = new UnityEvent();

        /// <summary>
        /// Record finished
        /// </summary>
        public static UnityEvent onRecordFinished = new UnityEvent();

        /// <summary>
        /// Playback started
        /// </summary>
        public static UnityEvent onPlaybackStarted = new UnityEvent();

        /// <summary>
        /// Playback stopped, but not at end of the track
        /// </summary>
        public static UnityEvent onPlaybackStopped = new UnityEvent();

        /// <summary>
        /// Playback finished playing, paused at the end of the track
        /// </summary>
        public static UnityEvent onPlaybackFinished = new UnityEvent();

        /// <summary>
        /// Add new track when add track button clicked
        /// </summary>
        public static UnityEvent onAddTrackButtonClicked = new UnityEvent();

        private TrackerState currentState = TrackerState.IDLE;
        private List<Trackable> trackables = new List<Trackable>();
        private float currentTime = 0.0f;
        private float totalTime = 0.0f;
        public float maxRecordTime = 5.0f;

        public static float FPS_10 = 0.1f;      //10 FPS 
        public static float FPS_30 = 0.033f;    //30 FPS
        public static float FPS_60 = 0.017f;    //60 FPS

        private float targetRecordingRate = FPS_10;
        private float remainingKeyframeTime = 0;

        private float playbackSpeed = 1.0f;

        /*---------- Track Control ---------*/
        public int numOfTracks = 0;
        private int currentTrackIndex = 0;
        //private List<List<Trackable>> multiTrackData = new List<List<Trackable>>(); // 存储多个轨道的数据
        public List<int> activeTrackIndices = new List<int>();

        //Do recording and playing after physics simulation
        void LateUpdate()
        {
            switch (currentState)
            {
                case TrackerState.IDLE:
                case TrackerState.SEEK:
                case TrackerState.PAUSE:
                    //nothing to do here
                    break;
                case TrackerState.RECORD:
                    //
                    //
                    UpdateRecord();
                    break;
                case TrackerState.PLAYBACK:
                    UpdatePlayback();
                    break;
                default:
                    Debug.LogError("Unknown Tracker State: " + currentState);
                    break;
            }
        }

        private void UpdateRecord()
        {
            currentTime += Time.deltaTime;
            totalTime = currentTime; //total time equals current time whilst recording
            remainingKeyframeTime -= Time.deltaTime;

            if (remainingKeyframeTime > 0)
                return;

            // 检查是否达到最大录制时间
            if (totalTime >= maxRecordTime)
            {
                // 结束录制
                Stop();
                TrackableManager.Instance.AllTrackList[currentTrackIndex].isRecorded = true;
                onRecordFinished.Invoke();
                return;
            }

            //if (DebugMode) Debug.Log("Recording (" + currentTime + ")");

            Track currentTrack = TrackableManager.Instance.AllTrackList[currentTrackIndex];
            foreach (Trackable t in currentTrack.trackables)
            {
                t.RecordAt(currentTime);
            }

            remainingKeyframeTime += targetRecordingRate;
        }

        private void UpdatePlayback()
        {
            currentTime += (Time.deltaTime * playbackSpeed);
            currentTime = Mathf.Clamp(currentTime, 0, totalTime);

            foreach (Track track in TrackableManager.Instance.AllTrackList)
            {
                // 只有当轨道已经录制完毕时，才进行回放
                if (track.isRecorded == true)
                {
                    foreach (Trackable t in track.trackables)
                    {
                        t.PlaybackAt(currentTime);
                    }
                }

            }


            //finished playback?
            if (currentTime >= totalTime)
            {
                //pause at end
                currentTime = totalTime;
                SetState(TrackerState.PAUSE);
            }
        }

        public void SetState(TrackerState newState)
        {
            if (currentState == newState)
                return;

            //===========If switch from recording to idle, set the track state isRecoding = true. =======
            if (currentState == TrackerState.RECORD && newState == TrackerState.IDLE)
            {
                Track currentTrack = TrackableManager.Instance.AllTrackList[currentTrackIndex];
                currentTrack.isRecorded = true;
                currentTrack.recordDuration = totalTime;
                Debug.Log("Current Track status isRecording =  " + currentTrack.isRecorded + "\n" + " , Current Track duration: " + currentTrack.recordDuration);
            }
            //===========================================================================================

            switch (newState)
            {
                case TrackerState.IDLE:
                    SetToIDLE(); // stop everything
                    break;
                case TrackerState.PAUSE:
                    //do nothing whilst paused
                    break;
                case TrackerState.RECORD:
                    PrepareForRecord();
                    break;
                case TrackerState.PLAYBACK:
                case TrackerState.SEEK:
                    PrepareForPlayback();
                    break;
                default:
                    Debug.LogError("Unknown Tracker State: " + newState);
                    return;
            }

            TrackerState currentStateCopy = currentState;

            currentState = newState;

            //launch events if needed (MUST be done in the end, to avoid events to change the state and keep previous state)
            LaunchEvents(currentStateCopy, newState);
        }

        /// <summary>
        /// Add trackables into track at here
        /// This function is invoked by Trackable.cs, when the trackables are firstly intialized in the game
        /// NEED RE-write when finishing hierachical mapping function
        /// </summary>
        /// <param name="t"></param>
        public void AddTrackable(Trackable t)
        {
            //if (!TrackableManager.Instance.individualTrackList[currentTrackIndex].Contains(t))
            //    TrackableManager.Instance.individualTrackList[currentTrackIndex].Add(t);
            Track currentTrack = TrackableManager.Instance.AllTrackList[currentTrackIndex];
            if (!currentTrack.trackables.Contains(t))
            {
                currentTrack.trackables.Add(t);
            }

        }

        public void RemoveTrackable(Trackable t)
        {
            if (trackables.Contains(t))
                trackables.Remove(t);
        }

        private void SetToIDLE()
        {
            if (DebugMode && trackables.Count > 0)
            {
                Debug.Log(GetDebugInfo());
            }

            currentTime = 0;

            foreach (Trackable t in trackables)
            {
                t.SetToIDLE();
            }
        }

        private void PrepareForRecord()
        {
            currentTime = 0;
            totalTime = 0;
            remainingKeyframeTime = 0;

            foreach (Trackable t in trackables)
            {
                t.ClearTrackData();
                t.PrepareForRecord();
            }
        }

        private void PrepareForPlayback()
        {
            //currentTime = 0;
            foreach (Trackable t in trackables)
            {
                t.PrepareForPlayback();
            }
        }

        public string GetDebugInfo()
        {
            string debugInfo = "";
            debugInfo += "TOTAL TRACKABLES: " + trackables.Count + "\n";
            debugInfo += "TIME RECORDED: " + (totalTime).ToString("0.00") + "s \n";

            return debugInfo;
        }

        /// <summary>
        /// sets the time where the track should be (seeks) and stops there
        /// </summary>
        /// <param name="time"></param>
        public void Seek(float time)
        {
            if (currentState != TrackerState.SEEK)
            {
                SetState(TrackerState.SEEK);
            }

            if (time > totalTime)
            {
                time = totalTime;
            }

            currentTime = time;
            foreach (Trackable t in trackables)
            {
                t.Seek(currentTime);
            }
        }

        /// <summary>
        /// Seeks to the beginning of the track
        /// </summary>
        public void SeekToStart()
        {
            Seek(0);
        }

        /// <summary>
        /// Seeks to end of the track
        /// </summary>
        public void SeekToEnd()
        {
            Seek(totalTime);
        }

        /// <summary>
        /// When Unity quits, it destroys objects in a random order.
        /// In principle, a Singleton is only destroyed when application quits.
        /// If any script calls Instance after it have been destroyed, 
        ///   it will create a buggy ghost object that will stay on the Editor scene
        ///   even after stopping playing the Application. Really bad!
        /// So, this was made to be sure we're not creating that buggy ghost object.
        /// </summary>
        public void OnDestroy()
        {
            applicationIsQuitting = true;
        }


        private void LaunchEvents(TrackerState previousState, TrackerState newState)
        {
            //record started?
            if (onRecordStarted != null
                && previousState != TrackerState.RECORD
                && newState == TrackerState.RECORD)
            {
                onRecordStarted.Invoke();
            }
            //playback started?
            else if (onPlaybackStarted != null
                 && previousState != TrackerState.PLAYBACK
                 && newState == TrackerState.PLAYBACK)
            {
                onPlaybackStarted.Invoke();
            }


            //record finished?
            if (onRecordFinished != null
                && previousState == TrackerState.RECORD
                && newState == TrackerState.IDLE)
            {
                onRecordFinished.Invoke();
            }
            //playback finished?
            else if (onPlaybackFinished != null
                && previousState == TrackerState.PLAYBACK
                && newState == TrackerState.PAUSE)
            {
                onPlaybackFinished.Invoke();
            }

            //playback stopped?
            if (onPlaybackStopped != null
                && previousState != TrackerState.IDLE
                && previousState != TrackerState.RECORD
                && newState == TrackerState.IDLE)
            {
                onPlaybackStopped.Invoke();
            }
        }

        /// <summary>
        /// Records at a given rate. Default is 0.1 (FPS_10). 
        /// But for non interpolatable trackables you may shorten it. eg: 0.033(FPS_30) or 0.017(FPS_60)
        /// Use sparingly because it will affect performance
        /// </summary>
        /// <param name="recordingRate"></param>
        public void Record(float recordingRate)
        {
            this.targetRecordingRate = recordingRate;
            Record();
        }

        public void Record()
        {
            //SetState(TrackerState.RECORD);
            //// Start recording into the specified track
            //foreach (Trackable t in TrackableManager.Instance.individualTrackList[currentTrackIndex])
            //{
            //    t.PrepareForRecord();
            //    Debug.Log("Record() invoke, multiTrackData[" + currentTrackIndex + "]");
            //}

            SetState(TrackerState.RECORD);
            Track currentTrack = TrackableManager.Instance.AllTrackList[currentTrackIndex];
            currentTrack.currentTrackState = Track.TrackState.RECORD;
            foreach (Trackable t in currentTrack.trackables)
            {
                t.PrepareForRecord();
            }
            Debug.Log("Record() invoke, AllTrackList[" + currentTrackIndex + "]");
        }

        public void Record(int trackIndex)
        {
            currentTrackIndex = trackIndex;
            activeTrackIndices.Clear();
            activeTrackIndices.Add(currentTrackIndex);
            Record();
        }


        public void Play()
        {
            //SetState(TrackerState.PLAYBACK);
            ///* --------- Start playback from the specified track --------*/
            //activeTrackIndices.Clear();
            //for (int i = 0; i < TrackableManager.Instance.individualTrackList.Count; i++)
            //{
            //    activeTrackIndices.Add(i); // 默认回放所有轨道
            //    foreach(Trackable t in TrackableManager.Instance.individualTrackList[i])
            //    {
            //        t.PrepareForPlayback();
            //    }
            //}
            SetState(TrackerState.PLAYBACK);
            activeTrackIndices.Clear();
            for (int i = 0; i < TrackableManager.Instance.AllTrackList.Count; i++)
            {
                Track currentTrack = TrackableManager.Instance.AllTrackList[i];
                if (currentTrack.isRecorded)
                {
                    activeTrackIndices.Add(i);
                    currentTrack.currentTrackState = Track.TrackState.PLAYBACK;
                    foreach (Trackable t in currentTrack.trackables)
                    {
                        t.PrepareForPlayback();
                    }
                }
            }

        }

        public void Play(float speed)
        {
            PlaybackSpeed = speed;
            SetState(TrackerState.PLAYBACK);
        }

        public void Pause()
        {
            SetState(TrackerState.PAUSE);
        }

        public void Stop()
        {
            //stop while not recording seeks to start
            if (!IsRecording())
            {
                SeekToStart();
            }

            SetState(Tracker.TrackerState.IDLE);
        }

        public bool IsPlaying()
        {
            return currentState == TrackerState.PLAYBACK;
        }

        public bool IsPaused()
        {
            return currentState == TrackerState.PAUSE;
        }

        public bool IsRecording()
        {
            return currentState == TrackerState.RECORD;
        }


        public void AddNewTrack()
        {
            numOfTracks++;
            //TrackableManager.Instance.individualTrackList.Add(new List<Trackable>());
        }

        public void SetCurrentTrackIndex(int trackIndex)
        {
            currentTrackIndex = trackIndex;
        }


    }
}
