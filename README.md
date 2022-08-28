# Umamusume-Assets-Extractor
ウマ娘のアセットファイルをダンプします (datフォルダにある英語の羅列のファイルに本来の名前を付けてコピーします)


## 何をする？
ウマ娘のアセットファイルは、そのまま置いてあるわけではなく、英語の羅列のファイルになっています。

例えば、うまぴょい伝説の音楽ファイルの場合、

![image](https://user-images.githubusercontent.com/90076182/186933969-5f3a6ca7-61cc-481d-838f-8528789ee180.png)

sound/l/1001/snd_bgm_live_1001_oke_01.awbですが、実際は

![image](https://user-images.githubusercontent.com/90076182/186935145-6c28ef28-6d16-40c3-8bc2-e32ec7bc99a4.png)

こんな感じに保存されています。

もし音楽ファイルを取り出したいときは、metaというデータベースを見て、取り出したいファイルに対応している英語の羅列のファイルを探して、データベースに書いてある本来の名前にしてと面倒くさいことをしないといけないので、

この工程を全自動でやってくれるアプリを作りました。

こんな感じにしてくれます

![image](https://user-images.githubusercontent.com/90076182/186937978-bc7c62ba-1fc0-4f5a-9aa2-bb5e268610ce.png)

(音楽ファイルのawbファイルを再生する方法を一番下に書いておきました)

## 使い方
動画: https://www.youtube.com/watch?v=OIHdvQwi-ig

## 注意
### ダウンロードされているかについて
ダウンロードされていないファイルはダンプできません。

(例:BLOW my GALEを一回も見たことがない場合はBLOW my GALEの音楽ファイルをダンプできない)

もし自分が見たいファイルが見つからなかった場合はウマ娘で一括ダウンロードしてからダンプしてみてください。

### IDについて
IDを知っておくと自分が欲しいファイルが簡単に見つけられます。

例えば、ライブの音楽ファイルは"sound/l/ライブID/snd_bgm_live_ライブID_oke_01.awb"に保存されています。

BLOW my GALEのライブIDは1048なので、BLOW my GALEの音楽ファイルは"sound/l/1048/snd_bgm_live_1048_oke_01.awb"に保存されているということになります。

もしIDがわからない場合、umamusume-localifyの作者さんが作ったものを改造させてもらったものがあるので、使ってください。

https://github.com/Endergreen12/text_dumper

このアプリは、キャラID、衣装ID、ライブIDそしてそれに対応するキャラやライブの名前が書いてあるテキストファイルを生成してくれます。

## おまけ:awbファイルの再生の仕方
動画: https://youtu.be/IsJBgcroclI

foobar2000のダウンロードリンク:https://www.foobar2000.org/download

vgmstreamのダウンロードリンク:https://vgmstream.org/downloads

ちなみにawbファイルと一緒に置いてあるacbファイルはゲーム側で再生するときは必要ですが、foobar2000で再生するときは必要はありません。
