using UnityEngine;

public class ShieldPickup : MonoBehaviour
{
    public float rechargeAmount = 25f;
    public float floatSpeed = 3f;
    public float lifetime = 15f;

    private Transform player;

    void Start()
    {
        PlayerShield shield = FindObjectOfType<PlayerShield>();
        if (shield != null)
        {
            player = shield.transform;
        }
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (player != null && player.gameObject.activeInHierarchy)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, floatSpeed * Time.deltaTime);
        }
        else
        {
            transform.position += Vector3.left * floatSpeed * BackGroundScroller.SpeedMultiplier * Time.deltaTime;
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        PlayerShield shield = col.GetComponent<PlayerShield>();
        if (shield != null)
        {
            shield.Recharge(rechargeAmount);
            Destroy(gameObject);
        }
    }
}
