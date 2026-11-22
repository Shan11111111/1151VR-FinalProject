using System;
using UnityEngine;

namespace ForestAdventure
{
    public enum GamePhase { Explore, Hunting, Boss, Won, Lost }
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        [Header("Inspector adjustable rules")]
        [Min(1)] public int requiredKills = 30;
        [Min(1)] public int requiredCollectedGems = 30;
        [Min(10)] public float explorationSeconds = 180f;
        [Min(10)] public float huntingSeconds = 240f;
        [Min(10)] public float bossSeconds = 120f;
        public bool persistAcrossScenes;
        public GamePhase Phase { get; private set; } = GamePhase.Explore;
        public GamePhase LastPlayablePhase { get; private set; } = GamePhase.Explore;
        public bool QuestAccepted { get; private set; }
        public bool SpecialGunUnlocked { get; private set; }
        public bool BossPortalActive => Phase == GamePhase.Hunting && SpecialGunUnlocked;
        public int Kills { get; private set; }
        public int Gems { get; private set; }
        public int GemsCollectedTotal { get; private set; }
        public float TimeLeft { get; private set; }
        public bool HuntingComplete => Kills >= requiredKills;
        public PlayerProgressData Progress { get; private set; } = new PlayerProgressData();
        public event Action Changed;
        public event Action<GamePhase> PhaseChanged;
        public event Action PlayerEaten;
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            TimeLeft = explorationSeconds;
            if (persistAcrossScenes) DontDestroyOnLoad(gameObject);
        }
        private void OnDestroy() { if (Instance == this) Instance = null; }
        private void Update()
        {
            if (Phase != GamePhase.Explore && Phase != GamePhase.Hunting && Phase != GamePhase.Boss) return;
            TimeLeft = Mathf.Max(0f, TimeLeft - Time.deltaTime);
            if (TimeLeft <= 0f) ChangePhase(GamePhase.Lost);
        }
        public void CapturePlayer()
        {
            var combat = FindFirstObjectByType<PlayerCombat>();
            Progress.Capture(combat, this);
        }
        public void AcceptQuest()
        {
            if (Phase != GamePhase.Explore) return;
            QuestAccepted = true; NotifyChanged();
        }
        public void CompleteExploration()
        {
            if (Phase != GamePhase.Explore || !QuestAccepted) return;
            CapturePlayer();
            TimeLeft = huntingSeconds;
            ChangePhase(GamePhase.Hunting);
        }
        public void RegisterKill()
        {
            if (Phase != GamePhase.Hunting || HuntingComplete) return;
            Kills++; NotifyChanged();
        }
        public void AddGems(int amount)
        {
            if (Phase != GamePhase.Hunting || amount <= 0) return;
            Gems += amount;
            GemsCollectedTotal += amount;
            NotifyChanged();
        }
        // Unlock the *new gun*, but do not enter Boss until the portal is touched.
        public bool TryUnlockSpecialGun()
        {
            if (Phase != GamePhase.Hunting || !HuntingComplete || GemsCollectedTotal < requiredCollectedGems) return false;
            if (SpecialGunUnlocked) return true;
            SpecialGunUnlocked = true;
            var combat = FindFirstObjectByType<PlayerCombat>();
            if (combat != null) { combat.UnlockSpecialGun(); combat.AddReserveAmmo(combat.specialUnlockAmmo); }
            CapturePlayer(); NotifyChanged();
            return true;
        }
        public bool EnterBossPortal()
        {
            if (!BossPortalActive) return false;
            CapturePlayer();
            TimeLeft = bossSeconds;
            ChangePhase(GamePhase.Boss);
            return true;
        }
        // Legacy method retained for compatibility with older demo helpers.
        public bool TryUnlockBoss() => TryUnlockSpecialGun() && EnterBossPortal();
        public void BossDefeated() { if (Phase == GamePhase.Boss) ChangePhase(GamePhase.Won); }
        public void EatenByBoss()
        {
            if (Phase != GamePhase.Boss) return;
            PlayerEaten?.Invoke(); ChangePhase(GamePhase.Lost);
        }
        public void RestartPhase()
        {
            GamePhase restart = LastPlayablePhase;
            Phase = restart;
            TimeLeft = restart == GamePhase.Explore ? explorationSeconds : restart == GamePhase.Hunting ? huntingSeconds : bossSeconds;
            if (restart == GamePhase.Explore) QuestAccepted = false;
            if (restart == GamePhase.Hunting) { Kills = 0; Gems = 0; GemsCollectedTotal = 0; SpecialGunUnlocked = false; }
            NotifyChanged(); PhaseChanged?.Invoke(Phase);
        }
        public void ResetRun()
        {
            Phase = GamePhase.Explore; LastPlayablePhase = GamePhase.Explore;
            QuestAccepted = false; Kills = 0; Gems = 0; GemsCollectedTotal = 0; SpecialGunUnlocked = false;
            TimeLeft = explorationSeconds;
            Progress = new PlayerProgressData(); NotifyChanged(); PhaseChanged?.Invoke(Phase);
        }
        public void NotifyChanged() { Changed?.Invoke(); }
        private void ChangePhase(GamePhase phase)
        {
            if (phase == Phase) return;
            Phase = phase;
            if (phase == GamePhase.Explore || phase == GamePhase.Hunting || phase == GamePhase.Boss) LastPlayablePhase = phase;
            NotifyChanged(); PhaseChanged?.Invoke(phase);
        }
    }
}
