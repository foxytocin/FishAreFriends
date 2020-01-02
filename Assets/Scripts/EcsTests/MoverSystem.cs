using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class MoverSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation, ref MoveSpeedComponent moveSpeedComponent) =>
        {
            translation.Value.y +=  moveSpeedComponent.moveSpeed * Time.DeltaTime;

            if(translation.Value.y > 5f)
            {
                moveSpeedComponent.moveSpeed = -Mathf.Abs(moveSpeedComponent.moveSpeed);
            }

            if (translation.Value.y < 0f)
            {
                moveSpeedComponent.moveSpeed = +Mathf.Abs(moveSpeedComponent.moveSpeed);
            }
        });
    }
}