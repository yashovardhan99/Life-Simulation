using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class TimeManager : MonoBehaviour
{
    public GameObject plane;
    public float dayInSeconds;
    int day;
    public event System.Action onDayStart;
    public event System.Action onDayEnd;
    public float timeScale = 1;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(ManageTime());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator ManageTime() {
        while(true) {
            while(!Input.GetKeyDown(KeyCode.Space)){
                yield return null;
            }
            if(Time.timeScale != timeScale) {
                Time.timeScale = timeScale;
            }
            print("Day "+day+" started");
            if(onDayStart != null) {
                onDayStart();
            }
            yield return new WaitForSeconds(dayInSeconds);
            print("Day "+day+" ended");
            if(onDayEnd != null) {
                onDayEnd();
            }
            day++;
        }
    }
}
