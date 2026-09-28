
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodProducer : MonoBehaviour
{
    // 棋盘系统
    [SerializeField]
    private MergeBoard board;

    // 生产按钮
    [SerializeField]
    private Button produceButton;

    // 显示体力的文字
    [SerializeField]
    private TMP_Text energyText;

    // 最大体力
    [SerializeField]
    private int maxEnergy = 10;

    // 当前体力
    private int currentEnergy;

    private void Start()
    {
        // 游戏开始时，体力恢复到最大值
        currentEnergy = Mathf.Max(0, maxEnergy);

        // 更新屏幕上的体力数字
        UpdateUI();
    }

    // 点击按钮时调用这个方法
    public void Produce()
    {
        // 检查是否已经连接棋盘
        if (board == null)
        {
            Debug.LogError("没有设置 Board！");
            return;
        }

        // 检查体力是否足够
        if (currentEnergy <= 0)
        {
            Debug.Log("体力不足，无法生产！");
            return;
        }

        // 尝试生产一个 1 级物品
        bool success = board.TrySpawnItem(1);

        // 只有生产成功，才扣除体力
        if (success)
        {
            currentEnergy--;

            Debug.Log("消耗 1 点体力，剩余：" + currentEnergy);

            UpdateUI();
        }
    }

    // 更新游戏界面
    private void UpdateUI()
    {
        // 更新体力文字
        if (energyText != null)
        {
            energyText.text =
                "Energy: " + currentEnergy + " / " + maxEnergy;
        }

        // 没有体力时，让按钮不可点击
        if (produceButton != null)
        {
            produceButton.interactable = currentEnergy > 0;
        }
    }
}