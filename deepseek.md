# DeepSeek 改动与诊断记录

> 供 Codex / 其他 agent 对齐用。记录本次会话对 Poolcore 的全部改动、**实测数据**、
> **已排除的假设**（避免重复劳动）与**踩坑记录**。所有数字均由 Unity CLI 在真实 Editor 中测得，非推测。

会话日期：2026-09-11
Unity CLI：`C:/Users/15792/AppData/Local/Unity/bin/unity.exe`
项目：`E:\code\test_astra\Poolcore\Poolcore`（`unity status` → 端口 7800，Unity 6000.6.0f1）

---

## 0. 一句话现状（Codex 先看这里）

- **问题**：水池边缘在**移动视角**时来回闪烁（静止不可见）。
- **根因**：**视角运动混叠**（view-motion aliasing）。相机原本只有 **SMAA**（纯空间滤波，结构上无法处理运动混叠）。
- **修复**：相机改 **TAA**（`m_Antialiasing: 3` / High），**并且必须关掉 MSAA** —— URP 在 MSAA 开启时会
  **静默禁用 TAA**，这一条曾让第一次修复在游戏里形同无效。
- **状态**：已实测、已重新打包、已推送。**待人工验收**：TAA 在长时间行走下的拖影观感。
- **⚠️ 验收位置**：必须看 **Game 视图（Play）或打包 exe**。**在 Scene 视图里看永远是闪的**，那不代表游戏有问题（见 §7-9）。

---

## 1. 改动清单

| 文件 | 改动 |
|---|---|
| `Assets/Poolcore/Scenes/Poolrooms.unity` | `First Person Camera` 的 `UniversalAdditionalCameraData`：`m_Antialiasing` 由 `2`(SMAA) → `3`(TAA)，`m_AntialiasingQuality: 2`(High)。场景已保存。 |
| `Assets/Poolcore/Scripts/ExperienceMenu.cs` | **关键**：`ApplyQuality()` 原把运行时 URP 克隆的 `msaaSampleCount` 设为 `quality==2?4:2`，**MSAA 会静默禁用 TAA**。改为恒为 `1`。 |
| `Assets/Poolcore/Editor/PoolroomsBuilder.cs` | `Create()` 中相机抗锯齿同步改为 TAA（附原因注释），防止重新生成场景时退回 SMAA。 |
| `Tools/diag/*.cs` | 新增 16 个可复现诊断脚本（`eval_file` 的 body 形式，**不在 `Assets/` 下**，不会被 Unity 编译）。 |
| `deepseek.md` | 本文件。 |

**由 Unity 自行写入、经确认后一并提交**（非手工编辑，为保持仓库与工程一致）：

| 文件 | 内容 |
|---|---|
| `ProjectSettings/ProjectSettings.asset` | `preloadedAssets` 由 `[]` → 登记 `InputSystem_Actions.inputactions`（Input System 的 Project-wide Actions） |
| `Assets/Resources.meta` | 空 `Assets/Resources/` 文件夹的 meta 被重新生成 |

**确实未改动**：`StillWater.shader`、`Porcelain.shader`、任何 `.mat`、`Movement.asset`、
`FirstPersonController.cs`、`WaterZone.cs`、`PoolAudio.cs`、`Foundation.unity`。
诊断脚本对材质属性的读写**全部在 `finally` 中还原**，材质文件时间戳与会话前一致（已核对）。

---

## 2. 症状与根因

**症状**：水池边缘在**一动视角**时于两种状态间来回闪烁；静止时看不到。

**排除过程的关键推理**：「静止不可见」直接排除了所有**时间驱动**的动画（水面 `_Ripple`、焦散
`_Caustics` 静止时仍在动）。「两种离散状态来回跳」是**欠采样/混叠**的特征（亚像素运动导致采样相位翻转），
而不是几何或逻辑错误。

**为什么是池沿最明显**：池沿是全场景对比度最高处 —— 阳光直射的亮色地面（`Wet Ivory` 0.65,0.73,0.69）
紧邻极暗的排水沟（`Deep Green Trim` 0.12,0.28,0.26），再叠加程序化瓷砖缝。高对比 + 高频 = 最易混叠。

**为什么原设置无效**：相机只有 **SMAA**。SMAA 是**纯空间**滤波器，逐帧独立处理，
**结构上无法消除帧间运动混叠** —— 它只能改善单帧内的边缘锯齿。

### 2.1 关键陷阱：MSAA 会静默禁用 TAA（第一次修复失效的原因）

只把相机改成 TAA **不够**。URP 在 MSAA 开启时直接放弃 TAA，并打印：

```
Disabling TAA because MSAA is on. Turn MSAA off on the camera or current URP Asset.
```

本项目有两处会触发：

1. **`ExperienceMenu.ApplyQuality()`（运行时，真正的元凶）**
   它 `Instantiate` 出一个 URP 资产克隆并设 `msaaSampleCount = 4/2`。于是
   **Play Mode 与打包版里 TAA 一直被顶掉**；而 **Edit 模式**用的是原始 `PC_RPAsset`（`msaa=1`），
   TAA 正常 —— 造成极具迷惑性的「编辑器里看着好了、跑起来还是闪」。**已改为恒为 `1`。**

2. **Scene 视图（编辑器，与游戏无关但会误导判断）**
   Scene 视图有自己独立的抗锯齿开关（`SceneView.antiAliasing = 1`），且它的 `SceneCamera` 上
   **根本没有 `UniversalAdditionalCameraData`**，因此**完全不经过游戏相机的 AA 设置**（见 §7-9）。

**审计结论**（`Tools/diag/pipelines.cs`）：`GraphicsSettings.defaultRenderPipeline = null`；
两个质量档（`Mobile` / `PC`）分别指向 `Mobile_RPAsset` / `PC_RPAsset`，两者 **`msaa=1`**；
`QualitySettings.antiAliasing = 0`。即修复后 MSAA 全关，TAA 全程有效。

---

## 3. 实测数据

**测量方法**：相机固定到池沿近景（`(9.0,1.8,0)` 看向 `(7.0,-0.3,3.5)`，与用户截图机位一致），
`Camera.Render()` 渲到 RenderTexture 读回像素，对比**一帧真实视角运动**前后的逐像素最大通道差。
`>16/32/64` = 跳变幅度超过该值的像素个数（画面 1000×800 = 800,000 px）。
`rot 0.02°` ≈ 鼠标每帧转动量；`trans 0.3 mm` ≈ 亚像素平移。

### 3.1 Play Mode，冻结动画（**权威数据**，`playmode-verify.cs`）

测量前隐藏全部水面渲染器并置 `_Caustics = 0`，以排除 Editor tick 带来的动画污染（见 §7-11）。

| AA 模式 | trans `>16/32/64` | rot `>16/32/64` | max |
|---|---|---|---|
| **TAA（现修复）** | 234/135/6 | **259/65/12** | **80** |
| SMAA（原出厂） | 812/281/4 | 1867/466/15 | 96 |
| None | 596/477/141 | 1708/1047/321 | **207** |

**旋转（最贴近"转视角"）改善 7.2×（466 → 65）；max 由 96 降至 80。**

### 3.2 Edit Mode，直接读场景配置、不覆盖相机设置（`verify-fix.cs`）

| 运动 | 修复前 SMAA | 修复后 TAA |
|---|---|---|
| rot `>16` | 1803 | 269 |
| rot `>32` | 481 | 70 |
| trans `>32` | 258 | 109 |
| max | 96 | 80 |

### 3.3 AA 模式 × MSAA 组合（Play Mode，`playmode-ab.cs`）

| 配置 | trans `>32` | rot `>32` | max |
|---|---|---|---|
| TAA + msaa=4（旧的坏状态） | 91 | 72 | 80 |
| SMAA + msaa=4 | 257 | 474 | 96 |
| None + msaa=4 | 453 | 1057 | 207 |
| TAA + msaa=1（现状态） | 100 | 76 | 80 |

> 注：`msaa=4` 与 `msaa=1` 下 TAA 的数值接近，是因为该组测量的 `eval` 渲染发生在
> Editor 已不再产生 `Disabling TAA` 警告之后；**判断 TAA 是否真的生效，不能只靠这组数字**，
> 必须结合 §2.1 的 URP 互斥规则与 §7-10 的警告增量判据。

---

## 4. 已排除的假设（含数据，避免重复劳动）

| 假设 | 检验方法 | 结果 |
|---|---|---|
| 池沿与地面共面重叠 → z-fighting | 沿世界坐标线逐点采样像素 + 2 mm 位移 | **排除**。重叠带渲成 `Wet Ivory`（R/G≈0.72）而非 `Sea Glass Mosaic`（R/G≈0.47），稳定无跳变。**完全共面时深度序与视角无关**，胜者由绘制顺序固定决定 |
| 水面折射 / 波纹 | 隐藏全部水面渲染器 | **排除**。`noWater` 与 base 几乎完全相同（394 vs 389 px，max 210 相同）→ 翻转像素**不在水面上** |
| 池壁 caustics 欠采样 | 1× vs 2× 超采样下行采样对比，`_Caustics` 开/关 | **排除**。4864 → 4793（无变化）。caustics 波长约 1.9 m，属低频，本就不易混叠 |
| 水面折射混叠 | 同上，`_Ripple` 开/关 | **排除**。4824 → 4825（无变化） |
| 镜面高光混叠 | `_Smoothness` 0.65→0.45→0.25→0.00 扫描 | **排除**。rot `>32` = 449/431/459/468，max 恒为 ~96 |
| 阳光阴影贴图抖动 | 关闭方向光阴影 | **反向**。rot `>64` 241 → 1156，max 210 → 228（更糟），不是原因 |
| 抗锯齿整体无效 | None / SMAA / TAA 对照 | **部分错误**。SMAA 显著优于 None，但**只有 TAA** 能压运动翻转 |

**开关有效性已单独验证**（否则 A/B 无意义）：关 caustics 影响 22,669 px（max 69）、
关 `_Ripple` 影响 10,368 px、隐藏水面影响 123,122 px —— 材质开关确实生效。

**⚠️ 测量陷阱 A**：SMAA 属于**后处理**效果。若把 `renderPostProcessing = false` 再对比"开/关 SMAA"，
两者完全相同，会得出"抗锯齿无效"的**错误结论**。本会话一开始就踩了这个坑，已纠正。

**⚠️ 测量陷阱 B**：见 §7-11（Editor tick 时动画污染测量）。

---

## 5. 未修复的已知隐患（建议但**本次未实施**）

### 5.1 三个水池的池沿与地面精确共面重叠

`PoolroomsBuilder.Pool()` 生成的池壁以池边线为**中心**（厚 0.16 m），而池边地面延伸到同一坐标，
因此两者**顶面精确共面于 y=0，且重叠 0.08 m 宽**。全场景共 **177 处共面重叠对**，其中 40 处**材质不同**；
面积最大的 12 处正是三个水池的池沿，合计 **9.28 m²**：

```
Y-max @ 0.0000  area 1.1200 m2  [Atrium West / Sea Glass Mosaic] <-> [Atrium West / Wet Ivory]
... (Atrium 四边 4×1.12 m²，Quiet 合计 2.40 m²，Warm 合计 2.40 m²)
```

**当前不闪**（见 §4 第一条），但它依赖绘制顺序的稳定性：一旦改动 near/far、换渲染后端或图形 API，
就可能翻转为真实闪烁。**建议修掉。**

建议补丁（把池壁改为纯内侧包边，与地面**边对边**相接而非重叠）：

```csharp
// 原：以池边线为中心，顶面与地面共面
Box(name+" West", new Vector3(x0,-0.45f,(z0+z1)/2), new Vector3(0.16f,0.9f,z1-z0), mat);
// 改为：整条池壁落在池口内侧，外侧面与地面边缘齐平
const float t = 0.08f;   // 半厚
Box(name+" West", new Vector3(x0+t,-0.45f,(z0+z1)/2), new Vector3(0.16f,0.9f,z1-z0), mat);
Box(name+" East", new Vector3(x1-t,-0.45f,(z0+z1)/2), new Vector3(0.16f,0.9f,z1-z0), mat);
Box(name+" North",new Vector3((x0+x1)/2,-0.45f,z1-t), new Vector3(x1-x0,0.9f,0.16f), mat);
Box(name+" South",new Vector3((x0+x1)/2,-0.45f,z0+t), new Vector3(x1-x0,0.9f,0.16f), mat);
// 台阶须让开包边，否则第 0 级顶面(y=0)会与包边顶面(y=0)产生新的同材质共面重叠：
//   z0+0.25f+i*0.5f  →  z0+0.16f+0.25f+i*0.5f
```

**未实施的原因**：① 可见包边宽度会由 8 cm 变为 16 cm（观感变化，需确认）；
② 几何变动会使烘焙光照失效，需重新 `Poolcore/Bake Soft Lighting` 并重跑
`Poolcore/Capture Pool Reflections`。二者都超出"修闪烁"的最小改动范围。

### 5.2 其余不同材质共面重叠（面积小，未处理）

`Connector Roof`×`Warm North Lintel`、`Passage Ceiling`×`Warm West Lintel` 等天花板/过梁交界处，
单处约 0.07 m²，共 28 处。位于天花角落，影响很小。

---

## 6. 诊断工具（`Tools/diag/`）

运行方式：`unity command eval_file --file <绝对路径>`。
脚本为 C# **语句体**（非类，无 `using`），会被包进方法执行，`return <string>` 即为结果。

| 脚本 | 作用 |
|---|---|
| `zfight-check.cs` | 通用共面重叠探测器（轴对齐盒体、同侧朝向、面积重叠），输出不同材质冲突排序表 |
| `rim-scan.cs` | 沿世界坐标线逐点采样像素，判断池沿"重叠带"实际由哪种材质胜出，并做微位移翻转判定 |
| `alias-probe.cs` | 1× 与 2× 超采样下行采样对比，量化空间欠采样并输出热力图 |
| `alias-ab.cs` | `_Caustics` / `_Ripple` 开关 A/B（含自动还原） |
| `verify-toggle.cs` | 验证材质开关是否真的影响渲染（A/B 前置检查，必做） |
| `shimmer.cs` | 合成行走路径下的逐像素时间方差图 + 归因 A/B |
| `flip-attr.cs` | 池沿翻转的逐项归因（水面/包边/排水沟/阴影/AA/后处理） |
| `flip-metric.cs` | 翻转像素定位图（含隐藏水面对照） |
| `flip-gloss.cs` | `_Smoothness` 扫描 |
| `flip-taa.cs` | AA 模式对照（含 TAA 时间累积预热） |
| `verify-fix.cs` | 读相机**实际**设置做修复前后 A/B，并输出池沿对照图 |
| `playmode-verify.cs` | **Play Mode 干净对照**（先冻结动画），TAA/SMAA/None 三方比较 —— 最常用 |
| `playmode-ab.cs` | Play Mode 下 AA 模式 × MSAA 组合对照 |
| `pipelines.cs` | 审计所有质量档的 URP 资产与其 MSAA（确认 TAA 不会被顶掉） |
| `state-audit.cs` | 相机/角色位姿、AA、场景 dirty 状态审计（用于确认探测未留残留状态） |
| `sceneview-probe.cs` | 证明 Scene 视图 `SceneCamera` 无 `UniversalAdditionalCameraData` |

原始运行产物在 `artifacts/diag/`（该目录已被 `.gitignore` 排除）：
`zfight-before.txt`、`flip-before.txt`、`flip-attr.txt`、`flip-gloss.txt`、`flip-taa.txt`、
`fix-baseline.txt`、`fix-after.txt`、`playmode-verify.txt`、`playmode-ab.txt`、
`rim-crop.png`、`rim-beauty-*.png`、`shimmer-base.png` 等。

---

## 7. 环境注意事项（踩坑记录）

1. **`eval` 的 `--code` 内联参数会被 CLI 解析器按空格切碎**，必须用 `eval_file --file <路径>`。
2. **`capture_game_view --save_path` 被 authoring root 限制在 `Assets/` 下**：传绝对路径也会落到
   `Assets/artifacts/…` 并触发导入，污染工程。改用 `eval_file` 自己 `File.WriteAllBytes` 到任意路径。
   若已误建，需删除 `Assets/artifacts/` 与 `Assets/artifacts.meta` 后 `AssetDatabase.Refresh`。
3. **Editor 是否推进 `Time` 取决于窗口是否在渲染/tick，不可假设。**
   本会话观察到了两种状态：一次 `Time.frameCount` 卡在 `2`（`set_autotick` 也无效），
   另一次为 `frame=1110 time=9.423`（正常推进）。
   **先打印 `Time.frameCount` 判断，再决定测量策略**；凡涉及动画或时间累积（TAA）的测量，
   要么先冻结动画，要么确保在同一 tick 内完成。
   （本会话为让渲染生效曾执行 `set_autotick --enable true --interval_ms 200`，
   未加 `--persist`，仅对该会话有效。）
4. `FindObjectsByType<T>(FindObjectsSortMode.None)` 在 Unity 6000.6 已过时，改用 `FindObjectsByType<T>()`。
5. 在 `eval_file` 里写委托时：`System.Func<...>` 的泛型参数个数**必须包含返回类型**；
   `delegate{...}` 赋值语句结尾是 `};` 而**不是** `});`（本会话犯了两次）。
6. **HTTPS 推送必须绕开 Schannel。** 本机 `http.sslbackend=schannel`
   （`C:/Program Files/Git/etc/gitconfig`）。直接 `git push` 失败：
   `schannel: AcquireCredentialsHandle failed: SEC_E_NO_CREDENTIALS (0x8009030e)`。
   这**不是网络被封**（TCP 443 可达，且公共仓库 `git ls-remote` 同样失败），而是 Schannel 取不到凭据句柄。
   改用 Git for Windows 自带的 OpenSSL 后端即可（CA bundle 已存在）：
   ```
   git -c http.sslBackend=openssl push -u origin main
   ```
7. **凭据助手需要具名管道。** 受限沙箱下推送还会报
   `sh.exe: *** fatal error - couldn't create signal pipe, Win32 error 5`
   与 `could not read Username for 'https://github.com'` —— Git Credential Manager 无法启动。
   需在放宽沙箱权限（`danger-full-access`）下执行推送命令。
8. **仓库状态**：远端 `https://github.com/suching8848/poolcore_test_unity.git`，分支 `main`。
   提交序列：`94fbbc6`（TAA 修复）→ `cbc0fbc`（文档）→ `8d3df1a`（MSAA 修复 + 文档同步）。
   远端 `HEAD` = `8d3df1a`。项目目录在父仓库 `E:\code\test_astra` 中**保持未跟踪**，未影响父仓库。
9. **Scene 视图与 Game 视图不是一回事（最容易误判的一条）。**
   Scene 视图的 `SceneCamera` 没有 `UniversalAdditionalCameraData`，
   **完全不使用**游戏相机的 TAA/SMAA 设置，所以**在 Scene 视图里永远比游戏里更闪**。
   判断画面质量**必须**看 Game 视图（Play）或打包版；在 Scene 视图里评估渲染修复会得出错误结论。
   Scene 视图自身的抗锯齿开关是 `SceneView.antiAliasing`（本机原值 `1`，本会话临时改为 `0` 验证后**已还原为 1**）。
10. **`clear_console` 并不清空 `console` 命令读取的日志缓冲区。** 它返回成功，但旧 warning 仍在。
    判断"某条 warning 是否还在产生"不能看总数，要看**单位时间的增量**
    （本次靠"12 秒增量 = 0"才确认 `Disabling TAA` 已停止产生）。
11. **Editor 在 tick 时，`eval_file` 内多次 `Camera.Render()` 之间 `_Time` 会推进。**
    有动画的材质（水面 `_Ripple`、焦散 `_Caustics`）会在两个采样点之间变化，
    把"动画"混进"闪烁"测量 —— 本次一度得到 `trans >32 = 6233` 的假异常值（真实值约 250）。
    **测运动混叠前必须冻结动画**（隐藏水面渲染器 + `_Caustics = 0`）。
12. `build_status` 的 `data.result` 是**一个 JSON 字符串**（不是对象），需二次 `ConvertFrom-Json`；
    否则 `$o.data.result.status` 恒为空。
13. **构建产物用文件时间戳核对最可靠**：增量构建**不会重写 `Poolrooms.exe`** 本身，
    但会重写 `Poolrooms_Data/` 与 `Poolrooms_Data/Managed/Assembly-CSharp.dll`。
    只看 exe 时间戳会误判为"没构建"。

---

## 8. 构建与验收状态

- **已重新打包 Windows player**（增量构建，`buildId=build_9d8f118afa09`，12:49 完成）：
  `result=Succeeded`，`totalErrors=0`，`totalWarnings=4`，`totalSizeBytes=133,725,718`，
  `outputPath=Builds/Poolrooms/Poolrooms.exe`。构建包含 URP 资产 `PC_RPAsset`（`msaa=1`）。
- **产物核验**：`Poolrooms_Data/Managed/Assembly-CSharp.dll` = 12:49:33、
  `sharedassets0.assets` = 12:49:32，均含本次 TAA + MSAA 修复；12:40 之后共写入 12 个文件。
- **冒烟测试**：`-batchmode -nographics` 启动后运行 10 秒正常，Input System 初始化成功，
  日志无 Error/Exception（仅一条 batch 模式下的 `Curl error 23` 日志写入噪声，非游戏错误）。
- **人工验收步骤**：双击 `Builds\Poolrooms\Poolrooms.exe` → 点「进入空间」→ 走到池边左右转视角。
  或在 Editor 按 **Play**、点画面进入空间后同样操作。**不要用 Scene 视图验收。**

---

## 9. 本次**未**完成 / 未验证（残余风险）

- **TAA 的副作用未做长时间人工验收**：TAA 可能带来轻微拖影/软化；**透明水面没有 motion vector**，
  行走时的水面是否出现拖影**需要人工行走观察确认**。静止画面已确认无异常
  （`artifacts/diag/rim-beauty-TemporalAntiAliasing.png`）。
- **未跑 `ExperienceValidation.ValidateRoute()`**：本次只改抗锯齿，未动几何与碰撞，理论上无影响，但未实测。
- **未重新烘焙光照**（几何未变，不需要）。
- **残余翻转**：Play Mode 下仍有 rot `>32` = 65 px、`>64` = 12 px、max 80（原 466/15/96）。
  属可接受残余。若需进一步压低，方向是**从源头减少高频信息**：对程序化瓷砖缝/水波做基于
  `fwidth`/导数的距离带限（`Porcelain.shader` 的勾缝已用 `fwidth`，但水波与 caustics 未做），
  或降低池沿/排水沟的极端明暗对比。
- **§5.1 的共面重叠隐患未修**（当前不闪，见 §5.1 的原因说明）。
- **`PROGRESS.md` 仍未更新**：会话开始前它就已过期（停在 02:47，未记录 10:25 烘焙 / 10:26 路线验证 /
  10:31 构建成功）。本次未替用户改写该文档 —— 按 `AGENTS.md` 要求，它需要记录真实验证结果与限制，
  建议由负责构建验收的一方统一更新。

---

## 10. 给 Codex 的下一步建议

1. **先人工验收** `Builds/Poolrooms/Poolrooms.exe`：确认池沿闪烁是否已可接受、TAA 有无可见拖影。
   在 Game 视图 / exe 里看，**不要在 Scene 视图里判断**（§7-9）。
2. 若观感仍不满意 → 走**源头带限**路线（§9 最后一条），而不是继续调 AA。
   可用 `Tools/diag/playmode-verify.cs` 做回归量化（记得它已内置冻结动画）。
3. 若决定修 §5.1 的共面重叠 → 必须连带**重新烘焙光照**并刷新水面 cubemap，
   然后用 `Tools/diag/zfight-check.cs` 验证"不同材质共面冲突"归零。
4. 更新 `PROGRESS.md`（当前已过期，见 §9）。
