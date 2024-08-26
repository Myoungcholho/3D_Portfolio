using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public AudioSource audioSourcePrefab2D; // 해당 프리팹이 가진 오디오 소스가 아닌 전체가 인스턴스화
    public AudioSource audioSourcePrefab3D;

    private Queue<AudioSource> audioSourcePool2D;
    private Queue<AudioSource> audioSourcePool3D;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        // 2D와 3D 오디오 소스 풀 초기화
        audioSourcePool2D = new Queue<AudioSource>();
        audioSourcePool3D = new Queue<AudioSource>();

        // 예제 풀 크기 초기화
        InitAudioSourcePool(audioSourcePool2D, audioSourcePrefab2D, 10);
        InitAudioSourcePool(audioSourcePool3D, audioSourcePrefab3D, 10);
    }

    private void InitAudioSourcePool(Queue<AudioSource> pool, AudioSource prefab, int size)
    {
        for (int i = 0; i < size; i++)
        {
            AudioSource newAudioSource = Instantiate(prefab, transform);
            newAudioSource.gameObject.SetActive(false);
            pool.Enqueue(newAudioSource);
        }
    }

    public void PlaySound(AudioClip clip, AudioMixerGroup mixer, bool is3D, Vector3 position = default)
    {
        AudioSource source;
        if (is3D)
        {
            if (audioSourcePool3D.Count <= 0)
            {
                Debug.Log("3D AudioPool is full");
                // 만약 동적으로 풀을 늘리고 싶다면 이곳에..
                return;
            }
            source = audioSourcePool3D.Dequeue();
        }
        else
        {
            if (audioSourcePool2D.Count <= 0)
            {
                Debug.Log("2D AudioPool is full");
                // 만약 동적으로 풀을 늘리고 싶다면 이곳에..
                return;
            }

            source = audioSourcePool2D.Dequeue();
        }


        source.clip = clip;
        source.outputAudioMixerGroup = mixer;
        source.gameObject.SetActive(true);

        if (is3D)
        {
            source.transform.position = position;
            source.spatialBlend = 1f; // 3D 사운드
        }
        else
        {
            source.spatialBlend = 0f; // 2D 사운드
        }

        source.Play();
        StartCoroutine(ReturnToPoolAfterPlay(source, is3D));
    }

    private IEnumerator<WaitForSeconds> ReturnToPoolAfterPlay(AudioSource source, bool is3D)
    {
        yield return new WaitForSeconds(source.clip.length);
        source.Stop();
        source.gameObject.SetActive(false);

        if (is3D)
            audioSourcePool3D.Enqueue(source);
        else
            audioSourcePool2D.Enqueue(source);
    }

    // TimeLine에 의한 호출
    public void RecvSignal()
    {
        AudioClip clip = SoundLibrary.Instance.instantKill01;
        AudioMixerGroup group = SoundLibrary.Instance.mixerBasic;

        PlaySound(clip, group, false);
    }
}
