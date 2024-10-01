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
        playerWeapon.OnDodgeAttack += SetActiveObject;      // 회피 이벤트 끝났을 때 호출받게

        player = playerObject.GetComponent<Player>();
        player.OnDodgeAttack += SetActiveObject;            // 회피 이벤트 시작할 떄 호출받게

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
