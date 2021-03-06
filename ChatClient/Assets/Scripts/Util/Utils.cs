﻿using Breakdawn.Protocol;
using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class Utils
{
	public enum CopyLocation
	{
		Head,
		Tail
	}

	public static byte[] AddHeadProtocol(byte[] message)
	{
		var length = message.Length;
		byte[] head = new byte[4];
		BitConverter.GetBytes(length + 4).CopyTo(head, 0);
		var result = AddMessage(head, message, CopyLocation.Head);
		return result;
	}

	public static byte[] AddMessage(byte[] src1, byte[] src2, CopyLocation local)
	{
		var src1Long = src1.Length;
		var src2Long = src2.Length;
		var dst = new byte[src1Long + src2Long];
		switch (local)
		{
			case CopyLocation.Head:
				Buffer.BlockCopy(src1, 0, dst, 0, src1Long);
				Buffer.BlockCopy(src2, 0, dst, src1Long, src2Long);
				break;
			case CopyLocation.Tail:
				Buffer.BlockCopy(src2, 0, dst, 0, src2Long);
				Buffer.BlockCopy(src1, 0, dst, src2Long, src1Long);
				break;
			default:
				throw new ArgumentException("这怎么可能...", nameof(local));
		}
		return dst;
	}

	public static byte[] Serialize<T>(T pack) where T : class
	{
		using (var ms = new MemoryStream())
		{
			try
			{
				var bf = new BinaryFormatter();
				bf.Serialize(ms, pack);
				ms.Seek(0, SeekOrigin.Begin);
				return ms.ToArray();
			}
			catch (Exception)
			{
				throw;
			}
		}
	}

	public static T DeSerialize<T>(byte[] bs) where T : class
	{
		using (var ms = new MemoryStream(bs))
		{
			try
			{
				var bf = new BinaryFormatter();
				var pkg = bf.Deserialize(ms) as T;
				return pkg;
			}
			catch (Exception)
			{
				throw;
			}
		}
	}

	public static byte[] AddCommand(Command command, byte[] message = null)
	{
		byte[] res;
		var cmd = (int)command;
		if (message != null)
		{
			res = AddMessage(BitConverter.GetBytes(cmd), Serialize(message), CopyLocation.Head);
		}
		else
		{
			var t = BitConverter.GetBytes(cmd);
			res = BitConverter.GetBytes(cmd);
		}
		return res;
	}

	public static void SendMessage(Socket s, byte[] message)
	{
		try
		{
			var ns = new NetworkStream(s);
			ns.BeginWrite(message, 0, message.Length, new AsyncCallback(OnSendFinish), ns);
		}
		catch (Exception e)
		{
			Debug.LogError($"{e.Message}\n{e.StackTrace}");
		}
	}

	private static void OnSendFinish(IAsyncResult result)
	{
		try
		{
			var ns = result.AsyncState as NetworkStream;
			ns.EndWrite(result);
			ns.Flush();
			ns.Close();
		}
		catch (Exception e)
		{
			Debug.LogError($"{e.Message}\n{e.StackTrace}");
		}
	}
}
