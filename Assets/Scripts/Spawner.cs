using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    TimeManager timeManager;
    public int foodCount;
    public int charactersCount;
    public GameObject characterPrefab;
    public GameObject foodPrefab;
    public List<GameObject> foods;
    public GameObject plane;
    float maxX, maxZ;
    public int currentCharacterCount {get; private set;}
    // Start is called before the first frame update
    void Start()
    {
        currentCharacterCount = 0;
        Bounds bounds = plane.GetComponent<MeshRenderer>().bounds;
        maxX = bounds.extents.x;
        maxZ = bounds.extents.z;
        foods = new List<GameObject>(foodCount);
        timeManager = GetComponent<TimeManager>();
        timeManager.onDayStart += startspawn;
        timeManager.onDayEnd += destroyUnusedFood;

        for(int i=0; i<charactersCount; i++) {
            spawnCharacter(characterPrefab);
        }
    }
    void startspawn() {
        for(int i=0; i<foodCount;i++) {
            GameObject food = Instantiate(foodPrefab, getRandomPosition(true), getRandomRotation());
            foods.Add(food);
        }
    }
    Vector3 getRandomPosition(bool food) {
        Vector3 pos = new Vector3();
        if(!food) {
            if(Random.value < 0.5) {
                pos.x = -(Random.Range(maxX-5, maxX-5.5f));
            }
            else {
                pos.x = (Random.Range(maxX-5, maxX-5.5f));
            }
            pos.z = Random.Range(- maxZ+5, maxZ - 5);
            pos.y = 1;
            }
        else {
            Vector2 xz = Random.insideUnitCircle;
            pos.x = xz.x * (maxX - 10);
            pos.z = xz.y * (maxZ - 10);
            pos.y = 0;
        }
        return pos;
    }
    Quaternion getRandomRotation() {
        Quaternion rot = Quaternion.Euler(0, Random.Range(0, 360), 0);
        return rot;
    }
    void destroyUnusedFood() {
        foreach (GameObject food in foods) {
            if(food != null) {
                Destroy(food);
            }
        }
        foods.Clear();
    }
    // Update is called once per frame
    public void notifyDeath() {
        // print(currentCharacterCount);
        currentCharacterCount--;
    }
    public void spawnCharacter(GameObject type) {
        GameObject character = Instantiate(type, getRandomPosition(false), getRandomRotation());
        character.GetComponent<RandomMovement>().timeManager = timeManager;
        character.GetComponent<FieldOfView>().spawner = this;
        currentCharacterCount++;
    }
}   
