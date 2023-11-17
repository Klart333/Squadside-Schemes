using DG.Tweening;
using UnityEngine;

public class AudioRepeater : MonoBehaviour
{
    [SerializeField]
    private float bpm;

    [SerializeField]
    private float targetTime;

    private AudioSource source;

    private float timer = 0;

    private void Awake()
    {
        source = GetComponent<AudioSource>();

        float v = Mathf.RoundToInt(targetTime / (60.0f / bpm));
        targetTime = v * (60.0f / bpm);
    }

    private void OnEnable()
    {
        source.Play();

        float target = source.volume;
        source.volume = 0;
        source.DOFade(target, 4.0f);
    }

    private void OnDisable()
    {
        source.Pause();
    }

    private void Update()
    {
        if (source.isPlaying)
        {
            timer += Time.deltaTime;

            if (timer >= targetTime)
            {
                timer = 0;
                source.PlayOneShot(source.clip);
            }
        }
    }
}
