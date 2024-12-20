using TMPro;
using UnityEditor;
using UnityEngine;

using StateType = StateComponent.StateType;
using DamagedType = StateComponent.DamageStateType;

public class PlayerDebugCanvas : MonoBehaviour
{
    [SerializeField]
    private GameObject debugObject;

    private StateComponent state;
    private WeaponComponent weapon;
    private PlayerMovingComponent moving;

    //Text
    private TextMeshProUGUI objectName;
    private TextMeshProUGUI currentStateText;
    private TextMeshProUGUI prevStateText;
    private TextMeshProUGUI weaponText;
    private TextMeshProUGUI speedText;
    private TextMeshProUGUI damagedText;

    private void Awake()
    {
        if (debugObject == null)
            return;

        state = debugObject.GetComponent<StateComponent>();
        weapon = debugObject.GetComponent<WeaponComponent>();
        moving = debugObject.GetComponent<PlayerMovingComponent>();

        objectName = gameObject.transform.FindChildByName("EnemyName")?.GetComponent<TextMeshProUGUI>();
        currentStateText = gameObject.transform.FindChildByName("StateText").GetComponent<TextMeshProUGUI>();
        prevStateText = gameObject.transform.FindChildByName("PrevStateText").GetComponent<TextMeshProUGUI>();
        weaponText = gameObject.transform.FindChildByName("WeaponText").GetComponent<TextMeshProUGUI>();
        speedText = gameObject.transform.FindChildByName("SpeedText")?.GetComponent<TextMeshProUGUI>();
        damagedText = gameObject.transform.FindChildByName("DamagedText")?.GetComponent<TextMeshProUGUI>();

        state.OnStateTypeChanged += CharacterStateUpdate;
        weapon.OnWeaponTypeChanged += CharacterWeaponUpdate;
        state.OnDamageStateChanged += CharacterDamagedTypeUpdate;
    }

    private void Start()
    {
        if(objectName  != null)
            objectName.text = gameObject.name;
    }

    private void Update()
    {
        if(speedText != null)
            speedText.text = moving.currentSpeed.ToString();
    }

    private void CharacterStateUpdate(StateType prevType, StateType type)
    {
        currentStateText.text = type.ToString(); 
        prevStateText.text = prevType.ToString();
    }

    private void CharacterWeaponUpdate(WeaponType prevType, WeaponType type)
    {
        weaponText.text = type.ToString();
    }

    private void CharacterDamagedTypeUpdate(DamagedType prevType, DamagedType type)
    {
        damagedText.text = type.ToString();
    }

}
