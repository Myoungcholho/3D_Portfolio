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

        // 스킬 캐스팅이라면 커서가 보이게,
        // 캐스팅이 끝나면 커서 사라지게
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

        // ray를 쐇는데 hit된게 없어서 (0,0,0)이된다.

        if (CameraHelpers.GetCursorLocation(out position, out normal, traceDistance, layerMask)==false)
            return;

        decalObject.transform.position = position;
    }

    // 데칼 생성
    public void ActivateDecalForTargeting(SkillType skillType, float distance)
    {
        this.skillType = skillType;
        traceDistance = distance;
        SetWorldCursor();

        decalObject.SetActive(true);
    }

    // 데칼 삭제
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
