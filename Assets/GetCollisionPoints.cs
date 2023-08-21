using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
// using UnityEngine.Rendering;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

public class CollisionPoints
{
    public int id;
    public double posX;
    public double posY;
    public double posZ;
    public bool collision;
    public string name_collision;
    
}

public class GetCollisionPoints :MonoBehaviour
{   
    public int numPoints = 99999; // Number of points to check
    public float rangex = 12f; // Range for random point generation
 
    public float rangey = 1.5f;
  
    public float rangez = 1f;

	public float rangex1 = 5.5f; // Range for random point generation
 
    public float rangey1 = 0.4f;
  
    public float rangez1 = 8f;
    
    public float sphereRadius = 0.01f; // Radius of the collision detection sphere
    
    public List<CollisionPoints> collisionpointsList;
 
    
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
    
    private void CheckCollisionPoints()
    {    
        List<CollisionPoints> collisionpointsList =new List<CollisionPoints>();
        
        for (int i = 0; i < numPoints; i++)
        {   
            CollisionPoints collisionpoints = new CollisionPoints();
            Vector3 randomPoint = transform.position + GetRandomPointInRange();
    
            Collider[] colliders = Physics.OverlapSphere(randomPoint, sphereRadius);
            
            if (colliders.Length > 0)
            {	
                Debug.Log("Collision detected at point: " + randomPoint);
				bool collision_door = false;
                for (int j = 0; j < colliders.Length; j++)
                {	if ("door_modern Variant" == colliders[j].gameObject.name)
					{
						collision_door = true;
					}     
                }
				if (collision_door)
				{
					
                    collisionpoints.id = i;
                    collisionpoints.posX = randomPoint.x;
                    collisionpoints.posY = randomPoint.z;
                    collisionpoints.posZ = randomPoint.y;
                    collisionpoints.collision = true;
                    collisionpoints.name_collision = "door";

				}
				else {
					
                    collisionpoints.id = i;
                    collisionpoints.posX = randomPoint.x;
                    collisionpoints.posY = randomPoint.z;
                    collisionpoints.posZ = randomPoint.y;
                    collisionpoints.collision = true;
                    collisionpoints.name_collision = "not_door" ;
				}
            }
            else
            {
                Debug.Log("No collision detected at point: " + randomPoint);
                collisionpoints.id = i;
                collisionpoints.posX = randomPoint.x;
                collisionpoints.posY = randomPoint.z;
                collisionpoints.posZ = randomPoint.y;
                collisionpoints.collision = false;
                collisionpoints.name_collision = "None";

            }
            collisionpointsList.Add(collisionpoints);
        }
        string json = JsonConvert.SerializeObject(collisionpointsList);
        string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory));
        string filePath = Path.Combine(downloadsPath, "collisionpts_hewitthall/collision4" +
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