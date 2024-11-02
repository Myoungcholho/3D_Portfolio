using UnityEngine;
using static StateComponent;

public enum SkillType
{
    None,Sword01,Sword02,Staff01,Staff02,Max,
}

public class DecalComponent : MonoBehaviour
{
    public GameObject decalPrefab;

    private SkillType skillType;
    public SkillType SkillType { get => skillType; }

    [SerializeField]
    private float traceDistance = 10;
    [SerializeField]
    private LayerMask layerMask;

    private StateComponent state;
    private WorldCursor cursor;
    private GameObject decalObject;
    private CursorComponent cursorComponent;

    private Vector3 position;
    public Vector3 Position { get => position; }
    private Vector3 normal;
    public Vector3 Normal { get => normal; }



    private void Awake()
    {
        state = GetComponent<StateComponent>();
        cursorComponent = GetComponent<CursorComponent>();

        // ��ų ĳ�����̶�� Ŀ���� ���̰�,
        // ĳ������ ������ Ŀ�� �������
        state.OnStateTypeChanged += OnCursor;
    }

    private void Start()
    {
        if (decalPrefab != null)
        {
            decalObject = Instantiate<GameObject>(decalPrefab);
            cursor = decalObject.GetComponent<WorldCursor>();
            cursor.TraceDistance = traceDistance;
            cursor.Mask = layerMask;
            decalObject.SetActive(false);
        }
    }

    private void Update()
    {
        bool bCheck = false;
        bCheck |= (state.SkillCastMode == false);
        if(bCheck)
        {
            CancelDecal();
            return;
        }

        // ray�� �i�µ� hit�Ȱ� ��� (0,0,0)�̵ȴ�.

        if (CameraHelpers.GetCursorLocation(out position, out normal, traceDistance, layerMask)==false)
            return;

        decalObject.transform.position = position;
    }

    // ��Į ����
    public void ActivateDecalForTargeting(SkillType skillType, float distance)
    {
        this.skillType = skillType;
        traceDistance = distance;
        SetWorldCursor();

        decalObject.SetActive(true);
    }

    // ��Į ����
    private void CancelDecal()
    {
        skillType = SkillType.None;
        traceDistance = 0;
        SetWorldCursor();

        decalObject.SetActive(false);
    }

    private void SetWorldCursor()
    {
        if (cursor == null)
            return;

        cursor.TraceDistance = traceDistance;
        cursor.Mask = layerMask;
    }

    private void OnCursor(StateType prev, StateType curr)
    {
        if (curr == StateType.SkillCast)
        {
            cursorComponent.ShowCursorForUI();
            return;
        }


        if(prev == StateType.SkillCast)
        {
            cursorComponent.HideCursorForUI();
            return;
        }
    }

}
