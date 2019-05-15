using UnityEngine;
using UnityEngine.UI;

public class RoomPanelControl : MonoBehaviour
{
	public InputField chatInput;
	public Text chatText;
	public ScrollRect scrollRect;
	public string username;

	
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
}
