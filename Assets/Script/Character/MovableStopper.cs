using System.Collections.Generic;
using UnityEngine;

public class MovableStopper : MonoBehaviour
{
    private static MovableStopper m_instance;
    public static MovableStopper Instance
    {
        get
        {
            return m_instance;
        }
    }

    private List<IStoppable> stoppers = new List<IStoppable>();

    private void Awake()
    {
        if(m_instance == null)
        {
            m_instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    public void Regist(IStoppable stopper)
    {
        stoppers.Add(stopper);
    }

    public void Remove(IStoppable stopper) 
    {
        stoppers.Remove(stopper);
    }

    public void Start_Delay(int frame)
    {
        if (frame < 1)
            return;

        foreach(IStoppable stopper in stoppers)
        {
            StartCoroutine(stopper.Start_FrameDelay(frame));
        }
    }

}
