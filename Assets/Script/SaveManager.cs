using System.Collections;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    [SerializeField]
    private MergeBoard board;

    [SerializeField]
    private FoodProducer producer;

    [SerializeField]
    private OrderManager orderManager;

    [SerializeField]
    private ItemCatalog catalog;

    [SerializeField]
    private PlayerProgressManager
    playerProgress;

    private string savePath;

    private void Awake()
    {
        savePath =
            Path.Combine(
                Application.persistentDataPath,
                "save.json"
            );

        Debug.Log(
            "存档路径：" + savePath
        );
    }

    private IEnumerator Start()
    {
        // 等一帧，让 MergeBoard 先创建好 16 个格子
        yield return null;

        LoadGame();
    }

    public void SaveGame()
    {
        if (board == null || producer == null || orderManager == null || playerProgress == null)
        {
            Debug.LogError(
                "SaveManager 缺少引用！"
            );

            return;
        }

        GameSaveData saveData =
            new GameSaveData();

        // 棋盘
        saveData.boardItems =
            board.GetBoardSaveData();

        // 体力
        saveData.energy =
            producer.GetCurrentEnergy();

        saveData.lastEnergyTimeUtc =
    producer.GetLastEnergyTimeUtc();

        // 金币
        saveData.coins =
            orderManager.GetCoins();

        // 玩家等级和经验
        saveData.playerLevel =
            playerProgress.GetPlayerLevel();

        saveData.currentXp =
            playerProgress.GetCurrentXp();

        // 当前随机订单内容
        saveData.orderRequirements =
            orderManager.GetOrderSaveData();

        // 当前订单奖励
        saveData.orderReward =
            orderManager.GetCurrentOrderReward();

        // 转换成 JSON
        string json =
            JsonUtility.ToJson(
                saveData,
                true
            );

        // 写入文件
        File.WriteAllText(
            savePath,
            json
        );

        Debug.Log(
            "游戏已保存：\n" + json
        );
    }

    public void LoadGame()
    {
        // 第一次玩，没有存档
        if (!File.Exists(savePath))
        {
            Debug.Log(
                "没有找到存档，开始新游戏。"
            );

            return;
        }

        string json =
            File.ReadAllText(savePath);

        GameSaveData saveData =
            JsonUtility.FromJson<GameSaveData>(
                json
            );

        if (saveData == null)
        {
            Debug.LogError(
                "存档读取失败！"
            );

            return;
        }

        // 恢复棋盘
        board.LoadBoard(
            saveData.boardItems,
            catalog
        );

        // 恢复体力
        producer.SetCurrentEnergy(
            saveData.energy
        );
        //新的load方法 取代上面的
        producer.LoadEnergyState(
    saveData.energy,
    saveData.lastEnergyTimeUtc
);

        // 必须先恢复玩家等级，
        // 再恢复或生成订单
        playerProgress.LoadState(
            saveData.playerLevel,
            saveData.currentXp
        );

        // 恢复金币和随机订单
        orderManager.LoadState(
            saveData.coins,
            saveData.orderRequirements,
            saveData.orderReward,
            catalog
        );

        Debug.Log(
            "游戏存档读取完成。"
        );
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    //临时快捷键
    private void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current == null)
        {
            return;
        }

        if (UnityEngine.InputSystem.Keyboard.current.sKey.wasPressedThisFrame)
        {
            SaveGame();
        }

        if (UnityEngine.InputSystem.Keyboard.current.lKey.wasPressedThisFrame)
        {
            LoadGame();
        }
    }
}