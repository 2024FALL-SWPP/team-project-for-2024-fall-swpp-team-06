using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DevionGames.InventorySystem
{
    public class PlantLifecycle : MonoBehaviour
    {
        [Header("Time Intervals (in seconds)")]
        private float TimeInterval = 60f; // 시간 간격

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

            // plantManager에 정보 보내기
            PlantManager.Instance.AddPlant(
                gameObject.name,
                gameObject.transform.position,
                this
            );

            // Plant 단계 활성화 순서를 시작
            StartCoroutine(ActivatePlantStages());
        }

        private void ActivateOnlyFirstState()
        {
            foreach (Transform child in childInstances)
            {
                if (child.name.Contains("Sprout"))
                {
                    child.gameObject.SetActive(true);
                }

                else
                    child.gameObject.SetActive(false);
            }
        }

        public string plantStatus = "Sprout";

        private IEnumerator ActivatePlantStages()
        {
            Transform lastActive = null;

            foreach (Transform child in childInstances)
            {
                string validStatus = GetValidPlantStatus(child.name);

                if (!string.IsNullOrEmpty(validStatus))
                {
                    if (lastActive != null)
                    {
                        lastActive.gameObject.SetActive(false);
                    }

                    child.gameObject.SetActive(true);
                    lastActive = child;

                    plantStatus = validStatus;

                    yield return new WaitForSeconds(TimeInterval);
                }
            }
        }

        private string GetValidPlantStatus(string name)
        {
            string[] possibleStatuses = { "Sprout", "Plant_1", "Plant_2", "Plant_3" };
            foreach (string status in possibleStatuses)
            {
                if (name.EndsWith(status))
                {
                    return status;
                }
            }
            return null;
        }
    }

}