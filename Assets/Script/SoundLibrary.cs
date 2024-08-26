using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundType
{
    SwordHit,
    MaxCount,
}

public enum MixerGroup
{
    basic,
    Impact,
    MaxCount,
}


public class SoundLibrary : MonoBehaviour
{
    [Header("-Clip-")]
    // fist
    [Header("--Fist--")]
    public AudioClip fistAttack01;
    public AudioClip fistImpact01;

    // sword
    [Header("--Sword--")]
    public AudioClip swordAttack01;
    public AudioClip swordImpact01;

    // Hammer
    [Header("--Hammer--")]
    public AudioClip hammerAttack01;
    public AudioClip hammerImpact01;

    // staff
    [Header("--Staff--")]
    public AudioClip staffAttack01;
    public AudioClip staffImpact01;
    public AudioClip projectileExplosion01;

    // dual sword


    // evade
    [Header("--evade--")]
    public AudioClip evade01;
    public AudioClip evadeDodage01;

    // InstantKill
    [Header("--InstantKill--")]
    public AudioClip instantKill01;
    public AudioClip instantKill02;

    // teleport
    [Header("--teleport--")]
    public AudioClip teleport01;

    // Boss
    [Header("--Boss--")]
    public AudioClip bossEarth;

    [Header("-Mixer-")]
    public AudioMixerGroup mixerBasic;
    public AudioMixerGroup mixerImpact;


    public static SoundLibrary Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}