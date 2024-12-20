using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FieldDecayManager : MonoBehaviour
{
    public const float DecayInterval = 30f;
    // three textures for tilled grid, 2 -> 1 -> 0
    public const int DecayStages = 3;
    private IEnumerator decayRoutine;
    private int index = 0;
    
    private FarmingManager farmingManager;

    private float gridSize;
    
    private readonly object queueLock = new object();
    
    // roll
    public float currentTime { get; private set; } = 0f;     // must be saved
    public float rollTime { get; private set; }

    private SortedDictionary<FieldDecayData, int> decayQueue = new SortedDictionary<FieldDecayData, int>();     // must be saved
    private Dictionary<(int, int), FieldInfo> fieldMap = new Dictionary<(int, int), FieldInfo>();               // must be saved
    
    internal class FieldInfo
    {
        public FieldDecayData decayData;
        public Terrain terrain;
        public float leftoverTime;
        public FieldState state;
        public float[,,] originalAlphamaps;
        public int targetTextureIndex;

        public FieldInfo(FieldDecayData decayData, Terrain terrain, float leftoverTime, FieldState state,
            float[,,] originalAlphamaps, int targetTextureIndex)
        {
            this.decayData = decayData;
            this.terrain = terrain;
            this.leftoverTime = leftoverTime;
            this.state = state;
            this.originalAlphamaps = originalAlphamaps;
            this.targetTextureIndex = targetTextureIndex;
        }
    }
    
    internal class FieldDecayData : IComparable<FieldDecayData>, IComparable
    {
        public float decayTime;
        public int x;
        public int z;
        public int decayStage;

        public FieldDecayData(float decayTime, int x, int z, int decayStage, FieldDecayManager fieldDecayManager)
        {
            this.decayTime = decayTime;
            this.x = x;
            this.z = z;
            this.decayStage = decayStage;
        }

        public int CompareTo(FieldDecayData other)
        {
            if (decayTime != other.decayTime)
                return decayTime.CompareTo(other.decayTime);
            if (x != other.x)
                return x.CompareTo(other.x);
            return z.CompareTo(other.z);
        }
        
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;
            if (obj is FieldDecayData other)
                return CompareTo(other);
            else
                throw new ArgumentException("Object is not a FieldDecayData");
        }
    }

    // Planted -> Tilled
    public FieldState Harvest(int x, int z)
    {
        
        lock (queueLock)
        {
            if (!fieldMap.ContainsKey((x, z)))
                return FieldState.NotTilled;

            var fieldInfo = fieldMap[(x, z)];
            var decayData = fieldInfo.decayData;

            if (decayQueue.ContainsKey(decayData))
                return FieldState.Tilled;
        
            fieldInfo.state = FieldState.Tilled;
        
            var leftoverTime = fieldInfo.leftoverTime;
            var decayStage = Math.Min(Mathf.FloorToInt(leftoverTime / DecayInterval), DecayStages - 1);
            var decayTime = leftoverTime - decayStage * DecayInterval;
        
            decayData.decayStage = decayStage;
        
            decayData.decayTime = decayTime + currentTime;

            decayQueue[decayData] = 0;
            
            StopCoroutine(decayRoutine);
            decayRoutine = DecayRoutine();
            StartCoroutine(decayRoutine);
            return FieldState.Tilled;
        }
    }

    // Tilled -> Planted
    public FieldState Plant(int x, int z)
    {
        lock (queueLock)
        {
            if (!fieldMap.ContainsKey((x, z)))
                return FieldState.NotTilled;
            
            var fieldInfo = fieldMap[(x, z)];
            var decayData = fieldInfo.decayData;

            decayQueue.Remove(decayData);

            fieldInfo.leftoverTime =
                decayData.decayStage * DecayInterval + decayData.decayTime - currentTime;
            fieldInfo.state = FieldState.Planted;
            
            StopCoroutine(decayRoutine);
            decayRoutine = DecayRoutine();
            StartCoroutine(decayRoutine);
            return FieldState.Planted;
        }
    }
    
    // NotTilled -> Tilled
    public FieldState Tile(int x, int z, Terrain terrain)
    {
        lock (queueLock)
        {
            if (fieldMap.ContainsKey((x, z)))
            {
                var fieldInfo = fieldMap[(x, z)];

                if (decayQueue.ContainsKey(fieldInfo.decayData))
                {
                    return FieldState.Tilled;
                }
                
                return FieldState.Planted;
            }
        }
        
        
        TerrainData terrainData = terrain.terrainData;
        
        float terrainSizeX = terrainData.size.x;
        float terrainSizeZ = terrainData.size.z;

        int alphaMapWidth = terrainData.alphamapWidth;
        int alphaMapHeight = terrainData.alphamapHeight;

        var (globalX, globalZ) = farmingManager.IndexToGlobal(x, z);
        
        Vector3 terrainPosition = terrain.transform.position;
        
        int alphaStartX = Mathf.FloorToInt((globalX - terrainPosition.x) / terrainSizeX * alphaMapWidth);
        int alphaStartZ = Mathf.FloorToInt((globalZ - terrainPosition.z) / terrainSizeZ * alphaMapHeight);

        int alphaWidth = 1;//Mathf.CeilToInt(gridSize / terrainSizeX * alphaMapWidth);
        int alphaHeight = 1;//Mathf.CeilToInt(gridSize / terrainSizeZ * alphaMapHeight);
        
        float[,,] originalAlphamaps = terrainData.GetAlphamaps(alphaStartX, alphaStartZ, alphaWidth, alphaHeight);

        var decayData = new FieldDecayData(DecayInterval + currentTime, x, z, DecayStages - 1, this);
        
        lock (queueLock)
        {
            if (decayQueue.ContainsKey(decayData))
            {
                return FieldState.Tilled;
            }
        
            var leftoverTime = DecayStages * DecayInterval;
            
            // TODO: change texture index by Regions
            var fieldInfo = new FieldInfo(decayData, terrain, leftoverTime, FieldState.Tilled, originalAlphamaps, 7);
        
            fieldMap[(x, z)] = fieldInfo;

            decayQueue[decayData] = index;
            index += 1;
            
            StopCoroutine(decayRoutine);
            decayRoutine = DecayRoutine();
            StartCoroutine(decayRoutine);
        }

        StartCoroutine(ChangeTexture(decayData));
        return FieldState.Tilled;
    }
    
    // Tilled -> NotTilled
    // Actually, only instance in fieldMap will be removed
    // Flow: remove from decayQueue -> restore original texture -> alert to FarmingManager -> Untile
    /*
    void UnTile(int x, int z)
    {
        lock (queueLock)
        {
            if (!fieldMap.ContainsKey((x, z)))
            {
                return;
            }
            var fieldInfo = fieldMap[(x, z)];
            decayQueue.Remove(fieldInfo.decayData);
            fieldMap.Remove((x, z));
        }
    }*/

    IEnumerator DecayRoutine()
    {
        while (true)
        {
            bool noData = false;
            lock (queueLock)
            {
                if (decayQueue.Count == 0)
                {
                    currentTime = 0f;
                    noData = true;
                }
            }

            if (noData)
            {
                yield return null;
                continue;
            }
            
            float waitTime = 0.2f;
            FieldDecayData nextDecayData = null;

            lock (queueLock)
            {
                while (true)
                {
                    if (decayQueue.Count == 0)
                    {
                        currentTime = 0f;
                        break;
                    }
                    
                    var minDecayData = decayQueue.First().Key;

                    if (minDecayData.decayTime < currentTime)
                    {
                        yield return RefineTexture(minDecayData);
                    }
                    else
                    {
                        waitTime = Mathf.Max(minDecayData.decayTime - currentTime, 0.1f);
                        nextDecayData = minDecayData;
                        break;
                    }
                }
            }
            
            yield return new WaitForSeconds(waitTime);

            lock (queueLock)
            {
                if (nextDecayData != null && decayQueue.ContainsKey(nextDecayData))
                {
                    yield return RefineTexture(nextDecayData);
                }
            }

            yield return null;
        }
    }

    // only called in locking function
    IEnumerator RefineTexture(FieldDecayData targetDecayData)
    {
        var idx = decayQueue[targetDecayData];
        decayQueue.Remove(targetDecayData);
        targetDecayData.decayStage -= 1;

        // if the targetDecayData should be deleted, ChangeTexture without yield return makes inconsistency.
        if (targetDecayData.decayStage == -1)
        {
            print(idx);
            farmingManager.RemoveFieldState(targetDecayData.x, targetDecayData.z);
            yield return ChangeTexture(targetDecayData);
            fieldMap.Remove((targetDecayData.x, targetDecayData.z));
        }
        else
        {
            print(idx);
            targetDecayData.decayTime = DecayInterval + currentTime;
            decayQueue[targetDecayData] = idx;
            StartCoroutine(ChangeTexture(targetDecayData));
        }
    }

    IEnumerator ChangeTexture(FieldDecayData decayData)
    {
        print("stage: " + decayData.decayStage);
        var x = decayData.x;
        var z = decayData.z;
        var fieldInfo = fieldMap[(x, z)];
        TerrainData terrainData = fieldInfo.terrain.terrainData;
        
        float terrainSizeX = terrainData.size.x;
        float terrainSizeZ = terrainData.size.z;

        int alphaMapWidth = terrainData.alphamapWidth;
        int alphaMapHeight = terrainData.alphamapHeight;
        
        // TODO: grid index -> alphamap index transform need to be refined (size issue)
        
        var (globalX, globalZ) = farmingManager.IndexToGlobal(x, z);
        
        Vector3 terrainPosition = fieldInfo.terrain.transform.position;
        
        int alphaStartX = Mathf.FloorToInt((globalX - terrainPosition.x) / terrainSizeX * alphaMapWidth);
        int alphaStartZ = Mathf.FloorToInt((globalZ - terrainPosition.z) / terrainSizeZ * alphaMapHeight);

        int alphaWidth = 1;//Mathf.CeilToInt(gridSize / terrainSizeX * alphaMapWidth);
        int alphaHeight = 1;//Mathf.CeilToInt(gridSize / terrainSizeZ * alphaMapHeight);
        
        if (decayData.decayStage == -1)
        {
            terrainData.SetAlphamaps(alphaStartX, alphaStartZ, fieldInfo.originalAlphamaps);
            yield break;
        }
        
        float[,,] originalAlphamaps = fieldInfo.originalAlphamaps;
        float[,,] newAlphamaps = new float[alphaHeight, alphaWidth, terrainData.alphamapLayers];

        var ratio = 0.5f + (decayData.decayStage + 1f) / (2 * DecayStages);

        for (int i = 0; i < alphaHeight; i++)
        {
            for (int j = 0; j < alphaWidth; j++)
            {
                for (int k = 0; k < terrainData.alphamapLayers; k++)
                {
                    newAlphamaps[i, j, k] = (k == fieldInfo.targetTextureIndex) ? ratio : (1 - ratio) * originalAlphamaps[i, j, k];
                    print("newAlphamaps" + i + ", " + j + ", " + k + ": " + newAlphamaps[i, j, k]);
                }
            }
        }
        
        
        terrainData.SetAlphamaps(alphaStartX, alphaStartZ, newAlphamaps);
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        farmingManager = FarmingManager.Instance;
        gridSize = farmingManager.gridSize;
        decayRoutine = DecayRoutine();
        StartCoroutine(decayRoutine);
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
    }
    
    void OnApplicationQuit()
    {
        foreach (FieldDecayData decayData in decayQueue.Keys)
        {
            var x = decayData.x;
            var z = decayData.z;
            var fieldInfo = fieldMap[(x, z)];
            TerrainData terrainData = fieldInfo.terrain.terrainData;
        
            float terrainSizeX = terrainData.size.x;
            float terrainSizeZ = terrainData.size.z;

            int alphaMapWidth = terrainData.alphamapWidth;
            int alphaMapHeight = terrainData.alphamapHeight;
        
            // TODO: grid index -> alphamap index transform need to be refined (size issue)
            
            var (globalX, globalZ) = farmingManager.IndexToGlobal(x, z);
        
            Vector3 terrainPosition = fieldInfo.terrain.transform.position;
        
            int alphaStartX = Mathf.FloorToInt((globalX - terrainPosition.x) / terrainSizeX * alphaMapWidth);
            int alphaStartZ = Mathf.FloorToInt((globalZ - terrainPosition.z) / terrainSizeZ * alphaMapHeight);
        
            
            terrainData.SetAlphamaps(alphaStartX, alphaStartZ, fieldInfo.originalAlphamaps);
        }
    }
}
