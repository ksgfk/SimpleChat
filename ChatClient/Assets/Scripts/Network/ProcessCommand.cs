using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.UI;

public class ProcessCommand : MonoBehaviour
{
	public Text chatText;
	public ScrollRect scrollRect;

	private ConcurrentQueue<string> chatQueue = new ConcurrentQueue<string>();

	public ConcurrentQueue<string> ChatQueue { get => chatQueue; }

	public static ProcessCommand Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
	}

	private void Update()
    {
		if (!chatQueue.TryDequeue(out var msg))
		{
			return;
		}
		chatText.text += $"{msg}\n";
		Canvas.ForceUpdateCanvases();
		scrollRect.verticalNormalizedPosition = 0f;
		Canvas.ForceUpdateCanvases();
	}
}
