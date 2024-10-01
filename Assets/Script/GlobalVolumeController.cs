using UnityEngine;
using UnityEngine.Rendering;

public class GlobalVolumeController : MonoBehaviour
{
    [SerializeField]
    private string playerName = "Player";

    private GameObject playerObject;
    private WeaponComponent playerWeapon;
    private Player player;
    private Volume volume;

    private void Awake()
    {
        playerObject = GameObject.Find("Player");
        Debug.Assert(playerObject != null);

        playerWeapon = playerObject.GetComponent<WeaponComponent>();
        playerWeapon.OnDodgeAttack += SetActiveObject;      // ȸ�� �̺�Ʈ ������ �� ȣ��ް�

        player = playerObject.GetComponent<Player>();
        player.OnDodgeAttack += SetActiveObject;            // ȸ�� �̺�Ʈ ������ �� ȣ��ް�

        volume = GetComponent<Volume>();
        Debug.Assert(volume != null);
    }

    private void Start()
    {
        volume.enabled = false;
    }

    private void SetActiveObject(bool check)
    {
        volume.enabled = check;
    }
}
