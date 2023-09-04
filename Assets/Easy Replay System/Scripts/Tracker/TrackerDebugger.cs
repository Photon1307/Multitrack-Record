using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BaldAndBold
{
    public class TrackerDebugger : MonoBehaviour
    {
        [Header("Debug Text")]
        public Text trackerStatus;
        public Text lastRecordInfo;

        [Header("Buttons")]
        public Button playButton;
        public Button pauseButton;
        public Button recordButton;
        public Button stopButton;

        [Header("Seek Slider UI")]
        public Slider SeekSlider;
        public Text currentSpeed;
        public Text currentSeekTime;
        public Text finalSeekTime;

        [Header("Add Track")]
        public Button addTrack;
        public ToggleGroup toggleTrack;
        public GameObject newTrack;
        public Transform trackContainer;

        [Header("For Trackables Mapping Selection")]
        // ---------Trackables Selection ------
        public GameObject togglePrefab;
        public Transform contentPanel;
        public GameObject trackableListCanvas;
        //--------------------------------------

        private float playbackSpeed = 1.0f;

        public int currentTrackIndex = 0;


        void Awake()
        {
            pauseButton.gameObject.SetActive(false);
            stopButton.gameObject.SetActive(false);
            playButton.interactable = false;
            SeekSlider.interactable = false;
            addTrack.onClick.AddListener(AddNewTrack);
        }

        public void SetPlaySpeed(float value)
        {
            playbackSpeed = (float)(Math.Truncate((double)value * 100.0) / 100.0);
            currentSpeed.text = "" + playbackSpeed;

            //Update tracker playbackspeed 
            Tracker.Instance.PlaybackSpeed = playbackSpeed;
        }

        public void Record()
        {
            Tracker.Instance.Record(currentTrackIndex);
            trackerStatus.text = "RECORDING";
        }

        private void onRecordStarted()
        {
            //set buttons states
            recordButton.gameObject.SetActive(false);
            pauseButton.gameObject.SetActive(false);

            playButton.gameObject.SetActive(true);
            playButton.interactable = false;

            stopButton.gameObject.SetActive(true);
            stopButton.interactable = true;

            SeekSlider.interactable = false;
        }

        public void Play()
        {
            Tracker.Instance.Play();
            trackerStatus.text = "REPLAYING";
        }

        private void onPlaybackStarted()
        {
            //set buttons states
            playButton.gameObject.SetActive(false);
            recordButton.gameObject.SetActive(false);

            stopButton.gameObject.SetActive(true);
            stopButton.interactable = true;

            pauseButton.gameObject.SetActive(true);
            pauseButton.interactable = true;
        }

        public void Seek(float timePercentage)
        {
            if (!Tracker.Instance.IsPlaying() && !Tracker.Instance.IsRecording())
            {
                float seekTime = Tracker.Instance.TotalTime * timePercentage;
                Tracker.Instance.Seek(seekTime);
                currentSeekTime.text = (Tracker.Instance.CurrentTime).ToString("0.00");
                trackerStatus.text = "SEEKING";
            }
        }


        public void Pause()
        {
            Tracker.Instance.Pause();
            trackerStatus.text = "PAUSED";

            //set buttons states            
            pauseButton.gameObject.SetActive(false);
            recordButton.gameObject.SetActive(false);

            playButton.gameObject.SetActive(true);
            playButton.interactable = true;

            stopButton.gameObject.SetActive(true);
            stopButton.interactable = true;
        }

        public void Stop()
        {
            //stop while not recording? reset seektime values
            if (!Tracker.Instance.IsRecording())
            {
                currentSeekTime.text = "0";
                //SeekSlider.value = 0;
            }
            Tracker.Instance.Stop();

            trackerStatus.text = "IDLE";
        }

        private void onRecordFinished()
        {
            finalSeekTime.text = (Tracker.Instance.TotalTime).ToString("0.00");
            lastRecordInfo.text = Tracker.Instance.GetDebugInfo();

            Seek(0);

            EnablePlayAndRecordButtons();

            SeekSlider.interactable = true;

            Debug.Log("============RECORD STOPPED==============");
        }

        private void onPlaybackFinished()
        {
            EnablePlayAndRecordButtons();
        }

        private void onPlaybackStopped()
        {
            EnablePlayAndRecordButtons();

            //SeekSlider.interactable = true;
        }

        private void EnablePlayAndRecordButtons()
        {
            //set buttons states
            stopButton.gameObject.SetActive(false);
            pauseButton.gameObject.SetActive(false);

            playButton.gameObject.SetActive(true);
            playButton.interactable = true;

            recordButton.gameObject.SetActive(true);
            recordButton.interactable = true;
        }

        private void onAddTrackButtonClicked()
        {
            Tracker.Instance.AddNewTrack();
        }


        /* ------------ TRACK Buttons ------------*/
        private void AddNewTrack()
        {
            onAddTrackButtonClicked();

            GameObject newTrackButton = Instantiate(newTrack, trackContainer);
            int newTrackIndex = trackContainer.childCount - 1;

            TrackInitialize(newTrackIndex);

            Text buttonText;
            buttonText = newTrackButton.GetComponentInChildren<Text>();
            buttonText.fontStyle = FontStyle.Bold;
            buttonText.text = "Track " + newTrackIndex;

            Toggle newToggle = newTrackButton.GetComponent<Toggle>();
            newTrackButton.GetComponent<Toggle>().onValueChanged.AddListener((isOn) => SelectOnTrack(newTrackIndex, isOn));


            if (newToggle != null)
            {
                newToggle.group = toggleTrack;
                newToggle.isOn = true;
                newToggle.Select();
                //SelectOnTrack(newTrackIndex);
            }
        }

        private void SelectOnTrack(int trackIndex, bool isOn)
        {
            if(isOn == true)
            {
                currentTrackIndex = trackIndex;
                Debug.Log("SELECT ON TRACK: CURRENT INDEX: " + currentTrackIndex);
            }
            
        }

        private void TrackInitialize(int trackIndex)
        {
            //Define mapping relationship
            //And store the desired trackables into current track

            //TrackableManager.Instance.individualTrackList.Add(new List<Trackable>());
            //DisplayTrackables(trackIndex);
            Track newTrack = new Track(new List<Trackable>());
            TrackableManager.Instance.AllTrackList.Add(newTrack);
            DisplayTrackables(trackIndex);
        }

        private void DisplayTrackables(int trackIndex)
        {
            Debug.Log("==========DISPLAY TRACKABLES==========");
            ClearContentList();
            trackableListCanvas.SetActive(true);
            //Display all trackables for selection.
            //NEEDS TO RE-WRITE AFTER INTEGRATING WITH MAPPING
            for (int i = 0; i < TrackableManager.Instance.fullTrackableList.Count; i++)
            {
                Trackable trackable = TrackableManager.Instance.fullTrackableList[i];
                GameObject newToggle = Instantiate(togglePrefab) as GameObject;
                newToggle.transform.SetParent(contentPanel, false);

                Toggle toggle = newToggle.GetComponent<Toggle>();
                toggle.onValueChanged.AddListener((isOn) => OnToggleChanged(isOn, trackable, trackIndex));

                Text label = newToggle.GetComponentInChildren<Text>();
                label.text = trackable.gameObject.name;
            }
        }

        private void ClearContentList()
        {
            foreach (Transform child in contentPanel)
            {
                Destroy(child.gameObject);
            }
        }

        void OnToggleChanged(bool isOn, Trackable trackable, int trackIndex)
        {
            Track currentTrack = TrackableManager.Instance.AllTrackList[trackIndex];

            if (isOn)
            {
                currentTrack.trackables.Add(trackable);
                //TrackableManager.Instance.individualTrackList[trackIndex].Add(trackable);
            }
            else
            {
                currentTrack.trackables.Remove(trackable);
                //TrackableManager.Instance.individualTrackList[trackIndex].Remove(trackable);
            }
        }

        void Update()
        {
            if (Tracker.Instance.IsPlaying())
            {
                currentSeekTime.text = (Tracker.Instance.CurrentTime).ToString("0.00");

                float timePercentage = Tracker.Instance.CurrentTime / Tracker.Instance.TotalTime;
                SeekSlider.value = timePercentage;
            }
            else if (Tracker.Instance.IsRecording())
            {
                SeekSlider.value = 0;
                finalSeekTime.text = (Tracker.Instance.CurrentTime).ToString("0.00");
            }
        }

        void OnEnable()
        {
            Tracker.onRecordFinished.AddListener(onRecordFinished);
            Tracker.onRecordStarted.AddListener(onRecordStarted);
            Tracker.onPlaybackStarted.AddListener(onPlaybackStarted);
            Tracker.onPlaybackFinished.AddListener(onPlaybackFinished);
            Tracker.onPlaybackStopped.AddListener(onPlaybackStopped);
            Tracker.onAddTrackButtonClicked.AddListener(onAddTrackButtonClicked);
        }

        void OnDisable()
        {
            //remember to free event listeners when leaving to avoid null pointer exceptions
            Tracker.onRecordFinished.RemoveListener(onRecordFinished);
            Tracker.onRecordStarted.RemoveListener(onRecordStarted);
            Tracker.onPlaybackStarted.RemoveListener(onPlaybackStarted);
            Tracker.onPlaybackFinished.RemoveListener(onPlaybackFinished);
            Tracker.onPlaybackStopped.RemoveListener(onPlaybackStopped);
            Tracker.onAddTrackButtonClicked.RemoveListener(onAddTrackButtonClicked);
        }

    }
}