
using UnityEngine;
using ForestAdventure;

public class ManualLevelTester : MonoBehaviour
{
    private void Update()
    {
        // F1：進入第二關
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (GameManager.Instance == null)
                return;

            GameManager.Instance.AcceptQuest();
            GameManager.Instance.CompleteExploration();

            Debug.Log("目前關卡：" + GameManager.Instance.Phase);
            Debug.Log("目前擊殺數：" + GameManager.Instance.Kills);
            Debug.Log("目前寶石數：" + GameManager.Instance.Gems);
        }

        // F2：查看目前資料
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (GameManager.Instance == null)
                return;

            Debug.Log("目前關卡：" + GameManager.Instance.Phase);
            Debug.Log("目前擊殺數：" + GameManager.Instance.Kills);
            Debug.Log("目前寶石數：" + GameManager.Instance.Gems);
            Debug.Log("累計拾取寶石：" + GameManager.Instance.GemsCollectedTotal);
        }

        // F3：模擬擊敗 30 隻動物並拾取 30 顆寶石
        if (Input.GetKeyDown(KeyCode.F3))
        {
            GameManager gm = GameManager.Instance;

            if (gm == null || gm.Phase != GamePhase.Hunting)
            {
                Debug.LogWarning("請先進入 Hunting 才能測試！");
                return;
            }

            // 補足所需擊殺數
            while (gm.Kills < gm.requiredKills)
            {
                gm.RegisterKill();
            }

            // 補足所需寶石數
            int missingGems =
                gm.requiredCollectedGems - gm.GemsCollectedTotal;

            if (missingGems > 0)
            {
                gm.AddGems(missingGems);
            }

            Debug.Log(
                "[測試達標] Kills = " + gm.Kills +
                " / Gems = " + gm.GemsCollectedTotal
            );
        }
    }
}
