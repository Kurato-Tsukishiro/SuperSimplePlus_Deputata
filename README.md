# SuperSimplePlus_Deputata
<img src="https://raw.githubusercontent.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/88505517de349ec9ad5e692e054cf39a0728374a/SuperSimplePlus/Resources/SSP_Deputata_Long.png" alt="SuperSimplePlus_DeputataLogo" height="300px">

# 免責事項
- 原文
  - This mod is not affiliated with Among Us or Innersloth LLC, and the content contained therein is not endorsed or otherwise sponsored by Innersloth LLC.
  - Portions of the materials contained herein are property of Innersloth LLC.
  - © Innersloth LLC.<br><br>
- 訳
  - SuperSimplePlus_DeputataはAmong UsやInnersloth LLCとは無関係であり、含まれるコンテンツはInnersloth LLCによって承認されたり、直接提供を受けているものではありません。
  - 此処に含まれる素材の一部はInnersloth LLCの所有物です。
  - © Innersloth LLC.

# このmodについて
## 目的
- このmodは廃村機能などのごく僅かな機能しか無い、機能追加modです。
- 他modに干渉しない事を目指しています。
  - 完全な共存性を保証できるModは[SuperNewRoles](https://github.com/ykundesu/SuperNewRoles)のみとなっています。
  - 主要Modとの共存性は調査していますが、完全な保証はできません。
  - このModを導入してバグが発生した場合、共存先modのバグだけでなく此方のModの原因も疑ってください。
    - 共存先のModのデバック時、このModの機能が必要ない場合は、抜く事をお勧めします。
    - このModの機能がデバックに必要で共存させる場合は、Host機のみに入れる事を推奨します。

# .NETバージョンについて

## 等Modの.NETバージョンについて
- .Net6を使用しております。
## .NETバージョンの確認の仕方
- 共存先のModの``モッド名.csrroj``ファイルの``<TargetFramework>○○</TargetFramework>``を確認してください。
  - ○○の部分が.NETバージョンになります
- SSPの場合は[ここ](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/blob/main/SuperSimplePlus/SuperSimplePlus.csproj#L3)です。
- <img src="https://user-images.githubusercontent.com/104145991/223727561-71424b18-7f74-484d-bec7-8b1166421b34.png" alt=".NETversionの確認" title=".NETversionの確認" width="500px">

[![](https://img.shields.io/discord/996781291871678544?label=Discord)](https://discord.gg/rsaU2zntey)

# 製作者
- SuperSimplePlus_Deputata
  - [月城蔵徒](https://github.com/Kurato-Tsukishiro)([Twitter](https://twitter.com/Kurato_SNR7))
- SuperSimplePlus (fork元製作者)
  - ~~[さつまいも](https://github.com/satsumaimoamo) ([Twitter](https://twitter.com/satsumaimo_SNR))~~
- 製作者はいつでも募集しています！Issuesまで！

# 実装予定の機能
**追加機能募集中です!**

## クレジット
- [ExtremeRoles](https://github.com/yukieiji/ExtremeRoles) Thanks to **yukieiji**!!
- [SuperNewRoles](https://github.com/ykundesu/SuperNewRoles) Thanks to **ykundesu**!!
- [TheOtherRoles](https://github.com/Eisbison/TheOtherRoles) Thanks to **Eisbison**!!
- [TheOtherRoles GM Edition](https://github.com/yukinogatari/TheOtherRoles-GM) Thanks to **yukinogatari**!!
- [TheOtherRoles-GM-Haoming](https://github.com/haoming37/TheOtherRoles-GM-Haoming) Thanks to **haoming37**!!
- [TownOfHost](https://github.com/tukasa0001/TownOfHost) Thanks to **tukasa0001**!!
- [TownOfPlus](https://github.com/tugaru1975/TownOfPlus) Thanks to **tugaru1975**!!
- [TownOfSuper](https://github.com/reitou-mugicha/TownOfSuper)Thanks to **reitou-mugicha**!!

## 機能一覧
#### ホストのみ
##### キーボードショートカット
| キー                    | 機能           | 使えるとき     |
| ----------------------- | -------------- | -------------- |
| `左Shift`+`A`+`右Shift` | 廃村           | 試合中いつでも |
| `左Shift`+`C`+`右Shift` | 投票&開票をスキップ | 会議中         |
##### 設定
|機能                |
| -- |
|PC以外をキックする|
|PC以外をバンする|
#### その他機能
| 機能             | 操作                                                       |
| ---------------- | ---------------------------------------------------------- |
| アップデート機能 | タイトル画面の右上のアップデートボタンを押してアップデート |
| 一人からゲームを開始できる<br>(共存先Modが[Nebula on the Ship](https://github.com/Dolly1016/Nebula)の場合機能が動いていない) | ロビーで常時発動 |
