using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

namespace BaldAndBold
{
    public class TrackableSpriteRenderer : Trackable
    {
        private SpriteRenderer sr;
        private Sprite defaultSprite;
        private Animator animator;
        private bool animatorWasEnabled = true;

        // 改为二维列表以支持多轨道
        protected List<List<TrackDataEntry>> multiTrackData = new List<List<TrackDataEntry>>();

        public void Start()
        {
            sr = this.gameObject.GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogError("TrackableSpriteRenderer needs a SpriteRenderer component!");
                this.enabled = false;
                return;
            }

            defaultSprite = sr.sprite;

            animator = this.gameObject.GetComponent<Animator>();
            if (animator != null)
            {
                animatorWasEnabled = animator.enabled;
            }
        }

        public override void SetToIDLE()
        {
            //restore animator and sprite
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
            /* ----------- NEW ----------*/
            //while (multiTrackData.Count <= trackIndex)
            //{
            //    multiTrackData.Add(new List<TrackDataEntry>());
            //}

            //var trackData = multiTrackData[trackIndex];

            //If the sprite is the same as the last entry, modify the last entry time to match this one
            if (trackData.Count > 1 &&
                trackData[trackData.Count - 1] != null &&
                ((Sprite)trackData[trackData.Count - 1].data) == sr.sprite)
            {
                trackData[trackData.Count - 1].time = time;
            }
            else
            {
                TrackDataEntry tde = new TrackDataEntry();
                tde.time = time;
                tde.data = sr.sprite;
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
            /*
            //for (int i = lastTrackEntryPointer; i < trackData.Count; i++)
            for (int i = 0; i < trackData.Count; i++)
            {
                TrackDataEntry currentDataEntry = trackData[i];
                if (currentDataEntry.time > time)
                {
                    sr.sprite = (Sprite)currentDataEntry.data;
                    break;
                }
            }
            */

            /* --------- NEW --------*/

            for (int i = 0; i < multiTrackData.Count; i++)
            {
                for (int j = 0; j < multiTrackData[i].Count; j++)
                {
                    TrackDataEntry currentDataEntry = multiTrackData[i][j] ;
                    if (currentDataEntry.time > time)
                    {
                        sr.sprite = (Sprite)currentDataEntry.data;
                        break;
                    }
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
                sr.sprite = (Sprite)lastTrackData.data;
            }
            else
            {
                PlaybackAt(time);
            }
        }
        #endregion

        private void RestoreInitialValues(bool value)
        {
            if (animator != null)
            {
                if (value)
                {
                    //restore initial values
                    animator.enabled = animatorWasEnabled;
                    sr.sprite = defaultSprite;
                }
                else
                {
                    //disable animator
                    animator.enabled = false;
                }
            }
        }
    }
}