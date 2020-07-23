using Ballance2;
using UnityEngine;

public class SpeedTestUtils : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
        if (ticking)
            tick += Time.deltaTime;
    }

    private float tick = 0;
    private bool ticking = false;

    private SpeedTestUtils TestStart = null;
    public SpeedTestUtils TestEnd  = null;
    public string TestObjectName = "";
    public float TestLength = 50;

    private void StartTick()
    {
        ticking = true;
        tick = 0;
    }
    private float EndTick()
    {
        ticking = false;
        float t = tick;
        tick = 0;
        return t;
    }
    private void SetTickSource(SpeedTestUtils s)
    {
        TestStart = s;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name == TestObjectName)
        {
            if (TestEnd == null)
            {
                if (TestStart != null)
                {
                    float sec = TestStart.EndTick();
                    if (sec > 0)
                    {
                        float speed = TestLength / sec;
                        GameLogger.Log("SpeedTestUtils", TestObjectName + " Speed : " + speed + " m/s  time : " + sec + " second");
                    }
                }
            }
            else
            {
                GameLogger.Log("SpeedTestUtils", TestObjectName + " Start test");
                StartTick();
                TestEnd.SetTickSource(this);
            }
        }
    }
}
