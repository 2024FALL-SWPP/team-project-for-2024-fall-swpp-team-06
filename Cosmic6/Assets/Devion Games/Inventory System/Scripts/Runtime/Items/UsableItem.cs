using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DevionGames.InventorySystem.ItemActions;
using System.Linq;

namespace DevionGames.InventorySystem
{
    [System.Serializable]
    public class UsableItem : Item
    {
        [SerializeField]
        private bool m_UseCategoryCooldown = true;
        [SerializeField]
        private float m_Cooldown = 1f;
        public float Cooldown
        {
            get
            {
                return this.m_UseCategoryCooldown ? Category.Cooldown : this.m_Cooldown;
            }
        }

        [SerializeReference]
        public List<Action> actions = new List<Action>();

        private Sequence m_ActionSequence;
        private IEnumerator m_ActionBehavior;

        protected override void OnEnable()
        {
            base.OnEnable();

            Debug.Log($"UsableItem OnEnable: Initializing actions for item '{this.Name}'.");

            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i] is ItemAction)
                {
                    ItemAction action = actions[i] as ItemAction;
                    action.item = this;
                    Debug.Log($"Action '{action.GetType().Name}' assigned to item '{this.Name}'.");
                }
            }
        }

        public override void Use()
        {
            Debug.Log($"UsableItem Use: Starting use of item '{this.Name}'.");

            if (this.m_ActionSequence == null)
            {
                Debug.Log("Creating a new ActionSequence...");
                this.m_ActionSequence = new Sequence(
                    InventoryManager.current.PlayerInfo.gameObject,
                    InventoryManager.current.PlayerInfo,
                    InventoryManager.current.PlayerInfo.gameObject.GetComponent<Blackboard>(),
                    actions.Cast<IAction>().ToArray()
                );
            }

            if (this.m_ActionBehavior != null)
            {
                Debug.Log("Stopping previous ActionBehavior coroutine...");
                UnityTools.StopCoroutine(m_ActionBehavior);
            }
            Debug.Log("Starting ActionBehavior coroutine...");
            this.m_ActionBehavior = SequenceCoroutine();
            UnityTools.StartCoroutine(this.m_ActionBehavior);
        }

        protected IEnumerator SequenceCoroutine()
        {
            Debug.Log("SequenceCoroutine started.");
            this.m_ActionSequence.Start();

            while (this.m_ActionSequence.Tick())
            {
                yield return null;
            }

            Debug.Log("SequenceCoroutine finished executing all actions.");
        }
    }
}
