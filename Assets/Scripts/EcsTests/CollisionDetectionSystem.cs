using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

public class CollisionDetectionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
     /*   
         Entities.ForEach((Entity entity, ref Translation translation, ref QuadrantEntity quadrantEntity, ref MoveSpeedComponent moveSpeed) => {

             // get all keys, that should be considered
             List<int> hashMapKeyList = QuadrantSystem.GetHashKeysForAroundData(translation.Value);

             QuadrantData nearestQuadrantData = default;
             bool foundNearestQuadrantData = false;
             
             foreach(int hashMapKey in hashMapKeyList)
             {          
                 QuadrantData quadrantData;
                 NativeMultiHashMapIterator<int> nativeMultiHashMapIterator;
                 if (QuadrantSystem.quadrantMultiHashMap.TryGetFirstValue(hashMapKey, out quadrantData, out nativeMultiHashMapIterator))
                 {
                     do
                     {
                         if (quadrantData.entity.Equals(entity))
                             continue;

                         if (!foundNearestQuadrantData)
                         {
                             nearestQuadrantData = quadrantData;
                             foundNearestQuadrantData = true;
                         }
                             

                         if(math.abs(math.distance(nearestQuadrantData.position, translation.Value)) > math.abs(math.distance(quadrantData.position, translation.Value)))
                         {
                             nearestQuadrantData = quadrantData;
                         }
                         

                         // implement here boid stuff
                         // TODO



                     } while (QuadrantSystem.quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
                 }
             }


             // move to nearest neighbour
             if (foundNearestQuadrantData)
             {
                 //Debug.Log("nearest neighbour found: " + nearestQuadrantData.position);

                 if (nearestQuadrantData.position.x > translation.Value.x)
                     moveSpeed.moveSpeedX = math.abs(moveSpeed.moveSpeedX);
                 else
                     moveSpeed.moveSpeedX = -math.abs(moveSpeed.moveSpeedX);

                 if (nearestQuadrantData.position.y > translation.Value.y)
                     moveSpeed.moveSpeedY = math.abs(moveSpeed.moveSpeedY);
                 else
                     moveSpeed.moveSpeedY = -math.abs(moveSpeed.moveSpeedY);

                 if (nearestQuadrantData.position.z > translation.Value.z)
                     moveSpeed.moveSpeedZ = math.abs(moveSpeed.moveSpeedZ);
                 else
                     moveSpeed.moveSpeedZ = -math.abs(moveSpeed.moveSpeedZ);

             }

             


             

         });
        

    */

    }
}
