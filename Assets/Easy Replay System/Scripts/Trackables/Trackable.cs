using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace BaldAndBold
{
    public abstract class Trackable : MonoBehaviour
    {
        public class TrackDataEntry
        {
            public float time { get; set; }
            public object data { get; set; }
        }

        protected List<TrackDataEntry> trackData = new List<TrackDataEntry>();

        //support multiple tracks of animation
        protected List<List<TrackDataEntry>> multiTrackData = new List<List<TrackDataEntry>>();

        public abstract void SetToIDLE();
        public abstract void PrepareForRecord();
        public abstract void PrepareForPlayback();
        public abstract void RecordAt(float time);
        public abstract void PlaybackAt(float time);
        public abstract void Seek(float time);

        public virtual List<TrackDataEntry> GetTrackData()
        {
            return trackData;
        }

        public virtual void ClearTrackData()
        {
            trackData.Clear();
        }

        public virtual void OnEnable()
        {
            //Register this trackable in the trackable manager list
            //Tracker.Instance.AddTrackable(this);
            TrackableManager.Instance.AddTrackable(this);

        }

        public virtual void OnDisable()
        {
            Tracker t = Tracker.Instance; //if application is quitting Instance will be null and would let a zombie "Tracker" on the scene if we continue (See Singleton.cs)
            if (t != null) { t.RemoveTrackable(this); }
        }

    }
}