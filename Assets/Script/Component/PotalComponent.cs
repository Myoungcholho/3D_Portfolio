using UnityEngine;
using UnityEngine.UI;

public class PotalComponent : MonoBehaviour , IInteractable
{
    public float canvasScale = 0.1f; // ���� �����ϰ� ���� ũ��
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
        // ī�޶�� ĵ���� ������ �Ÿ� ���
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);

        // �Ÿ��� �����ϰ� ĵ������ ������ ũ�⸦ �����ϵ��� ������ ����
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
