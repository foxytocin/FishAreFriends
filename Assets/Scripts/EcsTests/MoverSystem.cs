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
            // y
            translation.Value.y +=  moveSpeedComponent.moveSpeedY * Time.DeltaTime;

            if(translation.Value.y > 100f)
                moveSpeedComponent.moveSpeedY = -Mathf.Abs(moveSpeedComponent.moveSpeedY);

            if (translation.Value.y < 0f)
                moveSpeedComponent.moveSpeedY = +Mathf.Abs(moveSpeedComponent.moveSpeedY);

            // z
            translation.Value.z += moveSpeedComponent.moveSpeedZ * Time.DeltaTime;

            if (translation.Value.z > 250f)
                moveSpeedComponent.moveSpeedZ = -Mathf.Abs(moveSpeedComponent.moveSpeedZ);

            if (translation.Value.z < -250f)
                moveSpeedComponent.moveSpeedZ = +Mathf.Abs(moveSpeedComponent.moveSpeedZ);

            // x
            translation.Value.x += moveSpeedComponent.moveSpeedX * Time.DeltaTime;

            if (translation.Value.x > 250f)
                moveSpeedComponent.moveSpeedX = -Mathf.Abs(moveSpeedComponent.moveSpeedX);

            if (translation.Value.x < -250f)
                moveSpeedComponent.moveSpeedX = +Mathf.Abs(moveSpeedComponent.moveSpeedX);


        });
    }
}