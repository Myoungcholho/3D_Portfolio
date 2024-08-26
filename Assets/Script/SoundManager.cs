using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public AudioSource audioSourcePrefab2D; // �ش� �������� ���� ����� �ҽ��� �ƴ� ��ü�� �ν��Ͻ�ȭ
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
        // 2D�� 3D ����� �ҽ� Ǯ �ʱ�ȭ
        audioSourcePool2D = new Queue<AudioSource>();
        audioSourcePool3D = new Queue<AudioSource>();

        // ���� Ǯ ũ�� �ʱ�ȭ
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
                // ���� �������� Ǯ�� �ø��� �ʹٸ� �̰���..
                return;
            }
            source = audioSourcePool3D.Dequeue();
        }
        else
        {
            if (audioSourcePool2D.Count <= 0)
            {
                Debug.Log("2D AudioPool is full");
                // ���� �������� Ǯ�� �ø��� �ʹٸ� �̰���..
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
            source.spatialBlend = 1f; // 3D ����
        }
        else
        {
            source.spatialBlend = 0f; // 2D ����
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

    // TimeLine�� ���� ȣ��
    public void RecvSignal()
    {
        AudioClip clip = SoundLibrary.Instance.instantKill01;
        AudioMixerGroup group = SoundLibrary.Instance.mixerBasic;

        PlaySound(clip, group, false);
    }
}
