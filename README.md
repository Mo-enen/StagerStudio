# <img src="Resource/Logo64.png"/>   Stager Studio 测试版


> Stager Studio 是一款下落式音游通用谱面编辑器，凭借其 “舞台-轨道-音符” 的逻辑灵活的兼容多款主流音游。



**版本需求**

- Unity Editor 2020.1.10f1c1
- 代码API .NET 4.x



##### 注意事项

- 使用前需要先将压缩包解压

- 下载并使用其他用户制作的皮肤、语言包等内容可以提升使用体验

- 导入、导出其它音游的谱面文件需要使用额外的谱面转换器（例如Stager Map Converter）

- 您的作品将被保存在程序目录下的 Projects 文件夹内

- 建议在 [YouTube](https://www.youtube.com/) 或 [Bilibili](https://www.bilibili.com/) 等网站搜索并观看视频教程

    

![Screenshot](Resource/Screenshot.jpg)



**工程设置**

![Tag Layer v0.1.3](Resource/TagLayerv013.png)



##### 版权声明

- Stager Studio 采用 [Unity](https://unity.com/) 游戏引擎，由**楠瓜Moenen**设计开发并保留著作权；
- 下载后即可使用 Stager Studio 的全部功能；
- 本产品不包含作者本人作品之外的广告内容；
- 用户使用 Stager Studio 创作的谱面归该用户所有；



##### 联系方式

- 您可以通过以下方式联系我：
- Email moenen6@gmail.com | moenenn@163.com
- Twitter [_Moenen](https://twitter.com/_Moenen)
- YouTube [Moenen](https://www.youtube.com/channel/UC1aZDGIux_vlev_xN9Dx2Lg)
- Github [Mo-enen](https://github.com/Mo-enen)
- 作者QQ 1182032752
- 官方QQ群 754100943
- 哔哩哔哩 [楠瓜Moenen](https://space.bilibili.com/11318413)



##### 更新日志 



`v0.1.3`

- 修复了退谱（音符逆向上升）时不能正常显示音符的bug；

- 修复了咕咕咕模板不会自动添加轨道的bug；

    

`v0.1.2`

- 修复了绘制物体时不显示物体的bug；

    

`v0.1.1`

- 主场景边缘透明玻璃效果，超出范围的物体半透明显示；

- 退出皮肤编辑器时可以选择不保存；

- 皮肤、快捷键、语言文件夹位置改为软件根目录下；

- 全景模式下，点击主场景左侧的选择器可以跳转到相应舞台；

- 修改了语言文件格式，新格式一行一个键值对，增强了容错能力；

- 优化了彩蛋的触发方式，并加入了新的彩蛋；

- 选中舞台时，可以用Axis控件直接调整高度；

- 使用舞台笔刷时，可以用快捷键调节高度，默认按键 `-`、`=`；

- 添加了在设置中自定义网格数量的功能；

- 添加了右键菜单来修改工程信息窗口中的调色板、按键音、补间的顺序；

- 在主页左上方添加了设置窗口的入口；

- 略微修改了运动属性窗口的UI；

- 添加了皮肤的“无限判定线”功能，可在皮肤编辑器中开启，开启时判定线会自动延伸至主场景边缘，舞台的尺寸逻辑不变；

- 优化了退谱逻辑（动态下落速度为负数），音符不再有撞线两次的可能；

- 主场景右键拖拽改变音乐时间时，不再考虑全局下落速度，从而更好的适应退谱；

- 新的Axis控件美工，更有整体感；

- 移除了全局笔刷尺寸功能；

- **紧贴功能**：新增了调整物体时吸附周围物体的功能；

- 新的设置选项，设置拖拽控件调整物体尺寸、角度时的紧贴数量；

- 在谱面的属性面板中添加了下落速度按钮；

- 主页新建工程、章节的菜单改为两个单独的按钮；

- 新模板：三重舞台（节奏水龙头）；

- **基因系统**：基因会在创建工程选择模板时自动应用到工程，工程创建后不能修改基因。基因系统会根据工程的基因禁用部分功能，例如禁止修改音符的位置，或禁止修改轨道的宽度等。这将减少无关功能对创作的干扰，从而提升特定谱面的创作体验；

- 在属性面板中添加了颜色选择器，点击颜色标签或双击颜色输入框即可弹出颜色选择器；

- 可在设置中选择4款默认壁纸，可在设置窗口通用栏目中选择显示使用哪款壁纸（或随机）。软件刚开启和工程没有壁纸时会显示默认壁纸；

- 添加了时间组概念，为时间点和音符分组，只有与时间点分组相同的音符才会被其影响下落速度；

- 主页回收站改为显示在章节的最下方；

- 进度条只允许左键拖拽；

- 笔刷属性面板，选择笔刷时显示当前笔刷的宽度、种类等属性。取消了设置窗口中的笔刷栏目；

    

`v0.1.0`

- 此版本发布于 2020-6-4，仅官方Q群内公开，仅包含PC-64位版本；

- 重新制作了全部内容；

- 程序目录下的 Projects 文件夹中包含多个子文件夹，每个子文件夹代表一个章节。章节文件夹名代表章节的名称，章节文件夹内可以保存多个工程文件，每个工程文件代表一个工程。章节文件夹不能拥有子文件夹。

- 工程文件的拓展名为 `stager`，采用了新的数据结构，不兼容之前的版本；

- 工程文件中包含以下信息：
  - 音乐（音频×1）
  - 背景图（图片×1）
  - 封面图（图片×1）
  - 谱面（多个 `json` 文件）
  - 按键音（多个音频）
  - 调色板（多个颜色）
  - 补间曲线（多个曲线）
  - 工程文本信息（标题、简介、谱面作者、音乐作者、背景图作者）
  - 隐藏信息（上次编辑的时间，正在打开的谱面，数据版本编号，魔法编号STAGER）
  
- 支持的音乐文件格式：`ogg`、`mp3`、`wav`，建议使用 `ogg` 格式；

- 支持的图片文件格式：`png`、`jpg`；

- 增加了舞台概念，进一步完善了“舞台-轨道-音符”的逻辑，从而灵活的兼容了更多游戏规则。使用舞台笔刷可以绘制舞台（默认快捷键是`数字键1`）；

- 不再使用旧版的时间轴（Timeline），现在可以在主场景内直接编辑舞台、轨道的运动；

- 不再使用旧版的变速功能，变速信息改为由时间点（Timing）控制。时间点是一种特殊的音符，显示在主场景左侧边缘，可以改变谱面在特定时间上的全局下落速度。使用时间点笔刷可以绘制时间点（默认快捷键是`数字键4`）；

- 内置皮肤编辑器，可以编辑音符、轨道，判定线等物体的外观并保存为皮肤文件，拓展名为 `stagerskin` 。主场景左侧的皮肤按钮可以快速切换、编辑当前皮肤。设置窗口的皮肤专栏可以选择当前皮肤，专栏下方的按钮可以一键打开皮肤文件夹；

- 语言包文件为可阅读的 `txt` 文件，复制文件并替换相应内容即可制作新的语言文件，你可以在设置窗口的语言专栏选择当前语言，专栏下方的按钮可以一键打开语言文件夹；

- 目前的版本内置了快捷键修改功能，你可以在设置窗口的快捷键专栏查看或设置快捷键；

    

`v0.0.4`

- 轨道加入了 Rotation 属性，可以让轨道倾斜，单位是角度；

- 按住 Ctrl 点击 Timeline 里的轨道，可以快速添加一个属性点；

- 拖拽舞台框选音符或轨道；

- 被选中的音符、轨道在代表时间的轴向上超出屏幕时，系统会自动取消选择；

- 删除了 Inspector 里轨道的 Duration 输入框，轨道的长度由属性列表的最后一个点决定；

- 允许修改轨道动画列表的第一个点和最后一个点的时间；

- 修复了Library 不能自定义快捷键的问题；

- 修复了不能选择同名音乐、图片文件的问题；

- 可以将 Inspector 里轨道的属性列表的时间改为全局时间，在 Window -> Setting -> Game 里通过 Use Global Time in Track Inspector 修改；

- 打开工程后自动删除多余的 ProjectMapData；

- 添加了 File -> Save Project 菜单；

- 删除轨道时同时删除轨道里的音符；

- 集成了 Unity Analysis;

- 增加了指令功能，可以使用类似 AllNotes Time +0.6 等语句来快速调整指定物体的属性。

- 修复了新增加的动画属性点再修改属性后会同时修改其它点的属性的问题。
