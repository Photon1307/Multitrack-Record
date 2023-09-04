using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelController : MonoBehaviour
{
    public float moveSpeed = 1f;
    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 0.5f)
        {
            transform.position += new Vector3(moveSpeed, 0, 0);
            timer = 0f;
        }
    }

}
