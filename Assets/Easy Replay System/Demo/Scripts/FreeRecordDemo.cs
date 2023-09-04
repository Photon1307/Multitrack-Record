using BaldAndBold;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class FreeRecordDemo : MonoBehaviour {

    public Image recImage;
    public Image playImage;
    public Transform movableBox;

    public LayerMask floorLayerMask;

    private bool recording = false;
    private bool replaying = false;

    private Ray ray;
    private RaycastHit hit;
    private Vector3 movableBoxPosition;

    void Awake()
    {
        recImage.enabled = false;
        playImage.enabled = false;
    }

    void Start()
    {
        movableBoxPosition = movableBox.transform.position;
    }

	void Update () {
        UpdateInput();
        UpdateMovableBox();
        UpdateHUD();
    }

    private void UpdateInput()
    {
        if (Input.GetKeyUp(KeyCode.S))
        {
            StopTracker();
        }

        if (Input.GetKeyUp(KeyCode.R) && !recording)
        {
            StopTracker();
            Debug.Log("RECORDING!");
            Tracker.Instance.Record();
            recording = true;
        }

        if (Input.GetKeyUp(KeyCode.P) && !replaying)
        {
            StopTracker();
            Debug.Log("REPLAYING!");
            Tracker.Instance.Play();
            replaying = true;
        }
    }

    private void UpdateMovableBox()
    {
        if (!recording)
            return;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, floorLayerMask))
        {         
            movableBoxPosition.x = hit.point.x;
            movableBoxPosition.z = hit.point.z;
            movableBox.transform.position = movableBoxPosition;         
        }
    }

    private void UpdateHUD()
    {

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
    }

    private void StopTracker()
    {
        Tracker.Instance.Stop();
        recording = false;
        replaying = false;
        recImage.enabled = false;
        playImage.enabled = false;
    }

}
