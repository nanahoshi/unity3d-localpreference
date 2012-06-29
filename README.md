unity3d-localpreference
=======================

Unity3DでのPreferenceを指定したファイルに書き込む
(PC向けでは標準レジストリに書き込むのがいやだった)

使い方
------
Assets\LocalPreference.csをUnityに投げ込む。

LocalPreferenceを修正

###コード###
	public class LocalPreference{
		private const string filePath = "save/"; // 書き出し場所
		private const string fileName = "save.dat"; // 書き出しファイル名
		private const string Key = "12345"; // 暗号化の鍵
		private const string IV = "67890131"; // 暗号化のベクトル


###コード###
	//初期化
	LocalPreference pref = LocalPreference.getInstance();
	pref.getString("apple", null);
	pref.setString("apple","tea");
	//書き出し
	pref.save();

注意点
------
最初はデータをxmlにでも書き出して暗号化やろうと思ったけど、
そこまで必要性を感じなかったので、適当にやってます。

なので、多分文字列に[,]や制御文字が入ると上手く動作しないと思います。
文字列しか作ってないので、文字列以外の物は上手に変換してね！