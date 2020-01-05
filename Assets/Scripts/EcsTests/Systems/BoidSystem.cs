using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Unity.Mathematics;

public class BoidSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
      
         Entities.ForEach((Entity entity, ref Translation translation, ref QuadrantEntityComponent quadrantEntity, ref Rotation rotation, ref BoidComponent boidComponent) => {

             float deltaTime = Time.DeltaTime;

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



             // flipp direction
             if (translation.Value.y > 100f || translation.Value.y < 0f)
                 quadrantEntity.velocity.y *= -1;

             if (translation.Value.z > 250f || translation.Value.z < -250f)
                 quadrantEntity.velocity.z *= -1;

             if (translation.Value.x > 250f || translation.Value.x < -250f)
                 quadrantEntity.velocity.x *= -1;


             // move
             float3 currentPosition = translation.Value;
             float3 targetPosition = currentPosition + quadrantEntity.velocity;

             targetPosition = math.lerp(currentPosition, targetPosition, 0.5f * deltaTime);
             translation.Value = targetPosition;


             // rotate
             float3 lookVector = targetPosition - currentPosition;
             quaternion rotationValue = math.slerp(rotation.Value, quaternion.LookRotationSafe(lookVector, math.up()), 0.75f * deltaTime);
             //quaternion.LookRotationSafe(lookVector, math.up()); //Quaternion.Lerp(Quaternion.LookRotation(lookVector), 0.5f * deltaTime * 3);
             rotation.Value = rotationValue;





         });
        

    

    }
}
