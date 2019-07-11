﻿using System.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;

namespace OneVsMany
{
    public class GameHandler : MonoBehaviour
    {
        const int NumBullets = 25;
        const int MaxHealth = 100;
        public float bulletDamage = 5;
        public int numEnemies = 1;
        public float enemySpeed = 0.02f;
        public float foodSpawnInterval = 20;
        public float healthDegenRate = 1;
        public Mesh mesh;
        public Material playerMat;
        public Material enemyMat;
        public Material bulletMat;
        public Material foodMat;
        public Hud hud;

        // Start is called before the first frame update
        EntityManager entityManager;
        public static Entity playerEntity;

        void Start()
        {
            entityManager = World.Active.EntityManager;

            entityManager.World.GetOrCreateSystem<PlayerUpdateSystem>().Init(healthDegenRate, hud);

            CreatePlayer();
            CreateEnemies(numEnemies);
            CreateBullets(NumBullets);
            StartCoroutine(SpawnFood(foodSpawnInterval));
        }

        public void GameOver()
        {

            entityManager.World.GetExistingSystem<PlayerUpdateSystem>().Enabled = false;
            entityManager.World.GetExistingSystem<MovementSystem>().Enabled = false;
            entityManager.World.GetExistingSystem<CollisionSystem>().Enabled = false;
        }

        public void Restart()
        {
            entityManager.World.GetExistingSystem<PlayerUpdateSystem>().Enabled = true;
            entityManager.World.GetExistingSystem<MovementSystem>().Enabled = true;
            entityManager.World.GetExistingSystem<CollisionSystem>().Enabled = true;
        }

        IEnumerator SpawnFood(float interval)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                CreateFood(1, entityManager.GetComponentData<Translation>(playerEntity).Value);
            }
        }

        private void Update()
        {
            //if (Input.GetKeyDown(KeyCode.Space))
            //{
            //    CreateFood(1, entityManager.GetComponentData<Translation>(playerEntity).Value);
            //}

            //Health playerHealth = entityManager.GetComponentData<Health>(playerEntity);
            //hud.SetHealth(playerHealth.curr);

            //if (playerHealth.curr <= 0)
            //{
            //    // game over
            //}
        }

        private void LateUpdate()
        {
            //Camera.main.transform.LookAt(entityManager.GetComponentData<Translation>(playerEntity).Value);
        }

        void CreatePlayer()
        {
            playerEntity = entityManager.CreateEntity(
               typeof(Movement),
               typeof(Translation),
               typeof(LocalToWorld),
               typeof(RenderMesh),
               typeof(Scale),
               typeof(BoundingVolume),
               typeof(Health),
               typeof(Player),
               typeof(PlayerSystemState)
           );

            entityManager.SetComponentData<Movement>(playerEntity, new Movement { speed = 5 });
            
            hud.SetMaxHealth(MaxHealth);
            InitHealth(playerEntity, MaxHealth, MaxHealth);
            InitRenderData(playerEntity, new float3(0), 1, mesh, playerMat);
        }

        void CreateEnemies(int numEnemies)
        {
            for (int i = 0; i < numEnemies; i++)
            {
                Entity e = entityManager.CreateEntity(
                    typeof(Movement),
                    typeof(Translation),
                    typeof(LocalToWorld),
                    typeof(RenderMesh),
                    typeof(Scale),
                    typeof(BoundingVolume),
                    typeof(Health),
                    typeof(HealthModifier),
                    typeof(Enemy)
                );

                entityManager.SetComponentData<Movement>(e, new Movement { speed = 1 });
                InitHealth(e, 10, 10);
                InitHealthModifier(e, -10/*MaxHealth*/);
                InitRenderData(e, CreateRandomSpawnPosition(Vector2.zero, 0, 5), 0.5f, mesh, enemyMat);
            }
        }

        Vector3 CreateRandomSpawnPosition(Vector3 center, float minR, float maxR)
        {
            Vector2 spawnPos = UnityEngine.Random.insideUnitCircle * minR;
            spawnPos += UnityEngine.Random.insideUnitCircle * maxR;
            Vector3 final = spawnPos;
            final += center;
            return spawnPos;
        }

        void CreateBullets(int numBullets)
        {
            for (int i = 0; i < numBullets; i++)
            {
                Entity e = entityManager.CreateEntity(
                    typeof(Movement),
                    typeof(Translation),
                    typeof(LocalToWorld),
                    typeof(RenderMesh),
                    typeof(Scale),
                    typeof(BoundingVolume),
                    typeof(HealthModifier),
                    typeof(Bullet)
                );

                InitHealthModifier(e, -bulletDamage);
                entityManager.SetComponentData<Movement>(e, new Movement { speed = enemySpeed });
                InitRenderData(e, new float3(1000, 0, 0), 0.1f, mesh, bulletMat);
            }
        }

        void CreateFood(int numFood, float3 playerPos)
        {
            for (int i = 0; i < numFood; i++)
            {
                Entity e = entityManager.CreateEntity(
                    typeof(Translation),
                    typeof(LocalToWorld),
                    typeof(RenderMesh),
                    typeof(Scale),
                    typeof(BoundingVolume),
                    typeof(HealthModifier),
                    typeof(Food)
                );

                InitHealthModifier(e, 50);
                InitRenderData(e, CreateRandomSpawnPosition(playerPos, 2 , 2), 0.4f, mesh, foodMat);
            }
        }

        void InitHealth(Entity e, int curr, int max)
        {
            entityManager.SetComponentData<Health>(e, new Health { curr = curr, max = max });
        }

        void InitHealthModifier(Entity e, float amount)
        {
            entityManager.SetComponentData<HealthModifier>(e, new HealthModifier { value = amount });
        }

        void InitRenderData(Entity e, float3 pos, float scale, Mesh mesh, Material mat)
        {
            entityManager.SetComponentData<Scale>(e, new Scale { Value = scale });
            entityManager.SetComponentData<Translation>(e, new Translation { Value = pos });
            entityManager.SetSharedComponentData<RenderMesh>(e, new RenderMesh { mesh = mesh, material = mat });

            Bounds b = new Bounds();
            b.center = pos;
            float halfScale = scale * 0.5f;
            b.extents = new float3(halfScale, halfScale, halfScale);
            entityManager.SetComponentData<BoundingVolume>(e, new BoundingVolume { volume = b });
        }
    }
}
