using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BaldAndBold
{
    public class Track : MonoBehaviour
    {
        public bool isRecorded { get; set; }
        public float recordDuration { get; set; }
        public TrackState currentTrackState { get; set; }

        public List<Trackable> trackables;

        public enum TrackState
        {
            IDLE = 0,
            RECORD,
            PLAYBACK,
            PAUSE,
            SEEK
        }

        public Track(List<Trackable> trackables, bool isRecorded = false, float recordDuration = 0.0f, TrackState currentTrackState = TrackState.IDLE)
        {
            this.isRecorded = false;
            this.recordDuration = 0.0f;
            this.currentTrackState = TrackState.IDLE;
            this.trackables = trackables;
        }
    }
}
