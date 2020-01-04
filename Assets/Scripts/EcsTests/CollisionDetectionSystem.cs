using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class CollisionDetectionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        
         Entities.ForEach((Entity entity, ref Translation translation, ref QuadrantEntity quadrantEntity) => {

             // get all keys, that should be considered
             List<int> hashMapKeyList = QuadrantSystem.GetHashKeysForAroundData(translation.Value);
             
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

                         // implement here boid stuff
                         // TODO



                     } while (QuadrantSystem.quadrantMultiHashMap.TryGetNextValue(out quadrantData, ref nativeMultiHashMapIterator));
                 }
             }

             

         });
        



    }
}
