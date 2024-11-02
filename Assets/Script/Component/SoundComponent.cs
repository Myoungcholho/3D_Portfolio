using UnityEngine;
using UnityEngine.Audio;

[RequireComponent (typeof(AudioSource))]
public class SoundComponent : MonoBehaviour
{
    private AudioSource audioSource;        // 캐릭터 전용 오디오
    private SoundManager soundManager;      // 씬에 있는 Pool 오디오
    private SoundLibrary soundLibrary;      // 공용 오디오 저장소
    
    void Start()
    {
        soundManager = SoundManager.Instance;
        Debug.Assert(soundManager != null);

        soundLibrary = SoundLibrary.Instance;
        Debug.Assert(soundLibrary != null);

        audioSource = GetComponent<AudioSource>();
    }

    
    public void PlayPooledAudio(AudioClip clip, AudioMixerGroup group,bool is3D, Vector3 position=default)
    {
        soundManager.PlaySound(clip, group, is3D, position);
    }

    public void PlayLocalSound(AudioClip clip, AudioMixerGroup group, bool is3D, Vector3 position = default)
    {
        if (audioSource == null)
            return;

        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = group;
        audioSource.Play();
    }


    // Animation Clip Event call
    // 움직임 사운드 이므로 여기 
    private void Sound_Evade()  // pool X
    {
        if (audioSource == null)
            return;
        if (audioSource.isPlaying)
            return;

        AudioClip clip = soundLibrary.evade01;
        AudioMixerGroup group = soundLibrary.mixerBasic;
        
        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = group;
        audioSource.spatialBlend = 1f;

        audioSource.Play();
    }

    // 무기에 종속시키는 것이 맞아보임. 무기마다 회피공격이 가능하다면
    private void Sound_DodgeAttack()
    {
        AudioClip clip = soundLibrary.swordAttack01;
        AudioMixerGroup group = soundLibrary.mixerBasic;

        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = group;
        audioSource.spatialBlend = 1f;

        audioSource.Play();
    }

    // 피격음
    private void Sound_FistImpact()
    {
        AudioClip clip = soundLibrary.fistImpact01;
        AudioMixerGroup group = soundLibrary.mixerImpact;

        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = group;
        audioSource.spatialBlend = 1f;

        audioSource.Play();
    }
    private void Sound_SwordImpact()
    {
        AudioClip clip = soundLibrary.swordImpact01;
        AudioMixerGroup group = soundLibrary.mixerImpact;

        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = group;
        audioSource.spatialBlend = 1f;

        audioSource.Play();
    }
    private void Sound_HammerImpact()
    {
        AudioClip clip = soundLibrary.hammerImpact01;
        AudioMixerGroup group = soundLibrary.mixerImpact;

        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = group;
        audioSource.spatialBlend = 1f;

        audioSource.Play();
    }
    private void Sound_StaffImpact()
    {
        AudioClip clip = soundLibrary.swordImpact01;        // 나중에 수정
        AudioMixerGroup group = soundLibrary.mixerImpact;

        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = group;
        audioSource.spatialBlend = 1f;

        audioSource.Play();
    }
    private void Sound_DualSwordImpact()                    // 나중에 수정
    {
        AudioClip clip = soundLibrary.swordImpact01;
        AudioMixerGroup group = soundLibrary.mixerImpact;

        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = group;
        audioSource.spatialBlend = 1f;

        audioSource.Play();
    }
    private void Sound_InstantKillImpact()
    {
        AudioClip clip = soundLibrary.instantKill02;
        AudioMixerGroup group = soundLibrary.mixerImpact;

        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = group;
        audioSource.spatialBlend = 1f;

        audioSource.Play();
    }
}
