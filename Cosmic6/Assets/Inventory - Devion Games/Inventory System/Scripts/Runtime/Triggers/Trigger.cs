using DevionGames.UIWidgets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DevionGames.InventorySystem
{
    [UnityEngine.Scripting.APIUpdating.MovedFromAttribute(true, null, "Assembly-CSharp")]
    [DisallowMultipleComponent]
    public class Trigger : BehaviorTrigger
    {
        public enum FailureCause {
            Unknown,
            FurtherAction, // Requires a user action(Select an item for crafting)
            NotEnoughCurrency, // Player does not have enough money 
            Remove, // No longer exists (Item was removed)
            ContainerFull, // Given container is full
            InUse, // Something is already in use (Player is already crafting something-> no start possible)
            Requirement // Missing requirements for this action( Missing ingredient to craft)
        }

        public override PlayerInfo PlayerInfo => InventoryManager.current.PlayerInfo;
        public static ItemContainer currentUsedWindow;
        protected delegate void ItemEventFunction<T>(T handler, Item item, GameObject player);
        protected delegate void FailureItemEventFunction<T>(T handler, Item item, GameObject player, FailureCause failureCause);

        //Deprecate use SendMessage with Use
        //used for UI Button reference
        public void StartUse() {
            Debug.Log("StartUse called on " + gameObject.name);
            Use();
        }

        public void StartUse(ItemContainer window) {
            if (window.IsVisible) {
                Trigger.currentUsedWindow = window;
                Debug.Log("StartUse with window called on " + gameObject.name);
                Use();
            }
        }

        public void StopUse() {
            Debug.Log("StopUse called on " + gameObject.name);
            InUse = false;
        }

        public virtual bool OverrideUse(Slot slot, Item item) {
            return false;
        }

        protected override void DisplayInUse() {
            Debug.Log("DisplayInUse called on " + gameObject.name);
            InventoryManager.Notifications.inUse.Show();
        }

        protected override void DisplayOutOfRange() {
            Debug.Log("DisplayOutOfRange called on " + gameObject.name);
            InventoryManager.Notifications.toFarAway.Show();
        }

        protected void ExecuteEvent<T>(ItemEventFunction<T> func, Item item, bool includeDisabled = false) where T : ITriggerEventHandler {
            Debug.Log("ExecuteEvent called on " + gameObject.name + " with item: " + (item != null ? item.Name : "null"));
            for (int i = 0; i < this.m_TriggerEvents.Length; i++) {
                ITriggerEventHandler handler = this.m_TriggerEvents[i];
                if (ShouldSendEvent<T>(handler, includeDisabled)) {
                    func.Invoke((T)handler, item, PlayerInfo.gameObject);
                }
            }

            string eventID = string.Empty;
            if (this.m_CallbackHandlers.TryGetValue(typeof(T), out eventID))
            {
                CallbackEventData triggerEventData = new CallbackEventData();
                triggerEventData.AddData("Trigger",this);
                triggerEventData.AddData("Player",PlayerInfo.gameObject);
                triggerEventData.AddData("EventData", new PointerEventData(EventSystem.current));
                triggerEventData.AddData("Item", item);
                base.Execute(eventID, triggerEventData);
            }
        }

        protected void ExecuteEvent<T>(FailureItemEventFunction<T> func, Item item, FailureCause failureCause, bool includeDisabled = false) where T : ITriggerEventHandler {
            Debug.Log("ExecuteEvent with FailureCause called on " + gameObject.name + " with item: " + (item != null ? item.Name : "null") + " due to " + failureCause);
            for (int i = 0; i < this.m_TriggerEvents.Length; i++) {
                ITriggerEventHandler handler = this.m_TriggerEvents[i];
                if (ShouldSendEvent<T>(handler, includeDisabled)) {
                    func.Invoke((T)handler, item, InventoryManager.current.PlayerInfo.gameObject, failureCause);
                }
            }

            string eventID = string.Empty;
            if (this.m_CallbackHandlers.TryGetValue(typeof(T), out eventID))
            {
                CallbackEventData triggerEventData = new CallbackEventData();
                triggerEventData.AddData("Trigger", this);
                triggerEventData.AddData("Player", PlayerInfo.gameObject);
                triggerEventData.AddData("EventData", new PointerEventData(EventSystem.current));
                triggerEventData.AddData("Item", item);
                triggerEventData.AddData("FailureCause", failureCause);
                base.Execute(eventID, triggerEventData);
            }
        }
    }
}
