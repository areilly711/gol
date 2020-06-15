﻿using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;


public class PointInAABSystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray r = Camera.main.ScreenPointToRay(Input.mousePosition);
            Vector3 converted = r.origin;

            float3 point = new float3(converted.x, converted.y, 0.5f);

            //JobHandle jobHandle = job.Schedule(this, inputDeps);
            JobHandle jobHandle = Entities.ForEach((Entity entity, int entityInQueryIndex, ref BoundingBox b, ref Scale s) =>
            {
                if (b.box.Contains(point))
                {
                    s.Value += 0.1f;
                    b.box.Extents = s.Value * 0.5f;
                }
            }).Schedule(inputDeps);
            return jobHandle;
        }
        else
        {
            return inputDeps;
        }
    }
}

public struct BoundingBox : IComponentData
{
    public AABB box;
}