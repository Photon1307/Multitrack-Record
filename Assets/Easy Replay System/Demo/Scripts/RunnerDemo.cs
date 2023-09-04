using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using BaldAndBold;

public class RunnerDemo : MonoBehaviour
{

    public Image powerBar;
    public Text timeText;
    public Text instructions;
    public Animator runner;
    public Transform background;

    public Transform startRecordingPoint;
    public Transform finishLinePoint;

    public Image recImage;
    public Image playImage;

    public GameObject restartPopUp;

    private float currentPower = 0;
    private float powerRaiseRate = 0.15f;
    private float powerDrainRate = 0.01f;

    private bool raceStarted = false;
    private float elapsedTime = 0;
    private bool raceEnded = false;
    private bool runnerStoppedAtEnd = false;

    private float runAnimationScale = 4.0f;
    private float backgroundSpeedScale = 0.05f;

    private bool recording = false;
    private bool replaying = false;

    void Awake()
    {
        recImage.enabled = false;
        playImage.enabled = false;
    }

    public void OnEnable()
    {
        Tracker.onPlaybackFinished.AddListener(onPlaybackFinished);
    }

    public void OnDisable()
    {
        Tracker.onPlaybackFinished.RemoveListener(onPlaybackFinished);
    }



    // Update is called once per frame
    void Update()
    {

        if (raceStarted)
        {
            elapsedTime += Time.deltaTime;
        }

        UpdateInput();

        UpdatePower();

        UpdateGameStatus();

        UpdateRunner();

        UpdateBackground();

        UpdateHUD();
    }

    private void UpdateInput()
    {
        if (raceEnded)
            return;

        if (Input.GetKeyUp(KeyCode.Space))
        {
            currentPower += powerRaiseRate;
            if (!raceStarted)
            {
                raceStarted = true;
            }
        }
    }

    private void UpdatePower()
    {
        currentPower -= powerDrainRate;
        currentPower = Mathf.Clamp01(currentPower);
    }


    private void UpdateGameStatus()
    {
        //just crossed the start recording point?
        if (!raceEnded && !recording && startRecordingPoint.position.x < 0f)
        {
            Debug.Log("RECORDING!");
            Tracker.Instance.Record(Tracker.FPS_30);
            recording = true;
        }

        //reached finish line point?
        if (!raceEnded && finishLinePoint.position.x < 0f)
        {
            Debug.Log("REACHED FINISH!");
            powerDrainRate *= 6f; //drain power x6 faster
            RaceEnded();

            Invoke("Replay", 2.0f);
        }

        //let the runner stop
        if (raceEnded && !runnerStoppedAtEnd && currentPower <= 0)
        {
            Tracker.Instance.Pause(); //pause the recording
            runnerStoppedAtEnd = true;
        }
    }

    private void UpdateRunner()
    {
        runner.SetFloat("runspeed", currentPower * runAnimationScale);
    }

    private void UpdateBackground()
    {
        if (raceStarted)
        {
            Vector3 pos = background.localPosition;
            pos.x -= currentPower * backgroundSpeedScale;
            background.localPosition = pos;
        }
    }

    private void UpdateHUD()
    {
        if (!raceEnded)
        {
            int minutes = Mathf.FloorToInt(elapsedTime / 60F);
            int seconds = Mathf.FloorToInt(elapsedTime - minutes * 60);
            int cents = Mathf.FloorToInt((elapsedTime * 100) % 100);

            //flick instructions        
            if (currentPower < 0.1f)
            {
                instructions.enabled = ((int)(Time.timeSinceLevelLoad * 2.0f) % 2.0f == 0);
            }
            else
            {
                instructions.enabled = false;
            }

            //update time
            timeText.text = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, cents);
        }

        if (recording)
        {
            //flick rec button
            recImage.enabled = ((int)(Time.timeSinceLevelLoad * 4.0f) % 2.0f == 0);
        }
        else if (replaying)
        {
            //flick play button
            playImage.enabled = ((int)(Time.timeSinceLevelLoad * 4.0f) % 2.0f == 0);
        }


        //update power bar
        powerBar.rectTransform.localScale = new Vector3(currentPower, 1f, 1f);
    }

    private void RaceEnded()
    {
        instructions.enabled = false;
        recImage.enabled = false;
        recording = false;
        raceEnded = true;
    }

    private void Replay()
    {
        replaying = true;
        Tracker.Instance.Stop();
        Tracker.Instance.Play(0.3f);
    }

    private void onPlaybackFinished()
    {
        replaying = false;
        restartPopUp.SetActive(true);
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
