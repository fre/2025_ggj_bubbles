using UnityEngine;
using System;

[CreateAssetMenu(fileName = "BubbleSoundManager", menuName = "Bubbles/Sound Manager")]
public class BubbleSoundManager : ScriptableObject
{
  [Serializable]
  public class SoundConfig
  {
    public AudioClip Clip;
    public Vector2 PitchRange = new Vector2(0.9f, 1.1f);
    [Range(0f, 1f)]
    public float Volume = 1f;
    [Tooltip("Number of samples to skip at the start of the clip")]
    public int StartSampleOffset = 0;
  }

  [Header("Pop Sounds")]
  public SoundConfig[] PopSounds;

  [Header("Merge Sounds")]
  public SoundConfig[] MergeSounds;

  [Header("Audio Settings")]
  [Range(0f, 1f)]
  public float MasterVolume = 1f;
  public float MaxDistance = 10f;
  public float SpatialBlend = 1f; // 0 = 2D, 1 = 3D
  [SerializeField] private GameObject _audioSourcePrefab;

  public void PlayRandomSound(Vector3 position, SoundConfig[] configs)
  {
    if (configs == null || configs.Length == 0) return;
    if (_audioSourcePrefab == null)
    {
      Debug.LogError("No audio source prefab assigned!");
      return;
    }

    SoundConfig config = configs[UnityEngine.Random.Range(0, configs.Length)];
    if (config.Clip == null) return;

    GameObject audioObj = Instantiate(_audioSourcePrefab, position, Quaternion.identity);
    AudioSource source = audioObj.GetComponent<AudioSource>();
    if (source == null)
    {
      Debug.LogError("Audio source prefab doesn't have an AudioSource component!");
      Destroy(audioObj);
      return;
    }

    // Prepare audio source before playing
    source.clip = config.Clip;
    source.pitch = UnityEngine.Random.Range(config.PitchRange.x, config.PitchRange.y);
    source.volume = config.Volume * MasterVolume;
    source.timeSamples = Mathf.Min(config.StartSampleOffset, config.Clip.samples - 1); // Ensure we don't exceed clip length
    source.PlayScheduled(AudioSettings.dspTime); // Schedule immediate playback

    float remainingTime = (config.Clip.samples - config.StartSampleOffset) / (float)config.Clip.frequency;
    Destroy(audioObj, remainingTime + 0.1f);  // Adjust cleanup time based on remaining samples
  }

  public void PlayPopSound(Vector3 position)
  {
    Debug.Log("PlayPopSound called");
    PlayRandomSound(position, PopSounds);
  }

  public void PlayMergeSound(Vector3 position)
  {
    Debug.Log("PlayMergeSound called");
    PlayRandomSound(position, MergeSounds);
  }
}