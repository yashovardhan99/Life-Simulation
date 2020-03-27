using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RandomMovement : MonoBehaviour
{
    Vector3 direction;
    public float speed = 10;
    public float getDirTime = 1;
    bool getNewDir;
    public Rigidbody rigidBody;
    FieldOfView fieldOfView;
    public TimeManager timeManager;
    Vector3 initialPos;
    Quaternion initialRot;

    // Start is called before the first frame update
    void Start()
    {
        initialPos = transform.position;
        initialRot = transform.rotation;
        timeManager.onDayStart += startDay;
        timeManager.onDayEnd += endDay;
        fieldOfView = GetComponent<FieldOfView>();
        fieldOfView.resumeLooking += resumeLooking;
        rigidBody = GetComponent<Rigidbody>();
        // startDay();
    }

    void startDay() {
        resumeLooking();      
    }
    void endDay() {
        StopAllCoroutines();
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        transform.rotation = initialRot;
        transform.position = initialPos;
        getNewDir = false;
    }
    void FixedUpdate() {
        if(!fieldOfView.searching) {
            getNewDir = false;
        }
    }
    IEnumerator getDirection() {
        while(getNewDir) {
            Vector3 dir = Random.insideUnitSphere;
            direction = new Vector3(dir.x, 0, dir.z);
            // print(direction);
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.velocity = direction.normalized * speed;
            rigidBody.MoveRotation(Quaternion.LookRotation(direction, transform.up));
            yield return new WaitForSeconds(getDirTime);    
        }
        yield break;
    }
    void resumeLooking() {
        // print("Resuming to look..");
        getNewDir = true;
        StartCoroutine(getDirection());
    }
    void OnDestroy() {
        timeManager.onDayStart -= startDay;
        timeManager.onDayEnd -= endDay;
        fieldOfView.resumeLooking -=resumeLooking;
    }
}
