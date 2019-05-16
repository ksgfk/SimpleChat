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

	public ServerSession session;

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
	}

	public void OnSendButtonClick()
	{
		if (chatInput.text != string.Empty)
		{
			/*
			string addText = "\n  " + "<color=red>" + username + "</color>: " + chatInput.text;
			chatText.text += addText;
			chatInput.text = "";
			chatInput.ActivateInputField();
			Canvas.ForceUpdateCanvases();
			scrollRect.verticalNormalizedPosition = 0f;
			Canvas.ForceUpdateCanvases();
			*/
			var msg = new DawnMessage
			{
				cmd = Command.ChatMessage,
				nickName = username,
				charMessage = chatInput.text,
			};
			var body = Utils.Serialize(msg);
			var pack = Utils.AddHeadProtocol(body);
			Utils.SendMessage(session.Socket, pack);
			chatInput.text = "";
			chatInput.ActivateInputField();
		}
	}

	private void OnDestroy()
	{
		session?.Socket?.Close();
	}
}
