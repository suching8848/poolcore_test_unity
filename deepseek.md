# DeepSeek 改动与诊断记录

> 供 Codex / 其他 agent 对齐用。记录本次会话对 Poolcore 的全部改动、**实测数据**、以及
> **已排除的假设**（避免重复劳动）。所有数字均由 Unity CLI 在真实 Editor 中测得，非推测。

会话日期：2026-09-11
Unity CLI：`C:/Users/15792/AppData/Local/Unity/bin/unity.exe`
连接方式：`unity status` → 端口 7800，项目 `E:\code\test_astra\Poolcore\Poolcore`，Unity 6000.6.0f1

---

## 1. 改动清单

| 文件 | 改动 | 状态 |
|---|---|---|
| `Assets/Poolcore/Scenes/Poolrooms.unity` | `First Person Camera` 上 `UniversalAdditionalCameraData.antialiasing`：`SubpixelMorphologicalAntiAliasing` → `TemporalAntiAliasing`（quality 保持 `High`）。场景已保存。 | **已改** |
| `Assets/Poolcore/Editor/PoolroomsBuilder.cs` | `Create()` 中相机抗锯齿同步改为 TAA（附原因注释），防止重新生成场景时退回 SMAA。 | **已改** |
| `Tools/diag/*.cs` | 新增可复现诊断脚本（`eval_file` 的 body 形式，**不在 Assets 下**，不会被 Unity 编译）。 | **新增** |
| `deepseek.md` | 本文件。 | **新增** |

未改动：`StillWater.shader`、`Porcelain.shader`、任何 `.mat`、`Movement.asset`、`FirstPersonController.cs`、`ExperienceMenu.cs`。
诊断过程**未写入**任何材质/场景文件（通过文件时间戳核对）。

---

## 2. 症状与结论

**症状**（用户描述 + 截图）：水池边缘在**一动视角**时于两种状态间来回闪烁；静止不动时看不到。

**结论**：这是**视角运动混叠（view-motion aliasing）**，不是 z-fighting、不是时间驱动的动画问题、不是镜面高光。
池沿是全场景对比度最高的位置（阳光直射的亮色地面 × 极暗的排水沟 `Deep Green Trim` × 瓷砖缝），
而相机原本只开了 **SMAA**。SMAA 是**纯空间滤波器**，无法消除运动混叠。

**修复**：相机改用 **TAA**。

---

## 3. 修复前后实测

测量方法：把相机摆到池沿近景（`(9.0,1.8,0)` 看向 `(7.0,-0.3,3.5)`，与用户截图机位一致），
用 `Camera.Render()` 渲到 RenderTexture 读回像素，对比**一帧真实视角运动**前后的逐像素最大通道差。
`>16/32/64` 表示跳变幅度超过该值的像素个数（画面 1000×800 = 800,000 px）。

### 出厂设置（SMAA）vs 修复后（TAA）——不覆盖相机设置，直接读场景配置

| 运动 | 修复前 SMAA | 修复后 TAA | 改善 |
|---|---|---|---|
| 旋转 0.02°：`>16` | 1803 | 269 | **6.7×** |
| 旋转 0.02°：`>32` | 481 | 70 | **6.9×** |
| 平移 0.3 mm：`>32` | 258 | 109 | 2.4× |
| 最大跳变 | 96 | 80 | — |

### 抗锯齿模式对照（后处理开启，其余不变）

| AA 模式 | 平移 `>32` | 旋转 `>32` | 旋转 `>64` | max |
|---|---|---|---|---|
| None | 446 | 1045 | **358** | **207** |
| SMAA（原出厂） | 264 | 475 | 14 | 96 |
| **TAA** | **120** | **69** | 14 | **80** |

---

## 4. 已排除的假设（含数据）

| 假设 | 检验方法 | 结果 |
|---|---|---|
| 池沿与地面共面重叠 → z-fighting | 沿世界坐标线逐点采样像素 + 2 mm 位移 | **排除**。重叠带渲成 `Wet Ivory`（R/G≈0.72）而非 `Sea Glass Mosaic`（R/G≈0.47），稳定无跳变。**完全共面时深度序与视角无关**，胜者由绘制顺序固定 |
| 水面折射 / 波纹 | 隐藏全部水面渲染器 | **排除**。`noWater` 与 base 完全相同（394 vs 389 px，max 210 相同）→ 翻转像素不在水面上 |
| 池壁 caustics 欠采样 | 1× vs 2× 超采样下行采样对比，`_Caustics` 开/关 | **排除**。4864 → 4793（无变化）。caustics 波长约 1.9 m，属低频，本就不易混叠 |
| 水面折射混叠 | 同上，`_Ripple` 开/关 | **排除**。4824 → 4825（无变化） |
| 镜面高光混叠 | `_Smoothness` 0.65→0.45→0.25→0.00 扫描 | **排除**。旋转 `>32` = 449 / 431 / 459 / 468，max 恒为 ~96 |
| 阳光阴影贴图抖动 | 关闭方向光阴影 | **反向**。旋转 `>64` 241 → 1156，max 210 → 228（更糟），不是原因 |
| SMAA/MSAA 不足 | None / SMAA / TAA 对照 | SMAA 显著优于 None；但**只有 TAA** 能压运动翻转 |

**开关有效性已单独验证**（否则 A/B 无意义）：关 caustics 影响 22,669 px（max 69）、关 `_Ripple` 影响 10,368 px、隐藏水面影响 123,122 px —— 材质开关确实生效。

**⚠️ 测量陷阱（重要）**：SMAA 属于**后处理**效果。若把 `renderPostProcessing=false` 再对比"开/关 SMAA"，
两者完全相同，会得出"抗锯齿无效"的**错误结论**。本会话一开始就踩了这个坑，已纠正。

---

## 5. 未修复的已知隐患（建议但**未实施**）

### 5.1 三个水池的池沿与地面精确共面重叠

`PoolroomsBuilder.Pool()` 生成的池壁以池边线为**中心**（厚 0.16 m），而池边地面延伸到同一坐标，
因此两者的**顶面精确共面于 y=0，且重叠 0.08 m 宽**。共 **177 处共面重叠对**，其中 40 处**材质不同**，
面积最大的 12 处正是三个水池的池沿，合计 **9.28 m²**：

```
Y-max @ 0.0000  area 1.1200 m2  [Atrium West / Sea Glass Mosaic] <-> [Atrium West / Wet Ivory]
... (Atrium 四边 4×1.12，Quiet 2.40，Warm 2.40)
```

**当前不闪**（见 §4 第一条），但它依赖绘制顺序的稳定性：一旦改动 near/far、换渲染后端或图形 API，
就可能翻转为真实闪烁。**建议修掉，但本次未做。**

建议补丁（`PoolroomsBuilder.Pool()`，把池壁改为纯内侧包边，与地面**边对边**相接而非重叠）：

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

**未实施的原因**：① 可见包边宽度会由 8 cm 变为 16 cm（观感变化，需你确认）；
② 几何变动会使烘焙光照失效，需要重新 `Poolcore/Bake Soft Lighting` 并重跑
`Poolcore/Capture Pool Reflections`。二者都超出"修闪烁"的最小改动范围。

### 5.2 其余不同材质共面重叠（面积小，未处理）

`Connector Roof`×`Warm North Lintel`、`Passage Ceiling`×`Warm West Lintel` 等天花板/过梁交界处，
单处约 0.07 m²，共 28 处。位于天花角落，影响很小。

---

## 6. 诊断工具（`Tools/diag/`）

用 `unity command eval_file --file <绝对路径>` 运行；脚本为 C# **语句体**（非类），会被包进方法执行，`return <string>` 返回结果。

| 脚本 | 作用 |
|---|---|
| `zfight-check.cs` | 通用共面重叠探测器（轴对齐盒体、同侧、面积重叠），输出不同材质冲突排序表 |
| `rim-scan.cs` | 沿世界坐标线逐点采样像素，判断池沿"重叠带"实际由哪种材质胜出，并做微位移翻转判定 |
| `alias-probe.cs` | 1× 与 2× 超采样下行采样对比，量化空间欠采样并输出热力图 |
| `alias-ab.cs` | `_Caustics` / `_Ripple` 开关 A/B（含自动还原） |
| `verify-toggle.cs` | 验证材质开关是否真的影响渲染（A/B 前置检查） |
| `shimmer.cs` | 合成行走路径下的逐像素时间方差图 + 归因 A/B |
| `flip-attr.cs` | 池沿翻转的逐项归因（水面/包边/排水沟/阴影/AA/后处理） |
| `flip-metric.cs` | 翻转像素定位图（含隐藏水面对照） |
| `flip-gloss.cs` | `_Smoothness` 扫描 |
| `flip-taa.cs` | AA 模式对照（含 TAA 时间累积预热） |
| `verify-fix.cs` | 读相机**实际**设置做修复前后 A/B，并输出池沿对照图 |

原始运行产物在 `artifacts/diag/`（该目录已被 `.gitignore` 排除）：
`zfight-before.txt`、`flip-before.txt`、`flip-attr.txt`、`flip-gloss.txt`、`flip-taa.txt`、
`fix-baseline.txt`、`fix-after.txt`、`rim-crop.png`、`rim-beauty-*.png`、`shimmer-base.png` 等。

---

## 7. 环境注意事项（踩坑记录）

1. **`eval` 的 `--code` 内联参数会被 CLI 解析器按空格切碎**，必须用 `eval_file --file <路径>`。
2. **`capture_game_view --save_path` 被 authoring root 限制在 `Assets/` 下**，传绝对路径也会落到
   `Assets/artifacts/…` 并触发导入，污染工程。改用 `eval_file` 自己 `File.WriteAllBytes` 到任意路径。
   若已误建，需删除 `Assets/artifacts/` 与 `Assets/artifacts.meta` 后 `AssetDatabase.Refresh`。
3. **Edit Mode 下 Editor 未聚焦时 `Time` 不推进**（`Time.frameCount` 卡住），
   `set_autotick` 也无效；Play Mode 未聚焦同样不推进。因此**时间相关**（动画/时间累积 TAA）的测试
   必须在一次 `eval_file` 调用内自驱动，或让用户聚焦窗口。
   本会话为让 `edit` 生效曾执行 `set_autotick --enable true --interval_ms 200`（未加 `--persist`，仅本会话有效）。
4. `FindObjectsByType<T>(FindObjectsSortMode.None)` 在 Unity 6000.6 已过时，改用 `FindObjectsByType<T>()`。
5. 在 `eval_file` 里写委托时注意 `System.Func<...>` 的泛型参数个数必须含返回类型；
   `delegate{...}` 赋值语句结尾是 `};` 不是 `});`（本会话犯了两次）。
6. **HTTPS 推送必须绕开 Schannel。** 本机 `http.sslbackend=schannel`
   （`C:/Program Files/Git/etc/gitconfig`）。直接 `git push` 会失败：
   `schannel: AcquireCredentialsHandle failed: SEC_E_NO_CREDENTIALS (0x8009030e)`。
   这不是网络被封（TCP 443 可达，且公共仓库 `git ls-remote` 同样失败），而是 Schannel 取不到凭据句柄。
   改用 Git for Windows 自带的 OpenSSL 后端即可（CA bundle 已存在）：
   ```
   git -c http.sslBackend=openssl push -u origin main
   ```
7. **凭据助手需要具名管道。** 在受限沙箱下推送还会报
   `sh.exe: *** fatal error - couldn't create signal pipe, Win32 error 5`
   与 `could not read Username for 'https://github.com'` —— Git Credential Manager 无法启动。
   需要在放宽沙箱权限（danger-full-access）下执行推送命令。
8. 远端仓库：`https://github.com/suching8848/poolcore_test_unity.git`，
   分支 `main`，首次提交 `94fbbc6`（`git remote -v` 已配置 origin）。

---

## 8. 本次**未**完成 / 未验证（残余风险）

- **未重新打包 Windows player**。`Builds/Poolrooms/Poolrooms.exe` 仍是 TAA 改动**之前**的构建，
  不含本次修复。需要时重跑：
  `unity command build --target StandaloneWindows64 --outputPath Builds/Poolrooms/Poolrooms.exe --confirm true --project-path <root>`
- **未跑 `ExperienceValidation.ValidateRoute()`**：本次只改抗锯齿、不动几何与碰撞，理论上无影响，但未实测。
- **TAA 的副作用未做长时间人工验收**：TAA 可能带来轻微拖影/软化；透明水面无 motion vector，
  行走时的水面是否出现拖影**需要人工行走观察确认**（静止画面已确认无异常，见 `artifacts/diag/rim-beauty-TemporalAntiAliasing.png`）。
- **未重新烘焙光照**（几何未变，不需要）。
- 残余翻转：修复后仍有 70 px（旋转 0.02°）跳变 >32、14 px >64、max 80。属可接受残余，
  如需进一步压低可考虑：降低池沿/排水沟的极端明暗对比，或对程序化瓷砖缝做基于 `fwidth` 的距离带限。
- `PROGRESS.md` **仍未更新**（会话开始前它就已过期：停在 02:47，未记录 10:25 烘焙 / 10:26 路线验证 /
  10:31 构建成功）。本次未替用户改写该文档。
