using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BaldAndBold
{
    [DefaultExecutionOrder(-100)]
    public class TrackableManager : MonoBehaviour
    {
        public static TrackableManager Instance;
        public List<Trackable> fullTrackableList = new List<Trackable>();
        //public List<List<Trackable>> individualTrackList = new List<List<Trackable>>();

        //====For debug use=====
        [SerializeField]
        public List<Track> AllTrackList;
        //========================

        void Awake()
        {
            Debug.Log("TrackableManager Awake called.");

            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void AddTrackable(Trackable trackable)
        {
            if (!fullTrackableList.Contains(trackable))
            {
                fullTrackableList.Add(trackable);
            }
        }

        public void RemoveTrackable(Trackable trackable)
        {
            if (fullTrackableList.Contains(trackable))
            {
                fullTrackableList.Remove(trackable);
            }
        }

        public void AddTrackableIntoTrack(Trackable trackable, int trackIndex)
        {

        }

    }

}
