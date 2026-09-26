using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackGroundScroller : MonoBehaviour
{
    public BoxCollider2D collider;
    public Rigidbody2D rb;
    public float scrollSpeed = -2f;
    private float width;
    private Camera cam;

    void Start()
    {
        collider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        width = GetComponent<SpriteRenderer>().bounds.size.x;
        collider.enabled = false;
    }

    void Update()
    {
        // re-applied every frame so Inspector edits take effect live
        rb.velocity = new Vector2(scrollSpeed, 0);

        float dist = Mathf.Abs(cam.transform.position.z - transform.position.z);
        float leftEdge = cam.ViewportToWorldPoint(new Vector3(0f, 0f, dist)).x;

        // only reset once the sprite is fully off screen on the left
        if (transform.position.x + width / 2f < leftEdge)
        {
            transform.position += Vector3.right * (width * 2f);
        }
    }
}
