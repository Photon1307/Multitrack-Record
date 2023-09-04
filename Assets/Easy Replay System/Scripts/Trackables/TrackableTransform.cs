using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

namespace BaldAndBold
{
    public class TrackableTransform : Trackable
    {
        private Rigidbody rb;
        private bool wasKinematic = true;
        private bool wasDetectingCollisons = false;

        public class PosAndRot
        {
            public PosAndRot(Vector3 pos, Quaternion rot) { this.pos = pos; this.rot = rot; }
            public Vector3 pos { get; set; }
            public Quaternion rot { get; set; }
        }

        public void Start()
        {
            rb = this.gameObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                wasKinematic = rb.isKinematic;
                wasDetectingCollisons = rb.detectCollisions;
            }
        }

        public override void SetToIDLE()
        {
            //restore rigidbodies        
            RestoreInitialValues(true);
        }

        #region Record
        public override void PrepareForRecord()
        {
            //restore rigidbodies        
            RestoreInitialValues(true);
        }

        public override void RecordAt(float time)
        {
            //If the transform is the same as the last entry, modify the last entry time to match this one
            if (trackData.Count > 1 && trackData[trackData.Count - 1] != null && ((PosAndRot)trackData[trackData.Count - 1].data).pos == this.transform.position && ((PosAndRot)trackData[trackData.Count - 1].data).rot == this.transform.rotation)
            {
                trackData[trackData.Count - 1].time = time;
            }
            else
            {
                TrackDataEntry tde = new TrackDataEntry();
                tde.time = time;
                tde.data = new PosAndRot(this.transform.position, this.transform.rotation);
                trackData.Add(tde);
            }
        }
        #endregion

        #region playback
        public override void PrepareForPlayback()
        {
            //disable rigidbodies
            RestoreInitialValues(false);
        }

        public override void PlaybackAt(float time)
        {
            for (int i = 0; i < trackData.Count; i++)
            {
                TrackDataEntry currentDataEntry = trackData[i];
                if (currentDataEntry.time > time)
                {
                    TrackDataEntry previousDataEntry = (i > 0) ? trackData[i - 1] : currentDataEntry; //on first frame we use current data as previous                     
                    float timePortion = ((time - previousDataEntry.time) / (currentDataEntry.time - previousDataEntry.time));
                    transform.position = Vector3.Lerp(((PosAndRot)previousDataEntry.data).pos, ((PosAndRot)currentDataEntry.data).pos, timePortion);
                    transform.rotation = Quaternion.Lerp(((PosAndRot)previousDataEntry.data).rot, ((PosAndRot)currentDataEntry.data).rot, timePortion);
                    break;
                }
            }
        }
        #endregion

        #region seek
        public override void Seek(float time)
        {
            //first of all, if we have no trackData, return
            if (trackData.Count == 0)
            {
                return;
            }

            //first check if we are playbacking past the last track data time
            TrackDataEntry lastTrackData = trackData[trackData.Count - 1];
            if (time >= lastTrackData.time)
            {
                transform.position = ((PosAndRot)lastTrackData.data).pos;
                transform.rotation = ((PosAndRot)lastTrackData.data).rot;
            }
            else
            {
                PlaybackAt(time);
            }
        }
        #endregion

        private void RestoreInitialValues(bool value)
        {
            if (rb != null)
            {
                if (value)
                {
                    //restore initial values
                    rb.isKinematic = wasKinematic;
                    rb.detectCollisions = wasDetectingCollisons;
                }
                else
                {
                    //disable rigid bodies
                    rb.isKinematic = true;
                    rb.detectCollisions = false;
                }
            }
        }
    }
}