using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BaldAndBold
{
    public class CannonDemo : MonoBehaviour
    {

        public Transform cameraRig;
        public Transform cannonMuzzle;
        public Rigidbody cannonBall;
        public Vector3 launchForce;

        private Vector3 initialCameraPosition;

        void Start()
        {
            initialCameraPosition = cameraRig.position;
        }

        void OnEnable()
        {

            //in this example we will automatically launch the cannonball when we start recording
            Tracker.onRecordStarted.AddListener(FireCannonBall);
            Tracker.onRecordFinished.AddListener(ResetPositions);
            Tracker.onPlaybackStopped.AddListener(ResetPositions);
        }

        void OnDisable()
        {
            //remember to free event listeners when leaving to avoid null pointer exceptions
            Tracker.onRecordStarted.RemoveListener(FireCannonBall);
            Tracker.onRecordFinished.RemoveListener(ResetPositions);
            Tracker.onPlaybackStopped.RemoveListener(ResetPositions);
        }


        private void ResetPositions()
        {
            //reset cannon ball position
            cannonBall.isKinematic = true;
            cannonBall.velocity = Vector3.zero;
            cannonBall.angularVelocity = Vector3.zero;
            cannonBall.position = cannonMuzzle.position;

            //also reset camera position
            cameraRig.position = initialCameraPosition;
        }

        private void FireCannonBall()
        {
            ResetPositions();

            //fire!
            cannonBall.isKinematic = false;
            cannonBall.AddForce(launchForce, ForceMode.Impulse);
        }
    }
}