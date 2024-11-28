using UnityEditor;
using UnityEngine;

public class MeshFromTerrain : MonoBehaviour
{
    public int resolution = 512; // Mesh의 해상도 (높을수록 정교해짐)
    
    [MenuItem("Tools/Terrain To Mesh")]
    public static void GenerateMeshFromTerrain()
    {
        Terrain terrain = Selection.activeGameObject?.GetComponent<Terrain>();

        if (terrain == null)
        {
            Debug.LogError("선택된 GameObject가 Terrain이 아닙니다.");
            return;
        }

        TerrainData terrainData = terrain.terrainData;

        // 해상도 설정 (Terrain의 heightmapResolution과 동일하게 설정)
        int resolution = terrainData.heightmapResolution / 10;
        
        int numVertices = 10;

        // Mesh 데이터 생성
        Vector3[] vertices = new Vector3[numVertices * numVertices];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

        Vector3 terrainSize = terrainData.size;
        
        int heightmapWidth = terrainData.heightmapResolution;
        int heightmapHeight = terrainData.heightmapResolution;

        float xOffsetGlobal = 0;
        float zOffsetGlobal = 270;

        float xOffsetIndex = xOffsetGlobal * (heightmapWidth - 1) / terrainSize.x;
        float zOffsetIndex = zOffsetGlobal * (heightmapHeight - 1) / terrainSize.z;

        int xStartIndex = 9;
        int zStartIndex = 9;
        

        // 정점 생성
        for (int z = 0; z < numVertices; z++)
        {
            for (int x = 0; x < numVertices; x++)
            {
                // Heightmap 인덱스 계산
                float heightX = (x+xStartIndex) * (heightmapWidth - 1) / (float)(resolution - 1) + xOffsetIndex;
                float heightZ = (z+zStartIndex) * (heightmapHeight - 1) / (float)(resolution - 1) + zOffsetIndex;

                // 높이 값 샘플링 (x, z 순서로 전달)
                float heightValue = terrainData.GetHeight(Mathf.FloorToInt(heightX), Mathf.FloorToInt(heightZ));

                // 좌표 매핑 (x, y, z)
                vertices[z * numVertices + x] = new Vector3(
                    (x+xStartIndex) * terrainSize.x / (resolution - 1) + xOffsetGlobal,
                    heightValue,
                    (z+zStartIndex) * terrainSize.z / (resolution - 1) + zOffsetGlobal
                );
            }
        }

        // 삼각형 인덱스 생성
        int t = 0;
        for (int z = 0; z < numVertices - 1; z++)
        {
            for (int x = 0; x < numVertices - 1; x++)
            {
                int i = z * numVertices + x;

                // 첫 번째 삼각형
                triangles[t++] = i;
                triangles[t++] = i + numVertices;
                triangles[t++] = i + 1;

                // 두 번째 삼각형
                triangles[t++] = i + 1;
                triangles[t++] = i + numVertices;
                triangles[t++] = i + numVertices + 1;
            }
        }

        // Mesh 생성 및 설정
        Mesh mesh = new Mesh();

        // 32비트 인덱스 포맷 설정
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // .asset 파일로 저장
        string path = EditorUtility.SaveFilePanelInProject("Save Terrain Mesh", "TerrainMesh", "asset", "Choose location to save the mesh.");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            Debug.Log("Mesh saved to: " + path);
        }
        else
        {
            Debug.LogWarning("Mesh 저장이 취소되었습니다.");
        }
    }
    
    [MenuItem("Tools/Generate Circular Mesh From Terrain")]
    public static void GenerateCircularMeshFromTerrain()
    {

        var objects = Selection.gameObjects;
        Terrain[] terrains = new Terrain[objects.Length];
        /*
        for (int i = 0; i < objects.Length; i++)
        {
            terrains[i] = objects[i].GetComponent<Terrain>();
            print(i + ": " + objects[i].name);
        }*/
        terrains[1] = objects[0].gameObject.GetComponent<Terrain>();
        terrains[0] = objects[1].gameObject.GetComponent<Terrain>();

        Vector3 terrainPosition = terrains[0].transform.position;

        // 스크립트 내에서 중심 좌표, 반지름, 해상도 설정
        //Vector2 centerXZ = new Vector2(100f, 620f); // 중심 좌표 (X, Z)
        //float radius = 150f; // 반지름
        Vector2 centerXZ = new Vector2(65f, 570f); // 중심 좌표 (X, Z)
        float radius = 120f; // 반지름
        int resolution = 40; // 해상도 (삼각형 수)
        int radiusRes = 40;

        // 중심 좌표를 월드 좌표계로 변환
        Vector3 centerPosition = new Vector3(centerXZ.x, 0f, centerXZ.y);

        // 중심 정점 생성
        float centerHeight = terrains[0].SampleHeight(centerPosition + terrainPosition);
        Vector3 centerVertex = new Vector3(centerXZ.x, centerHeight, centerXZ.y);

        // 주변 정점 생성
        Vector3[] vertices = new Vector3[resolution * radiusRes + 1]; // 중심 정점 + 주변 정점들
        int[] triangles = new int[resolution * (2 * radiusRes - 1) * 3]; // 각 삼각형은 3개의 인덱스

        vertices[0] = centerVertex; // 중심 정점

        // 각도 간격 계산
        float angleStep = 360f / resolution;

        int t = 0;

        for (int i = 0; i < resolution; i++)
        {
            float angle = Mathf.Deg2Rad * (angleStep * i);

            float x = centerXZ.x + radius / radiusRes * Mathf.Cos(angle);
            float z = centerXZ.y + radius / radiusRes * Mathf.Sin(angle);

            var terrain = terrains[0];

            if (x < 0)
            {
                terrain = terrains[1];
            }
            
            
            Vector3 samplePosition = new Vector3(x, 0f, z);
            float height = terrain.SampleHeight(samplePosition + terrainPosition);
            vertices[i * radiusRes + 1] = new Vector3(x, height, z);
            
            triangles[t++] = 0;
            triangles[t++] = (i + 1) % resolution * radiusRes + 1;
            triangles[t++] = i * radiusRes + 1;
            
            for (int j = 1; j < radiusRes; j++)
            {
                x = centerXZ.x + radius * (j + 1) / radiusRes * Mathf.Cos(angle);
                z = centerXZ.y + radius * (j + 1) / radiusRes * Mathf.Sin(angle);
                
                terrain = terrains[0];

                if (x < 0)
                {
                    terrain = terrains[1];
                }
                
                samplePosition = new Vector3(x, 0f, z);
                height = terrain.SampleHeight(samplePosition + terrainPosition);
                vertices[i * radiusRes + j + 1] = new Vector3(x, height, z);
                
                // 삼각형 인덱스 설정 (정점 순서 수정)
                triangles[t++] = (i + 1) % resolution * radiusRes + j; // 중심 정점 인덱스
                triangles[t++] = (i + 1) % resolution * radiusRes + j + 1;
                triangles[t++] = i * radiusRes + j;
                
                triangles[t++] = i * radiusRes + j;
                triangles[t++] = (i + 1) % resolution * radiusRes + j + 1;
                triangles[t++] = i * radiusRes + j + 1; // 중심 정점 인덱스
                
            }
        }

        // Mesh 생성 및 설정
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        // .asset 파일로 저장
        string path = EditorUtility.SaveFilePanelInProject("Save Terrain Mesh", "CircularTerrainMesh", "asset", "Choose location to save the mesh.");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
            Debug.Log("Mesh saved to: " + path);
        }
        else
        {
            Debug.LogWarning("Mesh 저장이 취소되었습니다.");
        }
    }
    

}
