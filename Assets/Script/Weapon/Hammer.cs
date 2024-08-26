using UnityEngine;
using UnityEngine.Audio;

public class Hammer : Melee
{
    [SerializeField]
    private string handName = "Hand_Hammer";

    [SerializeField]
    private GameObject particlePrefab;

    [SerializeField]
    private string particleTransformName = "warhammer_head_low";

    private Transform handTransform;
    private Transform particleTransform;
    private GameObject trailParticle;


    private AudioClip audioSourceAttack01;
    private AudioMixerGroup audioMixer;

    protected override void Reset()
    {
        base.Reset();

        type = WeaponType.Hammer;
    }

    protected override void Awake()
    {
        base.Awake();

        handTransform = rootObject.transform.FindChildByName(handName);
        Debug.Assert(handTransform != null);

        transform.SetParent(handTransform, false);
        gameObject.SetActive(false);


        particleTransform = transform.FindChildByName(particleTransformName);
    }

    public override void Begin_Equip()
    {
        base.Begin_Equip();

        gameObject.SetActive(true);
    }

    public override void UnEquip()
    {
        base.UnEquip();

        gameObject.SetActive(false);
    }

    public override void Begin_Collision(AnimationEvent e)
    {
        base.Begin_Collision(e);

        if (particleTransform == null)
            return;

        if (particlePrefab == null)
            return;

        trailParticle = Instantiate<GameObject>(particlePrefab, particleTransform);
    }

    public override void End_Collision()
    {
        base.End_Collision();

        Destroy(trailParticle);
    }


    public override void Play_Sound()
    {
        base.Play_Sound();

        if (audioSourceAttack01 == null)
            audioSourceAttack01 = SoundLibrary.Instance.hammerAttack01;
        if (audioMixer == null)
            audioMixer = SoundLibrary.Instance.mixerBasic;

        if (soundComponent != null)
        {
            soundComponent.PlayLocalSound(audioSourceAttack01, audioMixer, false);
        }
    }
}