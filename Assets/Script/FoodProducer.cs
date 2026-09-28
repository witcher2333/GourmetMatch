using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodProducer : MonoBehaviour
{
    [SerializeField]
    private MergeBoard board;

    [SerializeField]
    private Button produceButton;

    [SerializeField]
    private TMP_Text energyText;

    [SerializeField]
    private int maxEnergy = 10;

    // 生产器可以生产哪些基础物品
    [SerializeField]
    private ItemData[] possibleItems;

    private int currentEnergy;

    private void Start()
    {
        currentEnergy = Mathf.Max(0, maxEnergy);

        UpdateUI();
    }

    public void Produce()
    {
        if (board == null)
        {
            Debug.LogError("没有设置 Board！");
            return;
        }

        if (currentEnergy <= 0)
        {
            Debug.Log("体力不足！");
            return;
        }

        if (possibleItems == null ||
            possibleItems.Length == 0)
        {
            Debug.LogError("生产器没有配置可生产食材！");
            return;
        }

        // 随机选择一种基础食材
        int randomIndex =
            Random.Range(0, possibleItems.Length);

        ItemData itemToProduce =
            possibleItems[randomIndex];

        // 现在传入的是 ItemData，而不是数字
        bool success =
            board.TrySpawnItem(itemToProduce);

        if (success)
        {
            currentEnergy--;

            Debug.Log(
                "消耗 1 点体力，剩余：" +
                currentEnergy
            );

            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (energyText != null)
        {
            energyText.text =
                "Energy: " +
                currentEnergy +
                " / " +
                maxEnergy;
        }

        if (produceButton != null)
        {
            produceButton.interactable =
                currentEnergy > 0;
        }
    }
}