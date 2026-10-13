# 肥鹅美食街

一个使用 Unity 制作的 2D 食物合成与经营小游戏。玩家在棋盘上生产并合成食材，完成订单获得金币和经验，再通过商店购买资源继续经营。

## 项目环境

- Unity：6000.3.24f1
- 渲染管线：Universal Render Pipeline 2D
- UI：Unity UI（uGUI）与 TextMeshPro
- 输入系统：Input System
- Android 构建：需要 Android Build Support、Android SDK & NDK Tools 和 OpenJDK

建议使用 Unity Hub 安装或打开与项目一致的 Unity 版本。首次打开时，等待 Unity 完成资源导入和脚本编译。

## 当前功能

- 食物棋盘：拖动相同等级的食物进行合成升级。
- 生产器：点击生产食材；生产次数、冷却和体力会影响生产。
- 订单：提交所需食物，获得金币和经验。
- 玩家进度：经验累积后升级。
- 商店：使用金币购买食物或体力。
- 出售：选择普通物品后出售并获得金币。
- 存档：保存棋盘、订单、金币、等级、经验及相关运行状态。
- 中文 UI 与食物名称。

## 目录说明

```text
Assets/
├── Data/       食物数据、食物目录
├── Fonts/      字体文件与 TextMeshPro 字体资源
├── Prefabs/    可复用预制体
├── Scenes/     游戏场景
├── Script/     游戏逻辑脚本
├── Sprites/    食物图片
└── UI/         UI 相关资源
```

主要游戏场景：`Assets/Scenes/MainScene.unity`。

## 在编辑器中运行

1. 用 Unity Hub 打开项目。
2. 在 Project 窗口打开 `Assets/Scenes/MainScene.unity`。
3. 点击 Unity 编辑器顶部的 **Play** 按钮运行。
4. 点击生产器生成食材，拖动相同食物进行合成；通过订单、商店和出售按钮体验经营流程。

## 构建 Android APK

### 安装 Android 构建组件

在 Unity Hub 的 **Installs** 中，找到项目使用的 Unity 版本，打开模块管理并安装：

- Android Build Support
- Android SDK & NDK Tools
- OpenJDK

### 配置构建场景

1. 在 Unity 中打开 `Assets/Scenes/MainScene.unity`。
2. 打开 **File → Build Profiles**，选择或添加 **Android** 构建配置，并切换到 Android 平台。
3. 检查构建场景列表：确保 `MainScene` 已添加且启用。当前项目的构建设置中只有 `Assets/Scenes/SampleScene.unity`，打包前需要将 `MainScene` 加入列表，并移除或取消勾选 `SampleScene`。

### 设置 Android Player

打开 **Edit → Project Settings → Player → Android**：

- 修改 **Product Name** 为希望显示在手机上的游戏名称。
- 将 **Package Name** 改成自己的唯一应用标识，例如 `com.yourname.fatgoose`。不要保留默认的 `com.DefaultCompany.urp_2d`；正式发布后也不要随意更换包名。
- 本项目界面按竖屏布局设计，可将 **Default Orientation** 设为 **Portrait**。当前配置为自动旋转。
- Target API 可使用 Unity 提供的自动/已安装版本；只有在构建提示或发布平台有要求时再调整。

### 生成并安装 APK

1. 在 **Build Profiles** 中确认 Android 配置和场景列表无误。
2. 如果是安装到自己的手机测试，关闭 **Build App Bundle (Google Play)**，然后点击 **Build**。
3. 选择输出位置并生成 `.apk` 文件。
4. 将 APK 传到 Android 手机并安装。手机若阻止安装，请在系统设置中允许当前文件管理器安装未知应用。

测试安装可使用 Unity 的调试签名。正式发布时需要创建并妥善备份自定义 Keystore 和密码；丢失签名密钥可能导致后续版本无法作为原应用更新。

## 中文字体

项目使用 TextMeshPro 字体资源显示中文。若某些汉字显示为空框或缺字：

1. 使用支持中文的源字体生成 TMP Font Asset。
2. 将游戏界面、物品名称和提示语中会用到的字符加入字符集并生成图集。
3. 将生成的 TMP Font Asset 指定给文本组件，或加入 TMP Settings 的 **Fallback Font Assets**。
4. 确认生成的字体资源保存在 `Assets` 目录内，以便随项目一起构建。

## 主要脚本

- `MergeBoard.cs`、`BoardSlot.cs`：棋盘与格子管理。
- `MergeItem.cs`：物品交互、拖动、合成与生产器状态显示。
- `ItemData.cs`、`ItemCatalog.cs`：物品数据与目录。
- `FoodProducer.cs`：体力恢复和消耗。
- `OrderManager.cs`：订单、金币和奖励。
- `PlayerProgressManager.cs`：等级与经验。
- `ShopManager.cs`：商店购买逻辑。
- `SaveData.cs`、`SaveManager.cs`：存档数据和读写。
- `GameFeedbackManager.cs`：游戏反馈提示及相关表现。
