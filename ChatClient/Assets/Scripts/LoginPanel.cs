using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
	public InputField ip;
	public InputField port;
	public InputField nickName;

	public RoomPanelControl roomPanel;

	public void OnLoginButtonClick()
	{
		var _ip = ip.text;
		var _port = port.text;
		var _nickName = nickName.text;
		IPAddress mip = null;
		int mport = 0;
		try
		{
			mip = IPAddress.Parse(_ip);
			mport = int.Parse(_port);
		}
		catch (System.Exception e)
		{
			Debug.LogWarning(e.Message);
			return;
		}
		if (_nickName == string.Empty)
		{
			Debug.LogWarning($"昵称不能为空");
			return;
		}
		var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		try
		{
			socket.Connect(new IPEndPoint(mip, mport));
		}
		catch (System.Exception e)
		{
			Debug.LogWarning(e.Message);
			return;
		}
		roomPanel.gameObject.SetActive(true);
		roomPanel.username = _nickName;
		roomPanel.session = new ServerSession(0, socket);
		roomPanel.session.ReceiveHeadMessage();
		gameObject.SetActive(false);
	}
}
