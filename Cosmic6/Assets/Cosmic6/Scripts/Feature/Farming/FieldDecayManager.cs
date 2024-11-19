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
    
    private FarmingManager farmingManager;

    private float gridSize = 1f;
    
    private readonly object queueLock = new object();
    
    // roll
    public float currentTime { get; private set; } = 0f;     // must be saved
    public float rollTime { get; private set; }

    private SortedDictionary<FieldDecayData, int> decayQueue = new SortedDictionary<FieldDecayData, int>();     // must be saved
    private Dictionary<(int, int), FieldInfo> fieldMap = new Dictionary<(int, int), FieldInfo>();               // must be saved
    
    private class FieldInfo
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
    
    private class FieldDecayData : IComparable<FieldDecayData>
    {
        public float decayTime;
        public int x;
        public int z;
        public int decayStage;
        private readonly FieldDecayManager fieldDecayManager;
        private readonly float rollTime;

        public FieldDecayData(float decayTime, int x, int z, int decayStage, FieldDecayManager fieldDecayManager)
        {
            this.decayTime = decayTime;
            this.x = x;
            this.z = z;
            this.decayStage = decayStage;
            this.fieldDecayManager = fieldDecayManager;
            this.rollTime = fieldDecayManager.rollTime;
        }

        public int CompareTo(FieldDecayData other)
        {
            if (decayTime != other.decayTime)
            {
                var currentTime = fieldDecayManager.currentTime;

                return ((decayTime + this.rollTime - currentTime) % this.rollTime).CompareTo(
                    (other.decayTime + this.rollTime - currentTime) % this.rollTime);
            }
                
            if (x != other.x)
                return x.CompareTo(other.x);
            return z.CompareTo(other.z);
        }
    }

    // Planted -> Tilled
    public bool Harvest(int x, int z)
    {
        
        lock (queueLock)
        {
            if (!fieldMap.ContainsKey((x, z)))
                return false;

            var fieldInfo = fieldMap[(x, z)];
            var decayData = fieldInfo.decayData;
            
            if (decayQueue.ContainsKey(decayData))
                return false;
        
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
            return true;
        }
    }

    // Tilled -> Planted
    public bool Plant(int x, int z)
    {
        lock (queueLock)
        {
            if (!fieldMap.ContainsKey((x, z)))
                return false;
            
            var fieldInfo = fieldMap[(x, z)];
            var decayData = fieldInfo.decayData;

            decayQueue.Remove(decayData);
        
            fieldInfo.leftoverTime = decayData.decayStage * DecayInterval + decayData.decayTime;
            fieldInfo.state = FieldState.Planted;
            
            StopCoroutine(decayRoutine);
            decayRoutine = DecayRoutine();
            StartCoroutine(decayRoutine);
            return true;
        }
    }
    
    // NotTilled -> Tilled
    public bool Tile(int x, int z, Terrain terrain)
    {
        TerrainData terrainData = terrain.terrainData;
        
        float terrainSizeX = terrainData.size.x;
        float terrainSizeZ = terrainData.size.z;

        int alphaMapWidth = terrainData.alphamapWidth;
        int alphaMapHeight = terrainData.alphamapHeight;
        
        int alphaStartX = Mathf.FloorToInt(x * gridSize / terrainSizeX * alphaMapWidth);
        int alphaStartZ = Mathf.FloorToInt(z * gridSize / terrainSizeZ * alphaMapHeight);

        int alphaWidth = Mathf.CeilToInt(gridSize / terrainSizeX * alphaMapWidth);
        int alphaHeight = Mathf.CeilToInt(gridSize / terrainSizeZ * alphaMapHeight);
        
        float[,,] originalAlphamaps = terrainData.GetAlphamaps(alphaStartX, alphaStartZ, alphaWidth, alphaHeight);
        
        var decayData = new FieldDecayData(DecayInterval, x, z, DecayStages - 1, this);
        
        lock (queueLock)
        {
            if (decayQueue.ContainsKey(decayData))
            {
                return false;
            }
        
            var leftoverTime = DecayStages * DecayInterval;
            
            // TODO: change texture index by Regions
            var fieldInfo = new FieldInfo(decayData, terrain, leftoverTime, FieldState.Tilled, originalAlphamaps, 3);
        
            fieldMap[(x, z)] = fieldInfo;

            decayQueue[decayData] = 0;
            
            StopCoroutine(decayRoutine);
            decayRoutine = DecayRoutine();
            StartCoroutine(decayRoutine);
        }

        StartCoroutine(ChangeTexture(decayData));
        return true;
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
            lock (queueLock)
            {
                if (decayQueue.Count == 0)
                {
                    yield return null;
                    continue;
                }
            }

            lock (queueLock)
            {
                while (true)
                {
                    var maxDecayData = decayQueue.Max().Key;
                    var decayTime = maxDecayData.decayTime;

                    if ((decayTime + rollTime - currentTime) % rollTime > DecayInterval)
                    {
                        yield return RefineTexture(maxDecayData);
                    }
                    else
                    {
                        break;
                    }
                }
            }

            float waitTime;
            FieldDecayData nextDecayData;

            lock (queueLock)
            {
                nextDecayData = decayQueue.Min().Key;
                var nextDecayTime = nextDecayData.decayTime;
                waitTime = (nextDecayTime + rollTime - currentTime) % rollTime;
            }
            
            yield return new WaitForSeconds(waitTime);

            lock (queueLock)
            {
                yield return RefineTexture(nextDecayData);
            }
        }
    }

    // only called in locking function
    IEnumerator RefineTexture(FieldDecayData targetDecayData)
    {
        decayQueue.Remove(targetDecayData);
        targetDecayData.decayStage -= 1;

        // if the targetDecayData should be deleted, ChangeTexture without yield return makes inconsistency.
        if (targetDecayData.decayStage == -1)
        {
            farmingManager.RemoveFieldState(targetDecayData.x, targetDecayData.z);
            yield return ChangeTexture(targetDecayData);
            fieldMap.Remove((targetDecayData.x, targetDecayData.z));
        }
        else
        {
            StartCoroutine(ChangeTexture(targetDecayData));
            targetDecayData.decayTime = DecayInterval;
        }
    }

    IEnumerator ChangeTexture(FieldDecayData decayData)
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
        int alphaStartX = Mathf.FloorToInt(x * gridSize / terrainSizeX * alphaMapWidth);
        int alphaStartZ = Mathf.FloorToInt(z * gridSize / terrainSizeZ * alphaMapHeight);
        
        int alphaWidth = Mathf.CeilToInt(gridSize / terrainSizeX * alphaMapWidth);
        int alphaHeight = Mathf.CeilToInt(gridSize / terrainSizeZ * alphaMapHeight);
        
        if (decayData.decayStage == -1)
        {
            terrainData.SetAlphamaps(alphaStartX, alphaStartZ, fieldInfo.originalAlphamaps);
            yield break;
        }
        
        float[,,] originalAlphamaps = fieldInfo.originalAlphamaps;
        float[,,] newAlphamaps = new float[alphaHeight, alphaWidth, terrainData.alphamapLayers];

        var ratio = (decayData.decayStage + 1f) / DecayStages;

        for (int i = 0; i < alphaHeight; i++)
        {
            for (int j = 0; j < alphaWidth; j++)
            {
                for (int k = 0; k < terrainData.alphamapLayers; k++)
                {
                    newAlphamaps[i, j, k] = (k == fieldInfo.targetTextureIndex) ? ratio : (1 - ratio) * originalAlphamaps[i, j, k];
                }
            }
        }
        
        yield return null;
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        rollTime = 4 * DecayInterval;
        decayRoutine = DecayRoutine();
        StartCoroutine(decayRoutine);
        farmingManager = GetComponent<FarmingManager>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += Time.deltaTime;
        if (currentTime >= rollTime)
        {
            currentTime -= rollTime;
        }
    }
}
