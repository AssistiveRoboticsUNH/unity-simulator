using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
// using UnityEngine.Rendering;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class CollisionPointsDoor
{
    public int id;
    public double posX;
    public double posY;
    public double posZ;
    public bool empty;
    public bool non_empty;
    public bool door1;
    public bool door2;
    public bool door3;
    public bool door4;
    
}

public class GetCollisionWithDoors :MonoBehaviour
{   
    public int numPoints = 99999; // Number of points to check
    public float rangex = 12f; // Range for random point generation
 
    public float rangey = 1.5f;
  
    public float rangez = 1f;

	public float rangex1 = 5.5f; // Range for random point generation
 
    public float rangey1 = 0.4f;
  
    public float rangez1 = 8f;
    
    public float sphereRadius = 0.01f; // Radius of the collision detection sphere
    
    public List<CollisionPointsDoor> collisionpointsList;
 
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 randomPoint = transform.position + GetRandomPointInRange();
            Gizmos.DrawSphere(randomPoint, sphereRadius);
        }
    }
    
    private void Start()
    {
        CheckCollisionPoints();
        // OnDrawGizmos();
    }
    // private void Update()
    // {
    //     CheckCollisionPoints();
    //     // OnDrawGizmos();
    // }
    
    private void CheckCollisionPoints()
    {    
        List<CollisionPointsDoor> collisionpointsList =new List<CollisionPointsDoor>();
        
        for (int i = 0; i < numPoints; i++)
        {   
            CollisionPointsDoor collisionpoints = new CollisionPointsDoor();
            collisionpoints.empty = false;
            collisionpoints.non_empty = false;
            collisionpoints.door1 = false;
            collisionpoints.door2 = false;
            collisionpoints.door3 = false;
            collisionpoints.door4 = false;
            
            Vector3 randomPoint = transform.position + GetRandomPointInRange();
    
            Collider[] colliders = Physics.OverlapSphere(randomPoint, sphereRadius);
            
            if (colliders.Length > 0)
            {	
                Debug.Log("Collision detected at point: " + randomPoint);
                collisionpoints.non_empty = true;
                for (int j = 0; j < colliders.Length; j++)
                {
                    if ("door1" == colliders[j].gameObject.name)
                    {
                        collisionpoints.id = i;
                        collisionpoints.posX = randomPoint.x;
                        collisionpoints.posY = randomPoint.z;
                        collisionpoints.posZ = randomPoint.y;
                        collisionpoints.door1 = true;
                        break;
                    }
                    else if ("door2" == colliders[j].gameObject.name)
                    {
                        collisionpoints.id = i;
                        collisionpoints.posX = randomPoint.x;
                        collisionpoints.posY = randomPoint.z;
                        collisionpoints.posZ = randomPoint.y;
                        collisionpoints.door2 = true;
                        break;
                    }
                    else if ("door3" == colliders[j].gameObject.name)
                    {
                        collisionpoints.id = i;
                        collisionpoints.posX = randomPoint.x;
                        collisionpoints.posY = randomPoint.z;
                        collisionpoints.posZ = randomPoint.y;
                        collisionpoints.door3 = true;
                        break;
                    }
                    else if ("door4" == colliders[j].gameObject.name)
                    {
                        collisionpoints.id = i;
                        collisionpoints.posX = randomPoint.x;
                        collisionpoints.posY = randomPoint.z;
                        collisionpoints.posZ = randomPoint.y;
                        collisionpoints.door4 = true;
                        break;
                    }
                    else
                    {
                        collisionpoints.id = i;
                        collisionpoints.posX = randomPoint.x;
                        collisionpoints.posY = randomPoint.z;
                        collisionpoints.posZ = randomPoint.y;
                        break;
                    }
                        
                }
            }
            else
            {
                Debug.Log("No collision detected at point: " + randomPoint);
                collisionpoints.id = i;
                collisionpoints.posX = randomPoint.x;
                collisionpoints.posY = randomPoint.z;
                collisionpoints.posZ = randomPoint.y;
                collisionpoints.empty = true;
            }
            
            collisionpointsList.Add(collisionpoints);
        }
        string json = JsonConvert.SerializeObject(collisionpointsList);
        string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        string filePath = Path.Combine(downloadsPath, "collision_door/collision7" +
                                                      ".json");
        File.WriteAllText(filePath, json);
    }

    private Vector3 GetRandomPointInRange()
    {
        float x = UnityEngine.Random.Range(-rangex1, rangex);
        float y = UnityEngine.Random.Range(-rangey1, rangey);
        float z = UnityEngine.Random.Range(-rangez1, rangez);

        return new Vector3(x, y, z);
    }
    
}