using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerProgressManager :
    MonoBehaviour
{
    [SerializeField]
    private TMP_Text levelText;

    [SerializeField]
    private TMP_Text xpText;

    [SerializeField]
    private Slider xpSlider;

    [SerializeField]
    private int baseXpPerLevel = 50;

    // 玩家至少从 1 级开始
    private int playerLevel = 1;

    // 当前等级已经拥有的经验
    private int currentXp = 0;

    private void Start()
    {
        UpdateUI();
    }

    // 增加玩家经验
    public void AddXp(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentXp += amount;

        Debug.Log(
            "获得 " +
            amount +
            " XP"
        );

        // 使用 while 而不是 if，
        // 避免一次获得大量经验时只能升一级
        while (
            currentXp >=
            GetXpNeededForNextLevel()
        )
        {
            int requiredXp =
                GetXpNeededForNextLevel();

            currentXp -= requiredXp;
            playerLevel++;

            Debug.Log(
                "玩家升级！当前等级：" +
                playerLevel
            );
        }

        UpdateUI();
    }

    // 当前升级需要多少经验
    public int GetXpNeededForNextLevel()
    {
        return Mathf.Max(
            1,
            playerLevel *
            baseXpPerLevel
        );
    }

    public int GetPlayerLevel()
    {
        return playerLevel;
    }

    public int GetCurrentXp()
    {
        return currentXp;
    }

    // 决定订单允许出现的最高物品等级
    public int GetMaxUnlockedItemLevel()
    {
        // 玩家 Lv.1 → 物品最高 Lv.2
        // 玩家 Lv.2 → 物品最高 Lv.3
        // 玩家 Lv.3 以上 → 物品最高 Lv.4
        return Mathf.Clamp(
            playerLevel + 1,
            2,
            4
        );
    }

    // 从存档恢复
    public void LoadState(
        int savedLevel,
        int savedXp
    )
    {
        playerLevel =
            Mathf.Max(
                1,
                savedLevel
            );

        currentXp =
            Mathf.Max(
                0,
                savedXp
            );

        // 防止异常存档中的 XP
        // 已经超过升级需要
        while (
            currentXp >=
            GetXpNeededForNextLevel()
        )
        {
            int requiredXp =
                GetXpNeededForNextLevel();

            currentXp -= requiredXp;
            playerLevel++;
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        int requiredXp =
            GetXpNeededForNextLevel();

        if (levelText != null)
        {
            levelText.text =
                "Level: " +
                playerLevel;
        }

        if (xpText != null)
        {
            xpText.text =
                "XP: " +
                currentXp +
                " / " +
                requiredXp;
        }

        if (xpSlider != null)
        {
            xpSlider.wholeNumbers =
                true;

            xpSlider.minValue =
                0;

            xpSlider.maxValue =
                requiredXp;

            xpSlider.value =
                currentXp;
        }
    }
}