using System;
using System.Collections.Generic;

[Serializable]
public class SlotSaveData
{
    public int slotIndex;
    public string itemId;

    // Producer 当前剩余次数
    public int producerCharges;

    // Producer 冷却结束时间
    public string producerCooldownEndUtc;
}

[Serializable]
public class OrderRequirementSaveData
{
    // 订单需要的物品 ID
    public string itemId;

    // 需要多少个
    public int amount;
}

[Serializable]
public class GameSaveData
{
    //棋盘上的物品
    public List<SlotSaveData> boardItems = new List<SlotSaveData>();

    //coin
    public int coins;

    //energy
    public int energy;

    //当前订单在数组中的位置
    public int orderIndex;

    // 当前随机订单的所有要求
    public List<OrderRequirementSaveData>
        orderRequirements =
            new List<OrderRequirementSaveData>();

    // 当前订单的奖励
    public int orderReward;

    //最后一次计算体力恢复的时间
    public string lastEnergyTimeUtc;

    // 玩家等级
    public int playerLevel;

    // 当前等级拥有的经验
    public int currentXp;
}