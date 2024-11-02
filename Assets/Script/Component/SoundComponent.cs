using UnityEngine;
using UnityEngine.Audio;

[RequireComponent (typeof(AudioSource))]
public class SoundComponent : MonoBehaviour
{
    private AudioSource audioSource;        // ĳ���� ���� �����
    private SoundManager soundManager;      // ���� �ִ� Pool �����
    private SoundLibrary soundLibrary;      // ���� ����� �����
    
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
    // ������ ���� �̹Ƿ� ���� 
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

    // ���⿡ ���ӽ�Ű�� ���� �¾ƺ���. ���⸶�� ȸ�ǰ����� �����ϴٸ�
    private void Sound_DodgeAttack()
    {
        AudioClip clip = soundLibrary.swordAttack01;
        AudioMixerGroup group = soundLibrary.mixerBasic;

        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = group;
        audioSource.spatialBlend = 1f;

        audioSource.Play();
    }

    // �ǰ���
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
        AudioClip clip = soundLibrary.swordImpact01;        // ���߿� ����
        AudioMixerGroup group = soundLibrary.mixerImpact;

        audioSource.clip = clip;
        audioSource.outputAudioMixerGroup = group;
        audioSource.spatialBlend = 1f;

        audioSource.Play();
    }
    private void Sound_DualSwordImpact()                    // ���߿� ����
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
