using System.Collections;

public interface IStoppable
{
    public void Regist_MovableStopper();
    public void Remove_MovableStopper();
    public void SetAnimationSpeed(float speed);
    public IEnumerator Start_FrameDelay(int frame);

    // 멈출 Frame, 애니메이션 속도 speed
    IEnumerator Start_FrameDelay(int frame, float speed);

    // 캐릭터는 TRUE를 반환, 적은 FALSE를 반환
    bool IsSpecialObject();
}