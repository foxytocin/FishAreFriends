using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

public class RotationSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Rotate rotate, ref RotationEulerXYZ euler) =>
        {
            euler.Value.y += rotate.radiansPerSeconds * Time.DeltaTime;
        });
    }
}
