using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public struct MoveSpeedComponent : IComponentData
{
    public float moveSpeedX;
    public float moveSpeedY;
    public float moveSpeedZ;
    public float3 targetPosition;
}
