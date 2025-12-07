# 📌프로젝트 개요
<p align="center">
  <img
    width="800"
    src="https://github.com/user-attachments/assets/ec6bebb4-e8f3-49d5-bb1b-de41042f192e"
    alt="unity3D" />
</p>

Unity 기반 3D 액션 프로젝트로, 플레이어 전투, 스킬, 회피 연출, 퀘스트 등 제가 즐겨 하던 게임들의 핵심 콘텐츠를 직접 구현한 프로젝트입니다.  

커맨드 패턴으로 여러 무기를 공통 인터페이스로 관리하고, 공통 기능은 계층으로 분리하며 상호작용 가능한 요소는 인터페이스로 추상화하는 등 재사용 가능한 구조를 목표로 설계·구현했습니다.



| 항목 | 내용 |
|------|------|
| 📹 소개 영상 | [📎포트폴리오 영상](https://www.youtube.com/watch?v=rzw9Piw8OtU&t=8s) |
| 🕒 개발 기간 | 2024.06.26 ~ 2024.11.04 (131일) |
| 👤 개발 인원 | 1명 |
| 🧰 개발 환경 | C# |
| 🛠 실행 및 디버깅 툴 | Unity |

</br>

# 📘목차
- [구현 내용](#구현-내용-목차-이동)
- [핵심 주요 코드](#핵심-주요-코드-목차-이동)
- [문제 해결 경험(트러블 슈팅)](#문제-해결-경험트러블-슈팅-목차-이동)
- [프로젝트에서 얻은 것](#프로젝트에서-얻은-것-목차-이동)
- [개발 계기](#개발-계기-목차-이동)

</br>

# 📘구현 내용 [(목차 이동)](#목차)

| 상세 설명 링크 | 구현 요약 |
|----------------------|------------------|
| 🧱 플레이어 & 전투 코어 | 움직임, 무기 관리 시스템, 무기별 스킬, 회피, 저스트 회피, 암살 등 전투 기능, 타겟팅, 마법진 |
| 🌍 전투 연출 | 슬로우 모션, 잔상, 데미지 폰트, 카메라 연출 시스템 |
| 🎨 적 AI | 보스 전투 패턴 구현, 등장/사망 시네마틱 연출 |
| 🕺 UI&상호작용 | 스테미너 UI, 스킬창 UI, NPC 대화, 퀘스트 시스템 |

</br>

# 📘핵심 주요 코드 [(목차 이동)](#목차)

| 코드 파일 | 코드 설명 |
|----------|-----------|
| Enemy [.cs](https://github.com/Myoungcholho/3D_Portfolio/blob/master/Assets/Script/Character/Enemy.cs) | Enemy의 베이스 클래스로, 피격 처리와 AI 스크립트 연동 등 적 공통 데이터를 관리합니다. |
| SoundManager [.cs](https://github.com/Myoungcholho/3D_Portfolio/blob/master/Assets/Script/Sound/SoundManager.cs) | 오디오 게임 오브젝트를 풀링해 관리하는 사운드 시스템으로, 필요 시 가져다 쓰고 반환하는 방식을 사용하고 있습니다. |
| WeaponComponent [.cs](https://github.com/Myoungcholho/3D_Portfolio/blob/master/Assets/Script/Component/WeaponComponent.cs) | 커맨드 패턴을 활용한 무기 관리 컴포넌트로, 여러 종류의 무기를 공통된 다형성 메서드로 제어합니다. |
| AIController [.cs](https://github.com/Myoungcholho/3D_Portfolio/blob/master/Assets/Script/AI/AIController.cs) | AI의 공격 딜레이와 내부 FSM을 통해 행동 패턴을 결정하는, 적 AI의 핵심 제어 클래스입니다.  |
| Quest [.cs](https://github.com/Myoungcholho/3D_Portfolio/blob/master/Assets/Script/Quest/Quest.cs) | QuestID, 선행 퀘스트, 목표 NPC·몬스터, 대사처럼 퀘스트를 정의하는 데이터와 수락 여부, 완료 여부, 처치 카운트 같은 진행 상태를 함께 관리하는 ScriptableObject입니다. |

</br>

# 📘문제 해결 경험(트러블 슈팅) [(목차 이동)](#목차)

<table style="border-collapse:collapse;">
  <tr>
    <th width="350" style="border:2px solid #ffb3b3; background:#ffe1e1;">
      📂 같은 퀘스트를 인식하지 못하는 문제
    </th>
    <th width="350" style="border:2px solid #ffd27f; background:#fff1d6;">
      📚 자동 타겟팅 종료 후 카메라가 타겟팅 이전 회전 값으로 되돌아가는 문제
    </th>
    <th width="350" style="border:2px solid #c3c3ff; background:#e9e9ff;">
      🧾 런타임에서 Transparent로 변경했지만, 알파가 먹지 않는 문제
    </th>
  </tr>

  <tr>
    <td width="350" style="border:2px solid #ffb3b3; background:#ffe1e1; vertical-align:top;">
      퀘스트를 확인하지 못하는 문제를, ID 기반으로 재모델링해 동일 퀘스트를 인식 가능하도록 해결했습니다.
      <br><br>
      <a href="#t0">[상세설명]</a>
    </td>
    <td width="350" style="border:2px solid #ffd27f; background:#fff1d6; vertical-align:top;">
      자동 타겟팅 이후 카메라와 캐릭터 회전이 동기화되지 않아 생기던 시점 문제를, 회전 기준을 통일해 동기화함으로써 해결했습니다.
      <br><br>
      <a href="#t1">[상세설명]</a>
    </td>
    <td width="350" style="border:2px solid #c3c3ff; background:#e9e9ff; vertical-align:top;">
      런타임에 머티리얼을 투명 타입으로 변경해도 투명도가 변하지 않던 문제를 Unity 렌더 파이프라인 상태까지 함께 갱신하는 방식으로 해결했습니다.
      <br><br>
      <a href="#t2">[상세설명]</a>
    </td>
  </tr>
</table>

<br>

<table style="border-collapse:collapse;">
  <tr>
    <th width="350" style="border:2px solid #a8ddff; background:#e6f6ff;">
      📘 앞으로 점프하는 Fist 스킬이 벽을 통과하거나 벽면에 딱 붙어 멈추는 문제
    </th>
    <th width="350" style="border:2px solid #c8ffa8; background:#ebffdf;">
      📂 다단히트 타이밍 문제
    </th>
  </tr>

  <tr>
    <td width="350" style="border:2px solid #a8ddff; background:#e6f6ff; vertical-align:top;">
      이동형 스킬이 벽을 통과하거나 벽에 달라붙는 문제를 물리 기반 이동이 아닌 Ray 기반 사전 충돌 예측 방식으로 해결했습니다.
      <br><br>
      <a href="#t3">[상세설명]</a>
    </td>
    <td width="350" style="border:2px solid #c8ffa8; background:#ebffdf; vertical-align:top;">
      피격 시점이 서로 다른데도 동일한 타이밍에 피격되던 문제를 Collider 감지 기반이 아닌 시간 기반 히트 관리 방식으로 바꾸어 해결했습니다.
      <br><br>
      <a href="#t4">[상세설명]</a>
    </td>
  </tr>
</table>

---

## 1. 퀘스트를 인식하지 못하는 문제 <a id="t0"></a> [(트러블 슈팅 목록 이동)](#문제-해결-경험트러블-슈팅-목차-이동)

<table>
  <tr>
    <td style="border:2px solid #4fa3ff; border-radius:8px; padding:12px 16px; background:#050812;">
      <strong>🧩 문제</strong>
      <ul>
        <li> 같은 퀘스트가 다른 참조로 인식돼 중복 등록되는 문제가 발생함 </li>
      </ul>
      <strong>🔍 원인 분석</strong>
      <ul>
        <li> 같은 퀘스트라도 매번 새 ScriptableObject 인스턴스로 생성해 서로 다른 객체로 취급된 것이 원인 </li>
      </ul>
      <strong>🛠 해결</strong><br>
      <ul>
        <li>퀘스트마다 고유 QuestID를 부여하고, 참조가 아닌 ID 기반 비교로 동일 퀘스트 여부를 판단하도록 수정함 </li>
      </ul>
      <strong>✅ 결과</strong><br>
      <ul>
        <li>같은 퀘스트가 중복 등록되고, 인식하지 못하는 문제를 해소 </li>
        <li>퀘스트 완료·보상 처리 로직도 의도대로 안정적으로 동작 </li>
      </ul>
      <strong>📚 배운 점</strong>
      <ul>
       <li>List 기반으로 퀘스트를 관리하면 비효율적이라는 것을 직접 겪으며, 상황에 맞는 컨테이너 선택의 중요성을 깨달음</li>
       <li>이 경험을 바탕으로 이후 언리얼 프로젝트의 아이템 관리는 처음부터 해시 기반 컨테이너로 설계함</li>
      </ul>
    </td>
  </tr>
</table>

---

## 2. 자동 타겟팅 종료 후 카메라가 타겟팅 이전 회전 값으로 되돌아가는 문제 <a id="t1"></a> [(트러블 슈팅 목록 이동)](#문제-해결-경험트러블-슈팅-목차-이동)

<table>
  <tr>
    <td style="border:2px solid #ffd27f; border-radius:8px; padding:12px 16px; background:#120d05;">
      <strong>🧩 문제</strong>
      <ul>
        <li> 자동 타겟팅 후, 원래 바라보던 방향으로 카메라가 튀듯이 되돌아가는 문제가 발생 </li>
      </ul>
      <strong>🔍 원인 분석</strong>
      <ul>
        <li> 타겟팅 동안 캐릭터 회전만 갱신되고, 마우스 입력 기준 회전 값은 갱신되지 않았던 것이 문제 </li>
      </ul>
      <strong>🛠 해결</strong><br>
      <ul>
        <li> 타겟팅 종료 시점에, 타겟을 향해 변경된 회전 값을 내부 캐시와 실제 트랜스폼에 동시에 반영하도록 수정 </li>
      </ul>
      <strong>✅ 결과</strong><br>
      <ul>
        <li> 타겟팅 해제 시 화면 튐 현상이 사라지고, 카메라 움직임이 자연스러워짐 </li>
      </ul>
      <strong>📚 배운 점</strong>
      <ul>
        <li> 핵심은 회전 기준을 하나로 통일하는 것이 좋다는 것을 배움 </li>
        <li> 값을 억지로 동기화하기보다, 애초에 동기화 지점이 적은 구조로 설계하는 것이 더 중요하다는 걸 깨달음 </li>
      </ul>
    </td>
  </tr>
</table>

---

## 3. 런타임에서 Transparent로 변경했지만, 알파가 먹지 않는 문제 <a id="t2"></a> [(트러블 슈팅 목록 이동)](#문제-해결-경험트러블-슈팅-목차-이동)

<table>
  <tr>
    <td style="border:2px solid #a8ddff; border-radius:8px; padding:12px 16px; background:#050a12;">
      <strong>🧩 문제</strong><br>
      <ul>
        <li> 런타임에 머티리얼 타입을 투명으로 변경했지만, 알파 값을 조정해도 투명해지지 않는 문제가 발생 </li>
      </ul>
      <strong>🔍 원인 분석</strong><br>
      <ul>
        <li> 인스펙터에서 물체의 타입을 변경하면 다양한 렌더링 상태가 함께 변경되는 것을 공식 문서를 통해 확인함 </li>
        <li> 즉 런타임에서 코드로 변경했을 때는 물체의 렌더링 상태가 여전히 불투명에 가까웠기 때문 </li>
      </ul>
      <strong>🛠 해결</strong>
      <ul>
        <li> 런타임에 투명 머티리얼이 정상적으로 동작하도록, 필요한 렌더 상태를 모두 명시적으로 설정함 </li>
      </ul>
      <strong>✅ 결과</strong><br>
      <img width="300" height="209" alt="image" src="https://github.com/user-attachments/assets/0c2e7075-8d5e-45ed-9f90-37ec1d9a7bb0" />
      <ul>
        <li> 투명 처리 문제를 해결했고, 동일한 방식으로 잔상 기능에도 적용함 </li>
      </ul>
      <strong>📚 배운 점</strong>
      <ul>
        <li> 게임 프로그래머라도 콘텐츠 로직만 알아서는 부족하고, 렌더링 파이프라인 기본 이해가 필수라는 점 </li>
      </ul>
    </td>
  </tr>
</table>

---

## 4. 앞으로 점프하는 Fist 스킬이 벽을 통과하거나 벽면에 딱 붙어 멈추는 문제 <a id="t3"></a> [(트러블 슈팅 목록 이동)](#문제-해결-경험트러블-슈팅-목차-이동)

<table>
  <tr>
    <td style="border:2px solid #4fa3ff; border-radius:8px; padding:12px 16px; background:#050812;">
      <strong>🧩 문제</strong><br>
      <img width="300" height="209" alt="image" src="https://github.com/user-attachments/assets/9b50ed26-0a07-45f7-8273-0d5f97ff5569" />
      <ul>
        <li> 돌진하는 스킬에서 캐릭터가 벽을 통과해버리거나 벽에 딱 달라붙는 등 부자연스럽게 정지하는 문제가 발생 </li>
      </ul>
      <strong>🔍 원인 분석</strong>
      <ul>
        <li> 한 프레임에 이동 거리가 너무 길어, 얇은 벽 콜라이더를 물리 엔진이 건너뛰는 터널링 현상이 문제 </li>
        <li> 애니메이션으로 움직이는 몸의 위치를 고려하지 않은 채, 충돌 지점을 그대로 목표 지점으로 사용한 것도 문제 </li>
      </ul>
      <strong>🛠 해결</strong></br>
      <img width="300" height="209" alt="image" src="https://github.com/user-attachments/assets/247894b9-277b-4956-9662-f773fd49414c" />
      <ul>
        <li> 이동 우선에서 충돌 예측 우선 구조로 전환해, 충돌 결과에 따라 이동을 제한함 </li>
        <li> 전방 Ray로 충돌 지점을 찾고, 그 거리까지만 이동하는 방식 사용 </li>
      </ul>
      <strong>✅ 결과</strong><br>
      <ul>
        <li> 스킬 사용 시 캐릭터가 얇은 벽을 관통하거나 콜라이더를 뚫는 현상이 사라짐 </li>
        <li> Ray 기반 검증 패턴은, 캐스팅 전 마법 위치 선정 등 다른 기능에도 응용할 수 있는 기반이 됨 </li>
      </ul>
      <strong>📚 배운 점</strong>
      <ul>
        <li> 충돌 결과를 사후 처리하는 대신, 이동·위치 결정의 기준으로 쓸 때 Ray가 유용하다는 것을 체감 </li>
        <li> 필요할 때만 한 번 선형 계산으로 검사하는 Ray를 통해, 물리 연산 부담을 줄일 수 있다는 점을 깨달음</li>
      </ul>
    </td>
  </tr>
</table>

---

## 5. 다단히트 타이밍 문제 <a id="t4"></a> [(트러블 슈팅 목록 이동)](#문제-해결-경험트러블-슈팅-목차-이동)

<table>
  <tr>
    <td style="border:2px solid #c8ffa8; border-radius:8px; padding:12px 16px; background:#060f06;">
      <strong>🧩 문제</strong>
      <ul>
        <li> 장판형 유지 스킬에서 실제 피격 시점은 다름에도, 피격 타이밍이 전부 동시에 들어가는 문제가 발생 </li>
        <li> 다단히트를 5회로 의도했지만, 6회 이상 히트가 발생하는 현상이 나타남 </li>
      </ul>
      <strong>🔍 원인 분석</strong><br>
      <ul>
        <li> Collider를 주기적으로 On/Off하는 방식이라, 피격 타이밍이 전부 같은 시점에 몰려 부자연스러웠음 </li>
        <li> FixedUpdate에서 매우 짧은 딜레이(0.05초)로 판정을 반복하면서, 시점이 겹쳐 1회 더 들어가는 문제가 발생 </li>
      </ul>
      <strong>🛠 해결</strong><br>
      <ul>
        <li> 다단히트 기준을 “Collider 감지 여부”가 아니라, 피격 시점을 기록하는 시간 관리 방식으로 변경 </li>
        <li> 내부에 카운트 변수를 두어 피격 최대 횟수를 제한해, 1회 더 들어가는 현상을 제거 </li>
      </ul>
      <strong>✅ 결과</strong><br>
      <ul>
        <li> 몬스터가 여러 마리여도 각자 개별적인 피격 타이밍이 적용되도록 개선됨 </li>
        <li> 의도한 간격으로, 의도한 횟수만큼 정확하게 데미지가 들어가도록 수정됨 </li>
      </ul>
      <strong>📚 배운 점</strong>
      <ul>
        <li> 물리 틱에 직접 의존한 설계는 잘못된 판정을 유발할 수 있다는 점을 깨달음 </li>
        <li> FixedUpdate는 충돌 신호만 담당하게 두고, 히트 판정·타이밍은 별도로 분리하는 것이 좋다는 구조적 한계를 체감함 </li>
      </ul>
    </td>
  </tr>
</table>

---

# 📘프로젝트에서 얻은 것 [(목차 이동)](#목차)

| 번호 | 얻은 경험 |
|------|-----------|
| 1 | [렌더링 파이프라인 이해의 중요성](#gain-drawcall) |
| 2 | [액션 로직 구현과 디자인 패턴 적용](#gain-ue-arch) |
| 3 | [3D 과정에서 새로 배운 것들](#gain-cpp-resource) |

---

### 1. 렌더링 파이프라인 이해의 중요성 <a id="gain-drawcall"></a> [(⬆표로 이동)](#프로젝트에서-얻은-것-목차-이동)

특히 런타임에 머티리얼을 투명 객체로 변경했는데도 제대로 적용되지 않는 문제를 겪으면서 렌더링 파이프라인에 대한 이해가 단순한 엔진 지식이 아니라 **실제 콘텐츠 개발 결과와 직결되는 요소**라는 것을 몸으로 느낄 수 있었습니다.

---

### 2. 액션 로직 구현과 디자인 패턴 적용 <a id="gain-ue-arch"></a> [(⬆표로 이동)](#프로젝트에서-얻은-것-목차-이동)

액션 게임에서 **공격 중 재입력 제한**, **다수 무기 관리** 같은 로직을 구현하면서 이를 정리하기 위해 커맨드 패턴을 적용했습니다.

이 과정을 통해 디자인 패턴을 단순 이론이 아니라 **실제 문제를 정리하고 해결하는 실전 도구**로 체득할 수 있었습니다.

---

### 3. 3D 과정에서 새로 배운 것들 <a id="gain-cpp-resource"></a> [(⬆표로 이동)](#프로젝트에서-얻은-것-목차-이동)

3D 개발에 진입하면서 키프레임, 블렌딩, 애니메이션 이벤트를 직접 활용해 보고, 여기에 더해 **루트 모션에 대한 이해까지 쌓을 수 있었던 경험**이었습니다.

</br>

# 📘개발 계기 [(목차 이동)](#목차)
### 1. 2D를 넘어, 3D RPG 콘텐츠 개발에 도전

평소 즐겨 하던 게임들처럼 3D 애니메이션을 활용한 액션과 연출을 직접 구현해 보고 싶었습니다.  
단순히 2D에서 하던 것을 반복하는 대신, 카메라·애니메이션·공간 구성처럼 3D에서만 마주하는 고민과 경험을 쌓기 위해 프로젝트를 진행했습니다.
