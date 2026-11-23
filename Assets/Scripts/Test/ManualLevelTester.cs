
using UnityEngine;
using ForestAdventure;

public class ManualLevelTester : MonoBehaviour
{
    private void Update()
    {
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

        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (GameManager.Instance == null)
                return;

            Debug.Log("目前關卡：" + GameManager.Instance.Phase);
            Debug.Log("目前擊殺數：" + GameManager.Instance.Kills);
            Debug.Log("目前寶石數：" + GameManager.Instance.Gems);
            Debug.Log("累計拾取寶石：" + GameManager.Instance.GemsCollectedTotal);
        }
    }
}
