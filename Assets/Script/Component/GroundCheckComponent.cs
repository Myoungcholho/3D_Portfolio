using UnityEngine;

public class GroundCheckComponent : MonoBehaviour
{
    public float checkSphereRadius = 0.1f; // CheckSphere¿« π›∞Ê
    public LayerMask groundLayers;

    private StateComponent state;

    private void Awake()
    {
        state = GetComponent<StateComponent>();
    }

    void Update()
    {
        IsGroundedUpdate();
    }


    void IsGroundedUpdate()
    {
        if(Physics.CheckSphere(transform.position, checkSphereRadius, groundLayers))
        {
            state.SetGroundedMode();
            return;
        }

        state.SetAirborneMode();
        return;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkSphereRadius);
    }
}
