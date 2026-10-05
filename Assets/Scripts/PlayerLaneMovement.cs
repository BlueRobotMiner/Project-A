// PlayerLaneMovement
// Moves the ship between fixed lanes with W/S or Up/Down, and controls the world scroll
// speed: holding D boosts it, releasing eases back to normal. Also plays the level intro,
// where the ship flies in from the left edge before controls unlock.
using UnityEngine;
using System.Collections.Generic;

public class PlayerLaneMovement : MonoBehaviour
{
    public int laneCount = 5;
    public float laneSpacing = 1.5f;
    public float centerY = 0f;
    public float moveSpeed = 8f;
    public float maxSpeedMultiplier = 2f;
    public float speedUpRate = 0.5f;
    public float slowDownRate = 1f;
    public float maxObjectSpeedMultiplier = 3f;
    public float introDuration = 1f;
    public float introOffscreenOffset = 1f;

    public static bool IntroPlaying;

    private int currentLane;
    private float targetY;
    private bool returningToDefault;
    private float introStartX;
    private float introEndX;
    private float introTime;

    // Places the ship off the left edge of the screen so the intro can fly it in.
    // With introDuration at 0 the ship just starts in its normal position.
    void Start()
    {
        BackGroundScroller.SpeedMultiplier = 1f;
        ObjectScroller.SpeedMultiplier = 1f;
        currentLane = laneCount / 2;
        targetY = GetLaneY(currentLane);
        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

        Camera cam = Camera.main;
        introEndX = transform.position.x;
        introStartX = introEndX;
        if (cam != null && introDuration > 0f)
        {
            float dist = Mathf.Abs(cam.transform.position.z - transform.position.z);
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            float halfWidth = sr != null ? sr.bounds.extents.x : 0.5f;
            introStartX = cam.ViewportToWorldPoint(new Vector3(0f, 0.5f, dist)).x - halfWidth - introOffscreenOffset;
            transform.position = new Vector3(introStartX, targetY, transform.position.z);
        }
        introTime = 0f;
        IntroPlaying = introStartX != introEndX;
    }

    void OnDisable()
    {
        IntroPlaying = false;
    }

    // Intro flight: eases the ship from off screen to its start position, then unlocks
    // movement and shooting. Lane input is ignored until this finishes.
    void Update()
    {
        if (PauseMenu.IsPaused) return;
        if (IntroPlaying)
        {
            introTime += Time.deltaTime;
            float t = Mathf.Clamp01(introTime / introDuration);
            float eased = 1f - (1f - t) * (1f - t);
            transform.position = new Vector3(Mathf.Lerp(introStartX, introEndX, eased), targetY, transform.position.z);
            if (t >= 1f) IntroPlaying = false;
            return;
        }
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

    // Reads A/D input and drives the shared scroll speed multipliers that the
    // background, objects and spawner all use.
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

        float t = maxSpeedMultiplier > 1f ? Mathf.InverseLerp(1f, maxSpeedMultiplier, BackGroundScroller.SpeedMultiplier) : 0f;
        ObjectScroller.SpeedMultiplier = Mathf.Lerp(1f, maxObjectSpeedMultiplier, t);
    }

    public float GetLaneY(int lane)
    {
        return centerY + (lane - (laneCount - 1) / 2f) * laneSpacing;
    }
}
