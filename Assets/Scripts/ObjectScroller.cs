using UnityEngine;

public class ObjectScroller : MonoBehaviour
{
    public float scrollSpeed = -2f;
    public float destroyOffsetX = 2f;
    public static float SpeedMultiplier = 1f;

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        transform.position += Vector3.right * scrollSpeed * SpeedMultiplier * Time.deltaTime;

        float dist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        float leftEdge = cam.ViewportToWorldPoint(new Vector3(0f, 0f, dist)).x;

        if (transform.position.x < leftEdge - destroyOffsetX)
        {
            Destroy(gameObject);
        }
    }
}
