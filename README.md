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
- このmodは``SSP_Dの機能を使用する``を無効にしている場合、廃村機能などのごく僅かな機能しか無い、機能追加modです。
  - 他modに干渉しない事を目指しています。
    - 完全な共存性を保証できるModは[SuperNewRoles](https://github.com/ykundesu/SuperNewRoles)のみとなっています。
    - 主要Modとの共存性は調査していますが、完全な保証はできません。
    - このModを導入してバグが発生した場合、共存先modのバグだけでなく此方のModの原因も疑ってください。
      - 共存先のModのデバック時、このModの機能が必要ない場合は、抜く事をお勧めします。
      - このModの機能がデバックに必要で共存させる場合は、Host機のみに入れる事を推奨します。
  - ``SSP_Dの機能を使用する``を無効にした場合、``SuperSimplePlus_Deputataの機能``の呼び出しを行わない為、有効な時よりも他modへの干渉が発生しにくくなります。

## 🔵 使用上の注意
- **このmodを単独で導入している場合、バニラサーバーで公開部屋を作成する事ができません。**
  - 併用している場合は、共存先Modの仕様に準じます。
  - "廃村", "投票&開票スキップ", "自動 Kick&BAN" も **『ユーザが意図しないMod体験』** に当たると判断したためです。ご了承ください。

## ChangeLog
- [更新履歴及びAmongUsバージョン対応表](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%9B%B4%E6%96%B0%E5%B1%A5%E6%AD%B4%E5%8F%8A%E3%81%B3AmongUs%E3%83%90%E3%83%BC%E3%82%B8%E3%83%A7%E3%83%B3%E5%AF%BE%E5%BF%9C%E8%A1%A8)
  - [更新履歴](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%9B%B4%E6%96%B0%E5%B1%A5%E6%AD%B4%E5%8F%8A%E3%81%B3AmongUs%E3%83%90%E3%83%BC%E3%82%B8%E3%83%A7%E3%83%B3%E5%AF%BE%E5%BF%9C%E8%A1%A8#%E6%9B%B4%E6%96%B0%E5%B1%A5%E6%AD%B4)(GitHub Wikiに移動)<br><br>
  - [AmongUsバージョン対応表](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%9B%B4%E6%96%B0%E5%B1%A5%E6%AD%B4%E5%8F%8A%E3%81%B3AmongUs%E3%83%90%E3%83%BC%E3%82%B8%E3%83%A7%E3%83%B3%E5%AF%BE%E5%BF%9C%E8%A1%A8#amongus%E3%83%90%E3%83%BC%E3%82%B8%E3%83%A7%E3%83%B3%E5%AF%BE%E5%BF%9C%E8%A1%A8)(GitHub Wikiに移動)<br><br>
  - [旧SSP時代のリリース履歴, バージョン対応, 対応コードの表](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%9B%B4%E6%96%B0%E5%B1%A5%E6%AD%B4%E5%8F%8A%E3%81%B3AmongUs%E3%83%90%E3%83%BC%E3%82%B8%E3%83%A7%E3%83%B3%E5%AF%BE%E5%BF%9C%E8%A1%A8#%E6%97%A7ssp%E6%99%82%E4%BB%A3%E3%81%AE%E3%83%AA%E3%83%AA%E3%83%BC%E3%82%B9%E5%B1%A5%E6%AD%B4-%E3%83%90%E3%83%BC%E3%82%B8%E3%83%A7%E3%83%B3%E5%AF%BE%E5%BF%9C-%E5%AF%BE%E5%BF%9C%E3%82%B3%E3%83%BC%E3%83%89%E3%81%AE%E8%A1%A8)(GitHub Wikiに移動)<br><br>

# .NETバージョンについて
## dllの違いについて
- SuperSimplePlus.dll
  - 共存先のmodの.NETバージョンが``.NET6.0``の時に使用してください。
    - 共存先のmodが v1.8.0.0以降の[SuperNewRoles](https://github.com/ykundesu/SuperNewRoles)の場合は此方になります。
      - 最新版のSNRを使用している場合は此方を使用して下さい。
    - **テストはしていますが、SNR以外のmodとの共存の動作は完全な保証はできません。**
      - 共存先のmodが[ExtremeRoles](https://github.com/yukieiji/ExtremeRoles)の場合はこちらになります。
      - 共存先のmodが[Nebula on the Ship](https://github.com/Dolly1016/Nebula)の場合はこちらになります。

## .NETバージョンの確認の仕方
- 共存先のModの``モッド名.csrroj``ファイルの``<TargetFramework>○○</TargetFramework>``を確認してください。
  - ○○の部分が.NETバージョンになります
- SSPの場合は[ここ](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/blob/main/SuperSimplePlus/SuperSimplePlus.csproj#L3)です。
- <img src="https://user-images.githubusercontent.com/104145991/223727561-71424b18-7f74-484d-bec7-8b1166421b34.png" alt=".NETversionの確認" title=".NETversionの確認" width="500px">

# 製作者
- SuperSimplePlus_Deputata
  - [月城蔵徒](https://github.com/Kurato-Tsukishiro)([Twitter](https://twitter.com/Kurato_SNR7))
- SuperSimplePlus (fork元製作者)
  - ~~[さつまいも](https://github.com/satsumaimoamo) ([Twitter](https://twitter.com/satsumaimo_SNR))~~
- 製作者はいつでも募集しています！Issuesまで！

# クレジット
- [ExtremeRoles](https://github.com/yukieiji/ExtremeRoles) Thanks to **yukieiji**!!
- [SuperNewRoles](https://github.com/ykundesu/SuperNewRoles) Thanks to **ykundesu**!!
- [TheOtherRoles](https://github.com/Eisbison/TheOtherRoles) Thanks to **Eisbison**!!
- [TheOtherRoles GM Edition](https://github.com/yukinogatari/TheOtherRoles-GM) Thanks to **yukinogatari**!!
- [TheOtherRoles-GM-Haoming](https://github.com/haoming37/TheOtherRoles-GM-Haoming) Thanks to **haoming37**!!
- [TownOfHost](https://github.com/tukasa0001/TownOfHost) Thanks to **tukasa0001**!!
- [TownOfPlus](https://github.com/tugaru1975/TownOfPlus) Thanks to **tugaru1975**!!
- [TownOfSuper](https://github.com/reitou-mugicha/TownOfSuper)Thanks to **reitou-mugicha**!!

# 機能一覧

## 設定
### SuperSimplePlusの機能
| 機能               | 説明                                                                              |
| :----------------- | :-------------------------------------------------------------------------------- |
| PC以外をキックする | 有効時 入室した或いは既に入室している steam及びEpic以外のプレイヤーをキックする。 |
| PC以外をバンする   | 有効時 入室した或いは既に入室している steam及びEpic以外のプレイヤーをBANする。    |

### SuperSimplePlus_Deputataの機能
| 機能                  | 説明                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| :-------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| SSP_Dの機能を使用する | **起動時**に有効な場合[^1] [ ``SuperSimplePlus_Deputataの機能`` ] と記載している機能が使用可能になる。                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| 登録者入室時バンする  | 有効時 ``BanFriendCodeList.txt``にフレンドコードが登録されているプレイヤー及びフレンドコード未所持のプレイヤーが入室した場合, BANをする。<br>[機能詳細:登録者入室時バンする](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%A9%9F%E8%83%BD-:-%5B-%E7%99%BB%E9%8C%B2%E8%80%85%E5%85%A5%E5%AE%A4%E6%99%82%E3%83%90%E3%83%B3%E3%81%99%E3%82%8B-%5D)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| ゲームログを作成する  | **起動時**に有効な場合[^1] 以下の機能が有効になる ( [機能詳細:ゲームログを作成する](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%A9%9F%E8%83%BD-:-%5B-%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B-%5D) )<br>・[1. ゲームログの作成 ](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%A9%9F%E8%83%BD-:-%5B-%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B-%5D#1%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0yymmdd_hhmm_amongus_gameloglog%E3%81%AE%E4%BD%9C%E6%88%90)<br>・[2. 自身のチャットのみのlogの作成 ](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%A9%9F%E8%83%BD-:-%5B-%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B-%5D#2%E8%87%AA%E8%BA%AB%E3%81%AE%E3%83%81%E3%83%A3%E3%83%83%E3%83%88%E3%81%AE%E3%81%BF%E3%81%AElogamongus_chatmemolog%E3%81%AE%E4%BD%9C%E6%88%90)<br>・[3. 自視点のみで表示されるチャット ](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%A9%9F%E8%83%BD-:-%5B-%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B-%5D#3%E8%87%AA%E8%A6%96%E7%82%B9%E3%81%AE%E3%81%BF%E3%81%A7%E8%A1%A8%E7%A4%BA%E3%81%95%E3%82%8C%E3%82%8B%E3%83%81%E3%83%A3%E3%83%83%E3%83%88)<br>・[4. 試合単位のゲームログを切り出す ](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%A9%9F%E8%83%BD-:-%5B-%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B-%5D#4%E8%A9%A6%E5%90%88%E5%8D%98%E4%BD%8D%E3%81%AE%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0%E3%82%92%E5%88%87%E3%82%8A%E5%87%BA%E3%81%99) |
| フレンドコード非表示  | 有効時 ゲームログ内などSSPの機能でフレンドコードを表示する物において、伏字で表記する。                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |

[^1]: ゲーム中で変更した設定は反映されない。反映されていない状態の場合、ボタン表示が薄い色になる。<br>(再起動後有効になる場合 : 薄い黄緑色, 再起動後無効になる場合 : 薄い赤色)

<hr>

## その他の機能
### SuperSimplePlusの機能
| 機能             | 説明                                                                                            | 操作                                                       |
| :--------------- | :---------------------------------------------------------------------------------------------- | :--------------------------------------------------------- |
| アップデート機能 | SSPのアップデートがあった時更新ボタンが表示され, それを押すことでアップデートすることができる。 | タイトル画面の右上のアップデートボタンを押してアップデート |

### SuperSimplePlus_Deputataの機能
| 機能                            | 説明                                                                                           | 操作                                                                                                                                                                                                                                                                                                      |
| :------------------------------ | :--------------------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 一人からゲームを開始できる      | 共存先Modが[Nebula on the Ship](https://github.com/Dolly1016/Nebula)の場合機能が動かない       | ロビーで常時発動                                                                                                                                                                                                                                                                                          |
| 対象者がいた場合警告する        | BANListに登録済みのプレイヤーが参加している場合, チャットで警告する。                          | - 自身の入室時 (ゲスト参加)<br> - 対象者入室時 (ゲスト参加時 又は ホストで[登録者入室時バンする](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%A9%9F%E8%83%BD-:-%5B-%E7%99%BB%E9%8C%B2%E8%80%85%E5%85%A5%E5%AE%A4%E6%99%82%E3%83%90%E3%83%B3%E3%81%99%E3%82%8B-%5D)機能無効時。) |
| BANを行ったプレイヤーを記録する | BANを手動で行った時 BanReport.logに 登録日時, 登録時のプレイヤー名, フレンドコードを記載する。 | ホストで手動BANを実行した時                                                                                                                                                                                                                                                                               |

<hr>

## キーボードショートカット
### SuperSimplePlusの機能
#### ホストのみ
| キー                    | 機能                | 使えるとき     |
| ----------------------- | ------------------- | -------------- |
| `左Shift`+`A`+`右Shift` | 廃村                | 試合中いつでも |
| `左Shift`+`C`+`右Shift` | 投票&開票をスキップ | 会議中         |

<hr>

## チャットコマンド
### SuperSimplePlus_Deputataの機能
#### 全員 / [ ゲームログを作成する ] 機能 起動時有効の場合
| コマンド                                                       | 引数                                                                              | 説明                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| :------------------------------------------------------------- | :-------------------------------------------------------------------------------- | :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| ゲームログ関連                                                 |                                                                                   | ロビーで使用可能                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| ``/savegamelog``<br>``/sgl``                                   | 最終ゲームのGameLogを取得する。<br>ファイル名(全角可)<br>未入力でも使用可能       | 記入例： ``/sgl マッドてるてる_位置偽装成功_追報勝利回``<br>[機能詳細 : 試合単位のゲームログを切り出す](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%A9%9F%E8%83%BD-%3A-%5B-%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B-%5D/_edit#4%E8%A9%A6%E5%90%88%E5%8D%98%E4%BD%8D%E3%81%AE%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0%E3%82%92%E5%88%87%E3%82%8A%E5%87%BA%E3%81%99)                                                      |
| ``/savegamelog [引数1], [引数2]``<br>``/sgl [引数1], [引数2]`` | [引数1]回目のゲームのGameLogを取得し、<br>ファイル名に[引数2]を使用して保存する。 | ``/sgl 2, マッドてるてる_位置偽装成功_追報勝利回``<br>(2回目のゲームのGameLogを ファイル名を指定して保存)<br>[機能詳細](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%A9%9F%E8%83%BD-:-%5B-%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B-%5D#%E6%95%B0%E5%AD%97%E3%81%AE%E5%BC%95%E6%95%B0%E3%81%82%E3%82%8A-%E6%8C%87%E5%AE%9A%E8%A9%A6%E5%90%88%E3%81%AE%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0%E3%81%AE%E5%8F%96%E5%BE%97) |
| チャットログ関連                                               |                                                                                   | 常に使用可能                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| ``/memo``<br>``/cm``                                           | 自視点のみで表示されるチャットの内容                                              | ``/cm 黄色 時間的に私のベント移動アドミン見ている 告発しない マッド?``<br>[機能詳細：自視点のみで表示されるチャット](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%A9%9F%E8%83%BD-:-%5B-%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B-%5D#3%E8%87%AA%E8%A6%96%E7%82%B9%E3%81%AE%E3%81%BF%E3%81%A7%E8%A1%A8%E7%A4%BA%E3%81%95%E3%82%8C%E3%82%8B%E3%83%81%E3%83%A3%E3%83%83%E3%83%88)                                                 |
| ``/nowgamecount``<br>``/ngc``                                  | 現在の試合回数を取得する。                                                        | ``/ngc`` => 「 現在のゲームプレイ回数 : {0} 回 」<br>[機能詳細 : 現在の試合回数を取得する。](https://github.com/Kurato-Tsukishiro/SuperSimplePlus_Deputata/wiki/%E6%A9%9F%E8%83%BD-:-%5B-%E3%82%B2%E3%83%BC%E3%83%A0%E3%83%AD%E3%82%B0%E3%82%92%E4%BD%9C%E6%88%90%E3%81%99%E3%82%8B-%5D#%E7%8F%BE%E5%9C%A8%E3%81%AE%E8%A9%A6%E5%90%88%E6%95%B0)                                                                                                                                                           |

#### 全員 / [ 登録者入室時バンする ] 機能 起動時有効の場合
| コマンド                        | 引数 | 説明                                                |
| :------------------------------ | :--- | :-------------------------------------------------- |
| ``/banlistlnquiry``<br>``/bll`` |      | 現在の参加者にBANList対象のユーザがいるか確認する。 |

<hr>

## 実装予定の機能
**追加機能募集中です!**

<hr>
