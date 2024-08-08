using System.Collections;

public interface IStoppable
{
    public void Regist_MovableStopper();
    public void Remove_MovableStopper();
    public void SetAnimationSpeed(float speed);
    public IEnumerator Start_FrameDelay(int frame);

    // ���� Frame, �ִϸ��̼� �ӵ� speed
    IEnumerator Start_FrameDelay(int frame, float speed);

    // ĳ���ʹ� TRUE�� ��ȯ, ���� FALSE�� ��ȯ
    bool IsSpecialObject();
}