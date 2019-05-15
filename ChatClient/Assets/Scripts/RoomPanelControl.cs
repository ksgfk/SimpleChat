using Breakdawn.Protocol;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class RoomPanelControl : MonoBehaviour
{
	public InputField chatInput;
	public Text chatText;
	public ScrollRect scrollRect;
	public string username;

	private ServerSession session;

	private void Awake()
	{
		DawnUtil.LogAction = (s, l) =>
		{
			switch (l)
			{
				case LogLevel.Info:
					Debug.Log(s);
					break;
				case LogLevel.Warn:
					Debug.LogWarning(s);
					break;
				case LogLevel.Error:
					Debug.LogError(s);
					break;
				default:
					Debug.LogError(s);
					break;
			}
		};

		var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
		socket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), 25565));
		session = new ServerSession(0, socket);
		session.ReceiveHeadMessage();
	}

	public void OnSendButtonClick()
	{
		if (chatInput.text != string.Empty)
		{
			string addText = "\n  " + "<color=red>" + username + "</color>: " + chatInput.text;
			chatText.text += addText;
			chatInput.text = "";
			chatInput.ActivateInputField();
			Canvas.ForceUpdateCanvases();
			scrollRect.verticalNormalizedPosition = 0f;
			Canvas.ForceUpdateCanvases();
		}
	}

	private void OnDestroy()
	{
		session?.Socket?.Shutdown(SocketShutdown.Both);
		session?.Socket?.Close();
	}
}
