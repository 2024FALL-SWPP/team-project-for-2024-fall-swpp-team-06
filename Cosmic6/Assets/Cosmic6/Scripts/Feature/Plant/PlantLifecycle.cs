using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantLifecycle : MonoBehaviour
{
    [Header("Time Intervals (in seconds)")]
    public float TimeInterval = 5f; // 시간 간격

    [Header("Seed and Fruit Spawning")]
    public GameObject SeedPrefab; // Seed 프리팹
    public GameObject FruitPrefab; // Fruit 프리팹
    public int SeedCount = 3; // Seed 개수
    public int FruitCount = 2; // Fruit 개수

    private List<Transform> childInstances = new List<Transform>();

    private void Start()
    {
        // Child 인스턴스 가져오기
        foreach (Transform child in transform)
        {
            childInstances.Add(child);
        }

        // 초기 상태: 숫자_Sprout만 활성화
        ActivateOnlyFirstState();

        // Plant 단계 활성화 순서를 시작
        StartCoroutine(ActivatePlantStages());
    }

    private void ActivateOnlyFirstState()
    {
        foreach (Transform child in childInstances)
        {
            if (child.name.Contains("Sprout"))
                child.gameObject.SetActive(true);
            else
                child.gameObject.SetActive(false);
        }
    }

    private IEnumerator ActivatePlantStages()
    {
        Transform lastActive = null;

        foreach (Transform child in childInstances)
        {
            if (child.name.Contains("Plant") || child.name.Contains("Sprout"))
            {
                // 이전 활성화된 오브젝트 비활성화
                if (lastActive != null)
                {
                    lastActive.gameObject.SetActive(false);
                }

                // 현재 오브젝트 활성화
                child.gameObject.SetActive(true);
                lastActive = child;

                // TimeInterval 만큼 대기
                yield return new WaitForSeconds(TimeInterval);
            }
        }

        // 모든 Plant 단계 완료 후 Seed와 Fruit 스폰
        //yield return new WaitForSeconds(TimeInterval);
        //SpawnSeedsAndFruits();
    }

    private void SpawnSeedsAndFruits()
    {
        Transform lastPlant = null;

        // 마지막 Plant_3을 찾아 위치 참조
        foreach (Transform child in childInstances)
        {
            if (child.name.Contains("Plant_3"))
            {
                lastPlant = child;
                break;
            }
        }

        if (lastPlant != null)
        {
            // Seed 스폰
            for (int i = 0; i < SeedCount; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                Instantiate(SeedPrefab, lastPlant.position + offset, Quaternion.identity);
            }

            // Fruit 스폰
            for (int i = 0; i < FruitCount; i++)
            {
                Vector3 offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));
                Instantiate(FruitPrefab, lastPlant.position + offset, Quaternion.identity);
            }
        }
        else
        {
            Debug.LogWarning("No Plant_3 found to spawn seeds and fruits.");
        }
    }
}
