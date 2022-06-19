using MelonLoader;
using System;
using TMPro;
using UnhollowerBaseLib.Attributes;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ModUI
{
	public class SelectionButton : MonoBehaviour
	{
		public Button leftArrow;
		public Button rightArrow;
		public Image buttonImage;
		public TextMeshProUGUI textField;
		public RectTransform thisRect;
		public Action leftAction;
		public Action rightAction;
		public string thisSection;

		public SelectionButton(IntPtr intPtr) : base(intPtr) { }

		[HideFromIl2Cpp]
		public void Setup(string label, string section, Color32 color, Action newLeftAction, Action newRightAction)
		{
			leftAction = newLeftAction;
			rightAction = newRightAction;
			thisSection = section;
			ManualAwake();

			textField.text = label;
			buttonImage.color = color;
		}

		[HideFromIl2Cpp]
		public void Setup(string label, Color32 color, Action newLeftAction, Action newRightAction)
		{			
			Setup(label, label, color, newLeftAction, newRightAction);
		}

		[HideFromIl2Cpp]
		public void Setup(string label, Action newLeftAction, Action newRightAction)
		{
			Color32 randomColor = new Color32((byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), (byte)UnityEngine.Random.Range(0, 255), 255);
			Setup(label, randomColor, newLeftAction, newRightAction);
		}

		public void ManualAwake()
		{
			leftArrow = this.gameObject.transform.Find("Left").GetComponent<Button>();
			rightArrow = this.gameObject.transform.Find("Right").GetComponent<Button>();
			textField = GetComponentInChildren<TextMeshProUGUI>();
			buttonImage = GetComponentInChildren<Image>();
			thisRect = transform.FindChild("Background").GetComponent<RectTransform>();

			leftArrow.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(leftAction));
			rightArrow.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(rightAction));
			leftArrow.onClick
		}
	}
}
