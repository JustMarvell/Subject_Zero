using System.Collections.Generic;
using UnityEngine;

namespace SubjectZero.Character.Player
{
    /// <summary>
    /// Minimal key-item tracking (keycards, etc) - deliberately not a full inventory
    /// system, which is still deferred per the earlier design decision. Just enough
    /// to gate locked doors.
    /// </summary>
    public class PlayerKeyItems : MonoBehaviour
    {
        public static PlayerKeyItems Instance { get; private set; }

        private readonly HashSet<string> _heldItems = new();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void AddItem(string itemId) => _heldItems.Add(itemId);
        public bool HasItem(string itemId) => _heldItems.Contains(itemId);
    }
}