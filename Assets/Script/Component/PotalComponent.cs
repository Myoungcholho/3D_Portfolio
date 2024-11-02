using UnityEngine;
using UnityEngine.UI;

public class PotalComponent : MonoBehaviour , IInteractable
{
    public float canvasScale = 0.1f; // 원래 설정하고 싶은 크기
    public string loadSceneName = "Main";
    public string spawnObjectName = "Spawn_Boss";
    private Canvas canvas;
    private GameObject canvasParentObject;

    private void Awake()
    {
        canvas = GetComponentInChildren<Canvas>();
        canvasParentObject = canvas.transform.parent.gameObject;
        Debug.Assert(canvas != null);
    }

    private void Start()
    {
        canvas.enabled = false;
    }

    private void Update()
    {
        if(canvas.enabled ==true)
            canvas.transform.rotation = Camera.main.transform.rotation;
    }

    void LateUpdate()
    {
        // 카메라와 캔버스 사이의 거리 계산
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);

        // 거리와 무관하게 캔버스가 일정한 크기를 유지하도록 스케일 조정
        canvasParentObject.transform.localScale = Vector3.one * distance * canvasScale;
    }

    #region Interaction Interface
    public void Interact(GameObject interactor)
    {
        LoadingSceneController.LoadScene(loadSceneName, spawnObjectName);
    }

    public int GetPriority()
    {
        return 1;    
    }
    #endregion
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        canvas.enabled = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        canvas.enabled = false;
    }

}
