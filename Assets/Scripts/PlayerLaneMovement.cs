using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLaneMovement : MonoBehaviour
{
    public int laneCount = 5;
    public float laneSpacing = 1.5f;
    public float centerY = 0f;
    public float moveSpeed = 8f;
    public float maxSpeedMultiplier = 2f;
    public float speedUpRate = 0.5f;
    public float slowDownRate = 1f;

    private int currentLane;
    private float targetY;
    private bool returningToDefault;

    void Start()
    {
        BackGroundScroller.SpeedMultiplier = 1f;
        currentLane = laneCount / 2;
        targetY = GetLaneY(currentLane);
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    }

    void Update()
    {
        UpdateScrollSpeed();

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (currentLane < laneCount - 1)
            {
                currentLane++;
            }
        }

        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            if (currentLane > 0)
            {
                currentLane--;
            }
        }

        targetY = GetLaneY(currentLane);

        Vector3 pos = transform.position;
        pos.y = Mathf.MoveTowards(pos.y, targetY, moveSpeed * Time.deltaTime);
        transform.position = pos;
    }

    void UpdateScrollSpeed()
    {
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            returningToDefault = true;
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            returningToDefault = false;
            BackGroundScroller.SpeedMultiplier = Mathf.MoveTowards(BackGroundScroller.SpeedMultiplier, maxSpeedMultiplier, speedUpRate * Time.deltaTime);
        }
        else if (returningToDefault)
        {
            BackGroundScroller.SpeedMultiplier = Mathf.MoveTowards(BackGroundScroller.SpeedMultiplier, 1f, slowDownRate * Time.deltaTime);
            if (BackGroundScroller.SpeedMultiplier <= 1f)
            {
                returningToDefault = false;
            }
        }
    }

    public float GetLaneY(int lane)
    {
        return centerY + (lane - (laneCount - 1) / 2f) * laneSpacing;
    }
}
