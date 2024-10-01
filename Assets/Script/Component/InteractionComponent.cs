using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class InteractionComponent : MonoBehaviour
{
    public TimelineAsset instantKillTimeline; // 즉사기 타임라인 Inspector에서 할당
    public GameObject targetingUI;            // Canvas에있는 Target 점 UI

    // 우선순위로 정렬함.
    private SortedSet<IInteractable> interactables = new SortedSet<IInteractable>(new InteractableComparer());
    private RaycastShooter raycastShooter;
    private Animator animator;
    private StateComponent state;
    private PlayableDirector playableDirector;
    private BrainController brainController;
    
    private void Awake()
    {
        raycastShooter = GetComponent<RaycastShooter>();
        Debug.Assert(raycastShooter != null);
        
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        playableDirector = GetComponent<PlayableDirector>();
        brainController = Camera.main.GetComponent<BrainController>();

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");

        actionMap.FindAction("Interaction").started += context =>
        {
            //animator.SetTrigger("IsStealAction");
            //animator.SetTrigger("StealDead");

            AddInteractablesToPriorityList();       // 상호작용 가능한 오브젝트들을 리스트에 담고
            ExecuteTopInteraction();                // 가장 우선순위가 높은 오브젝트의 상호작용 기능을 실행
        };
    }

    private void Update()
    {
        if (raycastShooter.hitForward.collider != null && targetingUI != null)
        {
            // 1. 만약 해당 객체가 InstantKillInteraction 컴포넌트를 가지고 있는지 판별 없다면 return
            InstantKillInteraction instantKill = raycastShooter.hitForward.collider.gameObject.GetComponent<InstantKillInteraction>();
            if (instantKill == null)
            {
                targetingUI.SetActive(false);
                return;
            }

            // 2. 내적 값이 0.9이상인지 판별, (플레이어가 적의 뒤에서 기습이 가능한지 판별) 아니라면 return
            float dotProduct = Vector3.Dot(transform.forward, raycastShooter.hitForward.transform.forward);
            if (dotProduct < 0.9f)
            {
                targetingUI.SetActive(false);
                return;
            }

            // 3. 타겟의 3D 위치를 2D 스크린 좌표로 변환
            Transform hitObjectPosition = raycastShooter.hitForward.collider.transform;
            Vector3 backPosition = hitObjectPosition.position + Vector3.up;
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(backPosition);

            // 4. UI를 해당 스크린 좌표로 이동
            targetingUI.transform.position = screenPosition;

            // 5. UI를 활성화
            targetingUI.SetActive(true);
        }
        else
        {
            // 타겟이 없으므로 UI 비활성화
            targetingUI.SetActive(false);
        }
    }

    // 상호작용 리스트에 추가하는 메서드
    public void AddInteractablesToPriorityList()
    {
        interactables.Clear();

        // 우선순위에 따라 추가
        AddToInteractableList(raycastShooter.hitForward.collider);
        AddToInteractableList(raycastShooter.sphereHitCollider);
    }
    // 가장 우선순위가 높은 상호작용을 실행하는 메서드
    public void ExecuteTopInteraction()
    {
        if (interactables.Count > 0)
        {
            IInteractable topInteraction = interactables.Max;

            // 나에 대한 상호작용 메서드 실행
            if(topInteraction is InstantKillInteraction instantKill)
            {
                // 0. 기습을 당하는 캐릭터의 뒷쪽인지 판별 25도각에서만 가능 dot() = 0.906
                Vector3 enemyForward = instantKill.transform.forward;
                float dotProduct = Vector3.Dot(enemyForward, transform.forward);
                if (dotProduct < 0.9) // 약25도라면
                    return;

                // 1. 상태를 바꿈으로 다른거 행동 제한
                state.SetInstantKillMode();

                // 2. 적의 위치를 얻어 적의 뒤로 이동
                Vector3 enemyPosition = instantKill.transform.position;  // 적의 위치
                Vector3 behindEnemyPosition = enemyPosition - instantKill.transform.forward * 1f;  // 적의 뒤 1f 위치로 이동
                behindEnemyPosition -= instantKill.transform.right * 0.3f;  // 좌우를 적절히 조정함
                transform.position = behindEnemyPosition;  // 플레이어의 위치를 적의 뒤로 이동

                // 3. 캐릭터가 적을 바라보도록 회전
                StartCoroutine(RotateTowardsTarget(instantKill.transform, 10f));

                // 4. 애니메이터의 Trigger를 설정하여 기습 애니메이션 시작
                animator.SetTrigger("IsStealAction");

                // 5. PlayableDirector를 사용하여 즉사 타임라인 설정 및 실행
                PlayInstantKillTimeline();

                // 6. 카메라를 CUT으로 변경 (타임라인이 끝나면 복구)
                brainController.SetDefaultBlend("Cut", 0f);

                //대상의 상호작용 메서드 실행
                topInteraction.Interact();
            }

            if(topInteraction is PotalComponent potal)
            {
                //대상의 상호작용 메서드 실행
                SoundManager.Instance.PlaySound(SoundLibrary.Instance.teleport01, SoundLibrary.Instance.mixerBasic, false);
                topInteraction.Interact();
            }

        }
        else
        {
            Debug.Log("No interaction available.");
        }
    }

    // 타임라인이 끝났을 때 호출받음
    public void End_InstantTimeLine()
    {
        brainController.RollBackBlend();
    }

    // 기습시 천천히 회전
    IEnumerator RotateTowardsTarget(Transform target,float rotationSpeed)
    {
        // 목표 방향이 설정될 때까지 계속 반복합니다.
        while (true)
        {
            // 적의 forward 방향을 얻습니다.
            Vector3 targetForward = target.forward;

            // 적의 forward 방향을 기준으로 캐릭터의 목표 회전값을 계산합니다.
            Quaternion lookRotation = Quaternion.LookRotation(targetForward);

            // 현재 회전과 목표 회전 사이를 부드럽게 보간합니다.
            while (Quaternion.Angle(transform.rotation, lookRotation) > 0.01f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
                yield return null; // 다음 프레임까지 대기
            }

            yield break;
        }
    }

    // 기습 타임라인 메서드
    private void PlayInstantKillTimeline()
    {
        if (playableDirector != null && instantKillTimeline != null)
        {
            // PlayableDirector에 타임라인을 설정
            playableDirector.playableAsset = instantKillTimeline;

            // 타임라인 재생 시작
            playableDirector.Play();
        }
        else
        {
            Debug.LogError("PlayableDirector 또는 TimelineAsset이 할당되지 않았습니다.");
        }
    }

    // 애니메이션에 이벤트 의해 호출되어 원래 상태 회복
    private void End_InstantKill()
    {
        state.SetIdleMode();
    }

    // Collider에서 IInteractable을 찾아 List에 추가하는 메서드
    private void AddToInteractableList(Collider collider)
    {
        if (collider != null)
        {
            IInteractable interactable = collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactables.Add(interactable);
            }
        }
    }

    private class InteractableComparer : IComparer<IInteractable>
    {
        public int Compare(IInteractable x, IInteractable y)
        {
            // 우선순위가 낮은 것부터 높은 것 순으로 정렬
            return x.GetPriority().CompareTo(y.GetPriority());
        }
    }
}
