using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

public class LocalPreference{
	private const string filePath = "save/";
	private const string fileName = "save.dat";
	private const string Key = "12345";
	private const string IV = "67890131";

	private static LocalPreference Instance = null;
	private Dictionary<string, string> KeyValue;

	private LocalPreference(){ }
	/// <summary>
	/// シングルトンなので、コレで初期化。
	/// </summary>
	/// <returns></returns>
	public static LocalPreference getInstance()
	{
		if (Instance == null)
		{
			Instance = new LocalPreference();
			Instance.load();
		}
		return Instance;
	}
	private byte[] encrypt(byte[] str)
	{
		byte[] src = str;
		RijndaelManaged aes = new RijndaelManaged();
		byte[] dest;
		byte[] _key = System.Text.Encoding.Unicode.GetBytes(Key);
		byte[] _iv = System.Text.Encoding.Unicode.GetBytes(IV);

		byte[] key = new byte[16];
		byte[] iv = new byte[16];
		for (int i = 0; i < 16; ++i)
		{
			key[i] = 0x16;
			iv[i] = 0x15;
			try
			{
				key[i] = _key[i];
			}
			catch
			{
			}
			try
			{
				iv[i] = _iv[i];
			}
			catch
			{
			}
		}
		aes.Padding = PaddingMode.PKCS7;
		using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
		using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write))
		{
			cs.Write(src, 0, src.Length);
			cs.FlushFinalBlock();
			dest = ms.ToArray();
		}
		return dest;
	}
	private byte[] decrypt(byte[] str)
	{
		byte[] src = str;
		RijndaelManaged aes = new RijndaelManaged();
		byte[] dest;
		byte[] _key = System.Text.Encoding.Unicode.GetBytes(Key);
		byte[] _iv = System.Text.Encoding.Unicode.GetBytes(IV);

		byte[] key = new byte[16];
		byte[] iv = new byte[16];
		for (int i = 0; i < 16; ++i)
		{
			key[i] = 0x16;
			iv[i] = 0x15;
			try
			{
				key[i] = _key[i];
			}
			catch
			{
			}
			try
			{
				iv[i] = _iv[i];
			}
			catch
			{
			}
		}

		aes.Padding = PaddingMode.PKCS7;

		using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
		using (CryptoStream cs = new CryptoStream(ms, aes.CreateDecryptor(key, iv), CryptoStreamMode.Write))
		{
			cs.Write(src, 0, src.Length);
			cs.FlushFinalBlock();
			dest = ms.ToArray();
		}
		return dest;
	}
	private List<byte[]> trimDataAndGetHash(byte[] input)
	{
		List<byte[]> list = new List<byte[]>();
		byte[] hash = new byte[32];
		for (int i = 0; i < 32; ++i)
		{
			hash[i] = input[i];
		}
		int num = 0;
		num = input.Length;
		byte[] data = new byte[num - 32];

		System.Array.Copy(input, 32, data, 0, num - 32);

		list.Add(hash);
		list.Add(data);
		return list;
	}
	private void load()
	{
		KeyValue = new Dictionary<string, string>();
		if (!Directory.Exists(filePath))
		{
			Directory.CreateDirectory(filePath);
		}
		if (File.Exists(filePath + fileName) == false)
		{
			return;
		}
		try
		{
			byte[] byteStream = File.ReadAllBytes(filePath + fileName);

			List<byte[]> datas = trimDataAndGetHash(byteStream);

			byte[] mainstream = decrypt(datas[1]);
			bool isok = true;

			byte[] hash = SHAHash(mainstream);
			string mainstring = System.Text.Encoding.Unicode.GetString(mainstream);

			for (int i = 0; i < 32; ++i)
			{
				if (hash[i] != datas[0][i])
				{
					isok = false;
					break;
				}
			}
			if (isok == false)
			{
				return;
			}

			var lines = mainstring.Split('\n');
			foreach (var line in lines)
			{
				var obj = line.Split(',');
				if (obj.Length == 2)
				{
					KeyValue.Add(obj[0], obj[1]);
				}

			}
		}
		catch (CryptographicException e)
		{
			Debug.Log(e.Message);
		}
		catch (FileNotFoundException e)
		{
			Debug.Log(e.Message);
		}
	}

	byte[] SHAHash(byte[] input)
	{
		SHA256 sha = new SHA256Managed();
		return sha.ComputeHash(input);
	}
	/// <summary>
	/// 現在のキーバリューをファイルに書き出します。
	/// </summary>
	public void save()
	{
		if (KeyValue.Count < 0)
			return;
		string data = "";
		foreach (var pair in KeyValue)
		{
			data += pair.Key + "," + pair.Value + '\n';
		}
		byte[] stream = encrypt(System.Text.Encoding.Unicode.GetBytes(data));
		byte[] hash = SHAHash(System.Text.Encoding.Unicode.GetBytes(data));
		using (FileStream fs = new FileStream(filePath + fileName, FileMode.Create, FileAccess.ReadWrite))
		using (BinaryWriter bw = new BinaryWriter(fs))
		{
			bw.Write(hash);
			bw.Write(stream);
		}
	}
	public void setString(string key, string val)
	{
		KeyValue[key] = val;
	}

	/// <summary>
	/// キーに対応したデータを取得します。
	/// もし無かった場合はdefを返却します。
	/// </summary>
	/// <param name="key"></param>
	/// <param name="def"></param>
	/// <returns></returns>
	public string getString(string key, string def)
	{
		string val;
		if (KeyValue.TryGetValue(key, out val) == false)
			val = def;
		return val;
	}
	public bool removeString(string key)
	{
		return KeyValue.Remove(key);
	}
	public bool hasKey(string key)
	{
		return KeyValue.ContainsKey(key);
	}
	/// <summary>
	/// 全てのキーとバリューを削除します。
	/// </summary>
	public void deleteAll()
	{
		KeyValue.Clear();
	}
}
