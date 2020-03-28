using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    public TimeManager timeManager;
    public Spawner spawner;
    public event System.Action resumeLooking;
    public float viewRadius;
    [Range(0, 360)]
    public float ViewAngle;
    Rigidbody rigidBody;
    public float delay = 0.2f;
    public bool searching;
    public float meshResolution;
    RandomMovement movement;
    public LayerMask targetMask;
    public int foodsCollectedToday {get; private set;}
    public float maxEnergy = 200;
    public float energyPerUpdate = 0.1f;
    public float energyPerFood = 100;
    [SerializeField]
    private float currentEnergy = 100;
    public GameObject[] mutations;
    public float[] mutationChance;    
    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();
    bool dayOnGoing;
    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<RandomMovement>();
        timeManager = movement.timeManager;
        timeManager.onDayStart += resumeSearching;
        timeManager.onDayEnd += endOfDay;
        searching = true;
        rigidBody = GetComponent<Rigidbody>();
        // StartCoroutine(findTargetsWithDelay());
    }

    void resumeSearching() {
        searching = true;
        dayOnGoing = true;
        StartCoroutine(findTargetsWithDelay());
    }

    void endOfDay() {
        dayOnGoing = false;
        // print(transform.name+" collected "+foodsCollectedToday+" food today");
        // if(foodsCollectedToday == 0) {
            // spawner.notifyDeath();
            // Destroy(gameObject);
        // }
        if(foodsCollectedToday >=2) {
            replicate();
        }
        foodsCollectedToday = 0;
        StopAllCoroutines();
        searching = false;
    }

    IEnumerator findTargetsWithDelay() {
        while(searching) {
            yield return new WaitForSeconds(delay);
            findVisibleTargets();
        }
        yield break;
    }
    void findVisibleTargets() {
        Vector3 minDistTarget = Vector3.positiveInfinity;
        // print(visibleTargets);
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);
        for (int i=0; i<targetsInViewRadius.Length; i++) {
            Transform target= targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            if(Vector3.Angle(transform.forward, dirToTarget) <= ViewAngle/2) {
                visibleTargets.Add(target);
                Debug.DrawLine(transform.position, target.position, Color.green, 0.2f);
                // print("Target spotted at "+target.position);
                searching = false;
                rigidBody.velocity = Vector3.zero;
                if(Vector3.Distance(transform.position, minDistTarget) > Vector3.Distance(transform.position, target.position)) {
                    minDistTarget = target.position;
                }
            }
        }
        if(!searching) {
            // print("Going to target "+minDistTarget);
            StartCoroutine(GoToTarget(minDistTarget));
        }
    }

    IEnumerator GoToTarget(Vector3 target) {
        while(Mathf.Abs(Vector3.Distance(transform.position, target)) > 2) {
            Vector3 move = Vector3.MoveTowards(rigidBody.position, target, movement.speed * Time.deltaTime);
            rigidBody.MovePosition(move);
            Debug.DrawLine(rigidBody.position, target, Color.green);
            yield return null;
        }
        // print("Reached object!");
        RaycastHit hitInfo;
        Ray ray = new Ray(transform.position, target - transform.position);
        Debug.DrawRay(transform.position, target - transform.position, Color.blue, 1);
        if(Physics.Raycast(ray, out hitInfo, 2, targetMask)) {
            // print("Object is here!");
            foodsCollectedToday++;
            currentEnergy += energyPerFood;
            if(currentEnergy > maxEnergy) {
                currentEnergy = maxEnergy;
            }
            Destroy(hitInfo.transform.gameObject);
        }
        else {
            // print("Seems like object ain't here!");
        }
        searching = true;
        StartCoroutine(findTargetsWithDelay());
        resumeLooking();
        yield break;
    }
    // Update is called once per frame
    void Update()
    {        
        DrawFOV();
    }
    void DrawFOV() {
        int stepCount = Mathf.RoundToInt(ViewAngle * meshResolution);
        float stepAngleSize = ViewAngle/stepCount;
        
        for(int i=0; i<=stepCount; i++) {
            float angle = transform.eulerAngles.y - ViewAngle/2 + stepAngleSize*i;
            Debug.DrawLine(transform.position, transform.position + dirFromAngle(angle, true) * viewRadius, Color.red);
        }
    }
    public Vector3 dirFromAngle(float angleInDegrees, bool global) {
        if(!global) {
            angleInDegrees+=transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees*Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
    void OnDestroy() {
        timeManager.onDayStart -= resumeSearching;
        timeManager.onDayEnd -= endOfDay;
    }
    void FixedUpdate() {
        if(dayOnGoing) {
            currentEnergy -= energyPerUpdate;
        }
        if(currentEnergy <= 0) {
            spawner.notifyDeath();
            Destroy(gameObject);
        }
    }
    void replicate() {
        GameObject toReplicate = gameObject;
        if(mutations.Length > 0) {
            float chance = Random.value;            
            for(int i=0; i<mutations.Length; i++) {
                if(chance < mutationChance[i]) {
                    toReplicate = mutations[i];
                    break;
                }
                chance-=mutationChance[i];
            }
        }
        spawner.spawnCharacter(toReplicate);
    }
}
