using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public float delayBeforePull = 1f;
    public float flySpeed = 6f;
    public float shrinkDistance = 1.5f;
    public float delayBeforeComplete = 1f;
    public string levelCompleteScene = "LevelComplete";
    public Color colorA = new Color(0.4f, 0.2f, 1f);
    public Color colorB = new Color(0.2f, 0.9f, 1f);
    public float colorCycleSpeed = 1.5f;
    public float shakeAmount = 0.05f;

    private Vector3 basePos;

    void Awake()
    {
        basePos = transform.position;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * colorCycleSpeed * Mathf.PI) + 1f) * 0.5f;
        EffectSpawner.Tint(gameObject, Color.Lerp(colorA, colorB, t));
        float shake = GameSettings.ScreenShake ? shakeAmount : 0f;
        transform.position = basePos + (Vector3)(Random.insideUnitCircle * shake);
    }

    IEnumerator Start()
    {
        AudioManager.Play(AudioManager.Sfx.Portal);
        PlayerHealth player = FindObjectOfType<PlayerHealth>();
        if (player != null && player.gameObject.activeInHierarchy)
        {
            DisableControls(player.gameObject);
            yield return new WaitForSeconds(delayBeforePull);

            Transform t = player.transform;
            Vector3 startScale = t.localScale;
            Vector3 target = new Vector3(basePos.x, basePos.y, t.position.z);
            while (t.position != target)
            {
                t.position = Vector3.MoveTowards(t.position, target, flySpeed * Time.deltaTime);
                t.localScale = startScale * Mathf.Clamp01(Vector3.Distance(t.position, target) / shrinkDistance);
                yield return null;
            }
            player.gameObject.SetActive(false);
        }

        yield return new WaitForSeconds(delayBeforeComplete);
        LevelClearMenu.LastLevel = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(levelCompleteScene);
    }

    void DisableControls(GameObject player)
    {
        PlayerLaneMovement movement = player.GetComponent<PlayerLaneMovement>();
        if (movement != null) movement.enabled = false;
        PlayerShooter shooter = player.GetComponent<PlayerShooter>();
        if (shooter != null) shooter.enabled = false;
        foreach (Collider2D col in player.GetComponentsInChildren<Collider2D>()) col.enabled = false;
    }
}
