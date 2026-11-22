using System;
using UnityEngine;

namespace ForestAdventure
{
    // Add to a UI manager GameObject. Subscribe to Changed and read the properties.
    // No direct Canvas/TextMeshPro dependency; classmates can bind any UI.
    public class GameplayUIBridge : MonoBehaviour
    {
        public event Action Changed;
        public event Action<GamePhase> PhaseChanged;
        public GamePhase Phase => GameManager.Instance != null ? GameManager.Instance.Phase : GamePhase.Explore;
        public int Kills => GameManager.Instance != null ? GameManager.Instance.Kills : 0;
        public int RequiredKills => GameManager.Instance != null ? GameManager.Instance.requiredKills : 30;
        public int Gems => GameManager.Instance != null ? GameManager.Instance.Gems : 0;
        public int TotalGemsCollected => GameManager.Instance != null ? GameManager.Instance.GemsCollectedTotal : 0;
        public int GemsToUnlock => GameManager.Instance != null ? GameManager.Instance.requiredCollectedGems : 30;
        public bool SpecialGunUnlocked => GameManager.Instance != null && GameManager.Instance.SpecialGunUnlocked;
        public bool BossPortalActive => GameManager.Instance != null && GameManager.Instance.BossPortalActive;
        public int SecondsRemaining => GameManager.Instance != null ? Mathf.CeilToInt(GameManager.Instance.TimeLeft) : 0;
        public int AmmoInMagazine { get { var p = FindFirstObjectByType<PlayerCombat>(); return p != null ? p.magazine : 0; } }
        public int AmmoInReserve { get { var p = FindFirstObjectByType<PlayerCombat>(); return p != null ? p.reserveAmmo : 0; } }
        private GameManager subscribed;
        private float nextRefresh;
        private void OnEnable() { TrySubscribe(); }
        private void Start() { TrySubscribe(); }
        private void Update()
        {
            TrySubscribe();
            if (Time.unscaledTime >= nextRefresh) { nextRefresh = Time.unscaledTime + 0.1f; Changed?.Invoke(); }
        }
        private void TrySubscribe()
        {
            var current = GameManager.Instance;
            if (current == subscribed) return;
            Unsubscribe();
            subscribed = current;
            if (subscribed != null)
            {
                subscribed.Changed += RelayChanged;
                subscribed.PhaseChanged += RelayPhase;
                Changed?.Invoke();
            }
        }
        private void RelayChanged() { Changed?.Invoke(); }
        private void RelayPhase(GamePhase phase) { PhaseChanged?.Invoke(phase); }
        private void OnDisable() { Unsubscribe(); }
        private void Unsubscribe()
        {
            if (subscribed != null) { subscribed.Changed -= RelayChanged; subscribed.PhaseChanged -= RelayPhase; }
            subscribed = null;
        }
    }
}
