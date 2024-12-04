using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevionGames.UIWidgets;
using UnityEditor;
using UnityEngine.Events;

namespace DevionGames.InventorySystem
{
    [UnityEngine.Scripting.APIUpdating.MovedFromAttribute(true, null, "Assembly-CSharp")]
    [Icon("Item")]
    [ComponentMenu("Inventory System/Pickup Item")]
    [RequireComponent(typeof(ItemCollection))]
    public class Pickup : Action
    {
        [SerializeField]
        private string m_WindowName = "Inventory";
        [SerializeField]
        private bool m_DestroyWhenEmpty = true;
        [SerializeField]
        private int m_Amount = -1;

        private ItemCollection m_ItemCollection;

        private UnityEvent onTriggered; // 다른 Pickup 트리거용 이벤트

        private PlantLifecycle plantLifeCycle;

        public override void OnStart()
        {
            this.plantLifeCycle = gameObject.GetComponent<PlantLifecycle>();
            Debug.Log("Plant Life Cycle: " + this.plantLifeCycle);

            this.m_ItemCollection = gameObject.GetComponent<ItemCollection>();
            this.m_ItemCollection.onChange.AddListener(delegate () {
                if (this.m_ItemCollection.IsEmpty && this.m_DestroyWhenEmpty)
                {
                    GameObject.Destroy(gameObject,0.1f);
                }
            });

        }
        
        public override ActionStatus OnUpdate()
        {
            ActionStatus status = PickupItems();

            onTriggered?.Invoke();
            return status;
        }

        public void TriggerPickup()
        {
            OnUpdate();
        }

        public ActionStatus  PickupItems()
        {
            if (this.m_ItemCollection.Count == 0) {
                InventoryManager.Notifications.empty.Show(gameObject.name.Replace("(Clone)", "").ToLower());
                return ActionStatus.Failure;
            }
            ItemContainer[] windows = WidgetUtility.FindAll<ItemContainer>(this.m_WindowName);
            List<Item> items = new List<Item>();
            if (this.m_Amount < 0)
            {
                // Pickup
                items.AddRange(this.m_ItemCollection);
            }
            else
            {
                for (int i = 0; i < this.m_Amount; i++)
                {
                    Item item = this.m_ItemCollection[Random.Range(0, this.m_ItemCollection.Count)];
                    items.Add(item);
                }
            }

            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];
                if (windows.Length > 0)
                {
                    for (int j = 0; j < windows.Length; j++)
                    {
                        ItemContainer current = windows[j];
                        string itemName = item.Name;

                        if (current.StackOrAdd(item))
                        {
                            // Pickup
                            this.m_ItemCollection.Remove(item);

                            List<Item> extraItems = GetItemsFromJSON(itemName);
                            foreach (Item extraItem in extraItems){
                                if (current.StackOrAdd(extraItem))
                                {
                                    this.m_ItemCollection.Remove(extraItem);
                                }
                            }
                            break;
                        }
                    }
                }
                else
                {
                    //Drop items to ground
                    Debug.Log("Drop items to the ground (windows.Length < 0 Case");

                    DropItem(item);
                    this.m_ItemCollection.Remove(item);
                }
            }

            return ActionStatus.Success;
        }

        private List<Item> GetItemsFromJSON(string itemName)
        {
            List<Item> items = new List<Item>();

            // JSON 데이터를 로드
            PlantItemData plantItemData = PlantItemLoader.Instance.GetPlantItemData(itemName);
            if (plantItemData == null)
            {
                Debug.Log($"No data found for item: {itemName}");
                return items;
            }

            // Seed 처리
            if (!string.IsNullOrEmpty(plantItemData.seed.prefab_path))
            {
                Debug.Log("Seed Path: " + plantItemData.seed.prefab_path);
                items.AddRange(
                    GetPrefabsAsItems(
                        plantItemData.seed.prefab_path,
                        plantItemData.seed.cnt)
                );
            }

            // Fruit 처리
            if (!string.IsNullOrEmpty(plantItemData.fruit.prefab_path))
            {
                Debug.Log("Fruit Path: " + plantItemData.fruit.prefab_path);
                items.AddRange(
                    GetPrefabsAsItems(
                        plantItemData.fruit.prefab_path,
                        plantItemData.fruit.cnt
                    )
                );
            }

            return items;
        }

        private List<Item> GetPrefabsAsItems(string prefabPath, int count)
        {
            List<Item> items = new List<Item>();

            // Prefab 로드
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath); // UnityEditor.AssetDatabase 사용
            if (prefab == null)
            {
                Debug.LogError($"Prefab not found at path: {prefabPath}");
                return items;
            }

            // 지정된 개수만큼 생성 및 컬렉션에 추가
            for (int i = 0; i < count; i++)
            {
                GameObject instance = ObjectManager.Instance.SpawnObject(prefab, Vector3.zero, Quaternion.identity);
                Debug.Log($"Instantiating prefab: {prefab.name} Num: {count}");

                // Items 추가 실패
            }

            return items;
        }


        private void DropItem(Item item)
        {
            Debug.Log("Start Drop Item");

            GameObject prefab = item.OverridePrefab != null ? item.OverridePrefab : item.Prefab;
            float angle = Random.Range(0f, 360f);
            float x = (float)(InventoryManager.DefaultSettings.maxDropDistance * Mathf.Cos(angle * Mathf.PI / 180f)) + gameObject.transform.position.x;
            float z = (float)(InventoryManager.DefaultSettings.maxDropDistance * Mathf.Sin(angle * Mathf.PI / 180f)) + gameObject.transform.position.z;
            Vector3 position = new Vector3(x, gameObject.transform.position.y, z);

            RaycastHit hit;
            if (Physics.Raycast(position, Vector3.down, out hit)) {
                position = hit.point+ Vector3.up;
            }

            GameObject go = InventoryManager.Instantiate(prefab, position, Random.rotation);
            ItemCollection collection = go.GetComponent<ItemCollection>();
            if (collection != null)
            {
                collection.Clear();
                collection.Add(item);
            }
        }
    }
}