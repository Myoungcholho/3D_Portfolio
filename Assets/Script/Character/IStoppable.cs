using System.Collections;

public interface IStoppable
{
    public void Regist_MovableStopper();
    public IEnumerator Start_FrameDelay(int frame);
}
