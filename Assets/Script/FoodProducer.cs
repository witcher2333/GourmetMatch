using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

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

    [SerializeField]
    private int energyRecoverySeconds = 300;

    private DateTime lastEnergyTimeUtc;

    private void Start()
    {
        currentEnergy = Mathf.Max(0, maxEnergy);
        lastEnergyTimeUtc = DateTime.UtcNow;
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
            UnityEngine.Random.Range(0, possibleItems.Length);

        ItemData itemToProduce =
            possibleItems[randomIndex];

        // 现在传入的是 ItemData，而不是数字
        bool success =
            board.TrySpawnItem(itemToProduce);

        if (success)
        {
            bool wasFull = currentEnergy == maxEnergy;

            currentEnergy--;

            // 如果刚才是满体力，
            // 从此次消耗开始计算恢复时间
            if (wasFull)
            {
                lastEnergyTimeUtc =
                    DateTime.UtcNow;
            }

            Debug.Log(
                "消耗 1 点体力，剩余：" +
                currentEnergy
            );

            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        if (currentEnergy >= maxEnergy)
        {
            energyText.text =
                "Energy: " +
                currentEnergy +
                " / " +
                maxEnergy +
                "\nFull";
        }
        else
        {
            TimeSpan elapsed =
                DateTime.UtcNow -
                lastEnergyTimeUtc;

            int remainingSeconds =
                energyRecoverySeconds -
                Mathf.FloorToInt(
                    (float)elapsed.TotalSeconds
                );

            remainingSeconds =
                Mathf.Max(
                    0,
                    remainingSeconds
                );

            int minutes =
                remainingSeconds / 60;

            int seconds =
                remainingSeconds % 60;

            energyText.text =
                "Energy: " +
                currentEnergy +
                " / " +
                maxEnergy +
                "\nNext: " +
                minutes.ToString("00") +
                ":" +
                seconds.ToString("00");
        }

        if (produceButton != null)
        {
            produceButton.interactable =
                currentEnergy > 0;
        }
    }

    //using Json save the energy
    public int GetCurrentEnergy()
    {
        return currentEnergy;
    }

    public string GetLastEnergyTimeUtc()
    {
        return lastEnergyTimeUtc.ToString("O");
    }

    public void LoadEnergyState(
    int savedEnergy,
    string savedTimeUtc
)
    {
        currentEnergy =
            Mathf.Clamp(
                savedEnergy,
                0,
                maxEnergy
            );

        DateTime parsedTime;

        bool success =
            DateTime.TryParse(
                savedTimeUtc,
                null,
                System.Globalization.DateTimeStyles.RoundtripKind,
                out parsedTime
            );

        if (success)
        {
            lastEnergyTimeUtc =
                parsedTime.ToUniversalTime();
        }
        else
        {
            lastEnergyTimeUtc =
                DateTime.UtcNow;
        }

        // 立刻计算离线期间恢复的体力
        RecoverEnergy();

        UpdateUI();
    }

    public void SetCurrentEnergy(
    int energy
)
    {
        currentEnergy =
            Mathf.Clamp(
                energy,
                0,
                maxEnergy
            );

        UpdateUI();
    }

    private void RecoverEnergy()
    {
        // 已满体力，不继续累计
        if (currentEnergy >= maxEnergy)
        {
            currentEnergy = maxEnergy;

            lastEnergyTimeUtc = DateTime.UtcNow;

            return;
        }

        TimeSpan elapsed =
            DateTime.UtcNow - lastEnergyTimeUtc;

        int recoveredEnergy =
            Mathf.FloorToInt(
                (float)elapsed.TotalSeconds /
                energyRecoverySeconds
            );

        // 时间还没到
        if (recoveredEnergy <= 0)
        {
            return;
        }

        currentEnergy =
            Mathf.Min(
                currentEnergy + recoveredEnergy,
                maxEnergy
            );

        // 把时间向后推进已经使用掉的恢复周期
        lastEnergyTimeUtc =
            lastEnergyTimeUtc.AddSeconds(
                recoveredEnergy *
                energyRecoverySeconds
            );

        // 如果已经满了，从当前时间重新开始
        if (currentEnergy >= maxEnergy)
        {
            lastEnergyTimeUtc =
                DateTime.UtcNow;
        }

        UpdateUI();
    }

    private void Update()
    {
        RecoverEnergy();
    }
}