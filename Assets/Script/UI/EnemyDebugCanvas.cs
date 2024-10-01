using TMPro;
using UnityEditor;
using UnityEngine;

using StateType = StateComponent.StateType;

public class EnemyDebugCanvas : MonoBehaviour
{
    private GameObject debugObject;

    private StateComponent state;
    private WeaponComponent weapon;
    private PlayerMovingComponent moving;
    private AIController aiController;
    
    //Text
    private TextMeshProUGUI objectName;
    private TextMeshProUGUI currentStateText;
    private TextMeshProUGUI prevStateText;
    private TextMeshProUGUI weaponText;
    private TextMeshProUGUI speedText;
    private TextMeshProUGUI aiTypeText;
    private TextMeshProUGUI aiPrevTypeText;
    private TextMeshProUGUI coolTimeText;

    private void Awake()
    {
        debugObject = transform.parent.gameObject;

        if (debugObject == null)
            return;

        state = debugObject.GetComponent<StateComponent>();
        weapon = debugObject.GetComponent<WeaponComponent>();
        moving = debugObject.GetComponent<PlayerMovingComponent>();
        aiController = debugObject.GetComponent<AIController>();

        objectName = gameObject.transform.FindChildByName("EnemyName")?.GetComponent<TextMeshProUGUI>();
        currentStateText = gameObject.transform.FindChildByName("StateText")?.GetComponent<TextMeshProUGUI>();
        prevStateText = gameObject.transform.FindChildByName("PrevStateText")?.GetComponent<TextMeshProUGUI>();
        weaponText = gameObject.transform.FindChildByName("WeaponText")?.GetComponent<TextMeshProUGUI>();
        speedText = gameObject.transform.FindChildByName("SpeedText")?.GetComponent<TextMeshProUGUI>();
        aiTypeText = gameObject.transform.FindChildByName("AITypeText")?.GetComponent<TextMeshProUGUI>();
        aiPrevTypeText = gameObject.transform.FindChildByName("AIPrevTypeText")?.GetComponent<TextMeshProUGUI>();
        coolTimeText = gameObject.transform.FindChildByName("CoolTimeText")?.GetComponent<TextMeshProUGUI>();

        state.OnStateTypeChanged += CharacterStateUpdate;
        weapon.OnWeaponTypeChanged += CharacterWeaponUpdate;

        if(aiController != null)
            aiController.OnAIStateTypeChanged += EnemyAIStateUpdate;
    }

    private void Start()
    {
        if (objectName != null)
            objectName.text = debugObject.name;
    }

    private void Update()
    {
        if (speedText != null)
            speedText.text = moving.currentSpeed.ToString() + "/s";

        if (aiController != null)
            coolTimeText.text = aiController.CurrentCoolTime.ToString();

        gameObject.transform.rotation = Camera.main.transform.rotation;    
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

    private void EnemyAIStateUpdate(AIController.Type prev, AIController.Type type)
    {
        aiPrevTypeText.text = prev.ToString();
        aiTypeText.text = type.ToString();
    }


}
