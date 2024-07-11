using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character, IDamagable
{
    [SerializeField]
    private float duration = 1.5f;

    private List<Material> allMaterials = new List<Material>();
    private Dictionary<Material,Color> originalColors = new Dictionary<Material,Color>();

    protected override void Awake()
    {
        base.Awake();



        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        actionMap.FindAction("Sword").started += context =>
        {
            weapon.SetSwordMode();
        };

        actionMap.FindAction("FireBall").started += context =>
        {
            weapon.SetFireBallMode();
        };

        actionMap.FindAction("DualSword").started += context =>
        {
            weapon.SetDualSwordMode();
        };

        actionMap.FindAction("Fist").started += context =>
        {
            weapon.SetFistMode();
        };

        actionMap.FindAction("Action").started += context =>
        {
            weapon.DoAction();

            // ���� �� RGB(0,0,1)�� �ϰ� ����
            foreach(Material mat in allMaterials)
            {
                mat.color = new Color(0, 0, 1);
            }
            StartCoroutine(ChangeColors());
        };

        actionMap.FindAction("Evade").started += context =>
        {
            /*if (weapon.UnarmedMode == false)
                return;*/
            /*if (state.IdleMode == false)
                return;*/

            state.SetEvadeMode();
        };

    }

    protected override void Start()
    {
        base.Start();

        // ���׸����� ���� ���� List�� ����
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Material[] materials = renderer.materials;
            foreach (Material material in materials)
            {
                allMaterials.Add(material);
                originalColors[material] = material.color;
            }
        }
    }

    IEnumerator ChangeColors()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            foreach (Material mat in allMaterials)
            {
                if (originalColors.ContainsKey(mat))
                {
                    mat.color = Color.Lerp(new Color(0, 0, 1), originalColors[mat], t);
                }
            }

            yield return null;
        }

        // ���� �Ϸ� �� ������ ���� �������� Ȯ���� ����
        foreach (Material mat in allMaterials)
        {
            if (originalColors.ContainsKey(mat))
            {
                mat.color = originalColors[mat];
            }
        }
    }


    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        
    }
}
