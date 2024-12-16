using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DevionGames.InventorySystem;

public class HandManager : MonoBehaviour
{
    public GameObject itemHandObject;
    private ItemCollection itemCollection;
    private ItemContainer itemContainer;

    public List<Item> holdItems;

    public int itemToUse = 0;

    // Start is called before the first frame update
    void Start()
    {
        itemCollection = itemHandObject.GetComponent<ItemCollection>();
        itemContainer = itemHandObject.GetComponent<ItemContainer>();
    }

    // Update is called once per frame
    void Update()
    {
        holdItems = itemCollection.m_Items;
        //Debug.Log(holdItems);

        if (itemToUse > 0)
        {
            Item item = itemCollection[0];
            int itemCount = itemCollection.ItemCount(item);

            if (itemCount > itemToUse)
            {
                itemCollection.Remove(item, itemToUse);
                itemContainer.RemoveItem(item, itemToUse);
            }
            else
            {
                //itemCollection.Remove(item);
                itemContainer.RemoveItem(item);
            }

            itemToUse = 0;
        }
    }

    public void UseItem(int amount)
    {
        holdItems = itemCollection.m_Items;
        Debug.Log(holdItems);

        if (amount > 0)
        {
            Item item = itemCollection[0];
            int itemCount = itemCollection.ItemCount(item);

            if (itemCount > amount)
            {
                itemCollection.Remove(item, amount);
                itemContainer.RemoveItem(item, amount);
            }
            else
            {
                //itemCollection.Remove(item);
                itemContainer.RemoveItem(item);
            }
        }
    }

}