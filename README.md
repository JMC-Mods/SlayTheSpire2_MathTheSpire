**🌐[ 中文 | [English](README_en.md) ]**

[📝更新日志](CHANGELOG.md)

[📦 Releases](https://github.com/JMC2002/SlayTheSpire2_MathTheSpire/releases)

# 伤害总数显示
##  0. 安装

### Mod本体安装
Steam版本直接在创意工坊订阅即可（暂未开放）

其他版本可以自行编译，或者在[📦 Releases](https://github.com/JMC2002/SlayTheSpire2_MathTheSpire/releases)界面下载.zip后解压到游戏安装目录下的Mods
目录下（没有就新建一个）

### 前置安装
**此外，本模组强依赖于模组[JmcModLib](https://github.com/JMC2002/JmcModLib_STS2/releases)**，安装方法同上

安装完成后的目录结构如下：

```sh
-- Slay the Spire 2
    |-- SlayTheSpire2.exe
        |-- mods
             |-- JmcModLib
             |-- MathTheSpire
                  |-- MathTheSpire.dll
                  |-- MathTheSpire.pck
                  |-- MathTheSpire.json
```

### 存档迁移
> 当你第一次安装MOD，游戏会默认将开启Mod的存档与没开启的隔离，可以按下面的方法迁移存档：

在安装好MOD后第一次打开游戏会询问是否启用MOD，启用并再次打开游戏一次后，退出游戏，将`%appdata%\SlayTheSpire2\steam\`下面的数字文件夹下的你对应的存档文件粘贴到该文件夹的`modded`文件夹中，以同步使用MOD前后的存档

---
## 🧠 1. 简介
在头顶显示当前回合即将受到伤害的总数以及未被格挡的伤害，绿色代表完全格挡，蓝色代表部分格挡，红色代表伤害溢出；以后可能更一些别的功能，有问题反馈B站JMC丶或者Github，二次分发需注明来源

[演示视频（B站）](https://www.bilibili.com/video/BV1ZCAxzfEuv/)

[Github仓库](https://github.com/JMC2002/SlayTheSpire2_MathTheSpire)
## ⚙️ 2. 功能
- 显示当前回合即将受到的总伤害和未被格挡的伤害
- 支持在游戏内“设置 -> 模组设置”中调整伤害数字字号、位置与颜色
- 致命伤害颜色：未被格挡的伤害会导致玩家死亡时使用
- 受伤警告颜色：护甲不足但不会致死时使用
- 安全格挡颜色：护甲足以完全格挡时使用
 
## 🔔 3. 提醒
- **本模组强依赖于模组[JmcModLib](https://github.com/JMC2002/JmcModLib_STS2/releases)**
- 显示配置由 JmcModLib 提供，修改后会保存到本地配置文件，并在下一次伤害数字刷新时生效。
 
## 🧩 4. 兼容性
- 由于游戏处于EA阶段，可能会随着游戏版本更新而失效

## 🧭 5. TODO
- 后续可能考虑再加一些别的计算

**如果你喜欢这个 Mod 的话，希望可以点一个star~**

如果你真的很有钱，可以考虑给我赞助，给我赞助你得不到任何东西，但是可以吓我一跳。

![图片描述](pic/wechat_qrcode.png)
