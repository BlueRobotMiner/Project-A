using UnityEngine;

public class PlayerStardust : MonoBehaviour
{
    public int stardust = 0;

    public void Add(int amount)
    {
        stardust += amount;
    }
}
