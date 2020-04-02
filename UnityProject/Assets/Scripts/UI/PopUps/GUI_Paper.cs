using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class GUI_Paper : NetTab
{
	public TMP_InputField textField;
	public ContentSizeFitter contentSizeFitter;

	public override void OnEnable()
	{
		base.OnEnable();
		StartCoroutine(WaitForProvider());
		textField.interactable = false;
	}

	IEnumerator WaitForProvider()
	{
		while (Provider == null)
		{
			yield return WaitFor.EndOfFrame;
		}
		RefreshText();
	}

	public override void RefreshTab()
	{
		RefreshText();
		base.RefreshTab();
	}

	public void RefreshText()
	{
		if (Provider != null)
		{
			textField.text = Provider.GetComponent<Paper>().PaperString;
		}
	}

	public void ClosePaper()
	{
		ControlTabs.CloseTab(Type, Provider);
	}

	public void OnTextFieldClick()
	{
		Debug.Log("OnTextFieldClick");
		if (IsPenInHand())
		{
			EnableEditing();
		}
		else
		{
			textField.interactable = false;
			DisableEditing();
		}
	}

	private bool IsPenInHand()
	{
		return UIManager.Hands.CurrentSlot.Item?.GetComponent<Pen>() != null
			|| UIManager.Hands.OtherSlot.Item?.GetComponent<Pen>() != null;
	}

	private void EnableEditing()
	{
		textField.interactable = true;
		textField.ActivateInputField();
		UIManager.IsInputFocus = true;
		UIManager.PreventChatInput = true;
		CheckForInput();
	}

	private void DisableEditing()
	{
		UIManager.IsInputFocus = false;
		UIManager.PreventChatInput = false;
	}

	//Safety measure:
	private async void CheckForInput()
	{
		Debug.Log("CheckForInput entered");
		await Task.Delay(500);
		Debug.Log("CheckForInput running");
		if (!textField.isFocused)
		{
			DisableEditing();
		}
	}

	//Request an edit from server:
	public void OnTextEditEnd()
	{
		Debug.Log("OnTextEditEnd");
		PlayerManager.LocalPlayerScript.playerNetworkActions.CmdRequestPaperEdit(Provider.gameObject, textField.text);
		DisableEditing();
	}

	public void OnTextValueChange()
	{
		Debug.Log("OnTextValueChange: rendered height " + textField.textComponent.renderedHeight
			+ " is truncated " + textField.textComponent.isTextTruncated);
		//Only way to refresh it to get it to do its job (unity bug):
		/*
		contentSizeFitter.enabled = false;
		contentSizeFitter.enabled = true;
		*/
		CheckLineLimit();
	}

	private void CheckLineLimit()
	{
		if (textField.placeholder.enabled || !textField.textComponent.isTextTruncated)
		{
			return;
		}

		// also guard for when the text isn't truncated.

		// iterate through the line info
		TMP_LineInfo[] lineInfos = textField.textComponent.textInfo.lineInfo;
		for (int i = 0; i < lineInfos.Length; i++)
		{
			var info = lineInfos[i];
			if (info.lineHeight < 20f)
			{
				Debug.Log("line " + i + " has height" + info.lineHeight);
			}
		}

		/*
		Canvas.ForceUpdateCanvases();
		if (textField.textComponent.cachedTextGenerator.lineCount > 20)
		{
			var sub = textField.text.Substring(0, textField.text.Length - 1);
			textField.text = sub;
		}
		*/
	}
}
