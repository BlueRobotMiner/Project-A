// SceneMusic
// Optional per-scene music override. Put it on an empty object with the scene's own
// tracks; the AudioManager finds it on scene load. Scenes without one play the
// AudioManager's level rotation.
using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    public AudioClip[] tracks;
    public bool shuffle = true;
}
