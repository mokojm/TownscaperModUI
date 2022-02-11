﻿using MelonLoader;
using System;
using System.Collections.Generic;
using TMPro;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using IniParser;
using IniParser.Model;
using IniParser.Parser;
using System.IO;

namespace ModUI
{
	public class ModSettings : MonoBehaviour
	{
		public MelonMod parentMod;
		public GameObject subPanel;
		public RectTransform subRect;

		public GameObject backButtonGameObject;
		public Button backButton;
		public FileIniDataParser thisIniParser;
		public string settingsFile;
		public IniData iniData;

		public static Dictionary<String, GameObject> controlButtons = new Dictionary<String, GameObject>();
		public static Dictionary<String, GameObject> controlSliders = new Dictionary<String, GameObject>();
		public static Dictionary<String, GameObject> controlInputFields = new Dictionary<String, GameObject>();
		public static Dictionary<String, GameObject> controlToggle = new Dictionary<String, GameObject>();
		public static Dictionary<String, GameObject> controlKeybind = new Dictionary<String, GameObject>();

		bool isOpen = false;

		public ModSettings(MelonMod thisMod)
		{
			parentMod = thisMod;			
			Setup();
		}

		public void Setup()
		{
			subPanel = UnityEngine.Object.Instantiate(UIManager.uiPrefabs["SubPanel"]);
			subRect = subPanel.GetComponent<RectTransform>();
			subPanel.name = parentMod.Info.Name;

			MelonLogger.Msg("[" + parentMod.Info.Name + "] Initializing settings UI ...");

			backButtonGameObject = UnityEngine.Object.Instantiate(UIManager.uiPrefabs["ButtonBack"]);
			backButtonGameObject.transform.parent = subPanel.transform;

			backButton = backButtonGameObject.GetComponent<Button>();
			backButton.onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(new Action(delegate { this.Toggle(); })));

			subPanel.transform.parent = UIManager.viewport.transform;
			subRect.position = UIManager.sidePanelPositionClosed;
			subPanel.SetActive(false);

			OpenOrCreateSettingsFile();
		}

		public void AddButton(string name, string section, Color32 buttonColor, Action newAction)
		{
			controlButtons.Add(name, UnityEngine.Object.Instantiate(UIManager.uiPrefabs["ButtonSmall"]));
			controlButtons[name].transform.parent = subPanel.transform;

			ButtonSmall newButton = controlButtons[name].AddComponent<ButtonSmall>();
			newButton.Setup(name, section, buttonColor, newAction);
		}

		public void AddSlider(string name, string section, Color32 sliderColor, float minValue, float maxValue, bool wholeNumbers, float defaultValue, Action<float> newAction)
		{
			controlSliders.Add(name, UnityEngine.Object.Instantiate(UIManager.uiPrefabs["Slider"]));
			controlSliders[name].transform.parent = subPanel.transform;

			DZSlider newSlider = controlSliders[name].AddComponent<DZSlider>();
			newSlider.Setup(name, section, sliderColor, minValue, maxValue, wholeNumbers, newAction, this);

			bool valueExists = GetValueFloat(name, section, out float valueResult);

			if (valueExists)
			{
				newSlider.thisSlider.value = valueResult;
			}
			else
			{
				newSlider.thisSlider.value = defaultValue;
				SetValueFloat(name, section, defaultValue);
			}

			newSlider.awoken = true;
		}

		public void AddInputField(string name, string section, Color32 fieldColor, TMP_InputField.ContentType contentType, string defaultValue, Action<string> newAction)
		{
			controlInputFields.Add(name, UnityEngine.Object.Instantiate(UIManager.uiPrefabs["Input"]));
			controlInputFields[name].transform.parent = subPanel.transform;

			InputField newInputField = controlInputFields[name].AddComponent<InputField>();
			newInputField.Setup(name, section, fieldColor, contentType, newAction, this);

			bool valueExists = GetValueString(name, section, out string valueResult);

			if (valueExists)
			{
				newInputField.thisInputField.text = valueResult;
			}
			else
			{
				newInputField.thisInputField.text = defaultValue;
				SetValueString(name, section, defaultValue);
			}
		}

		public void AddToggle(string name, string section, Color32 toggleColor, bool defaultValue, Action<bool> newAction)
		{
			controlToggle.Add(name, UnityEngine.Object.Instantiate(UIManager.uiPrefabs["Toggle"]));
			controlToggle[name].transform.parent = subPanel.transform;

			DZToggle newToggleField = controlToggle[name].AddComponent<DZToggle>();
			newToggleField.Setup(name, section, toggleColor, newAction, this);

			bool valueExists = GetValueBool(name, section, out bool valueResult);

			if (valueExists)
			{
				newToggleField.thisToggle.isOn = valueResult;
			}
			else
			{
				newToggleField.thisToggle.isOn = defaultValue;
				SetValueBool(name, section, defaultValue);
			}
		}

		public void AddKeybind(string name, string section, KeyCode defaultValue, Color32 keybindColor)
		{
			controlKeybind.Add(name, UnityEngine.Object.Instantiate(UIManager.uiPrefabs["ButtonKeybind"]));
			controlKeybind[name].transform.parent = subPanel.transform;

			Keybind newKeybind = controlKeybind[name].AddComponent<Keybind>();
			newKeybind.Setup(name, section, keybindColor, this);

			bool valueExists = GetValueKeyCode(name, section, out KeyCode valueResult);

			if (valueExists)
			{
				newKeybind.thisKeyCode = valueResult;
				newKeybind.contentField.text = valueResult.ToString();
			}
			else
			{
				newKeybind.thisKeyCode = defaultValue;
				newKeybind.contentField.text = defaultValue.ToString();
				SetValueKeyCode(name, section, defaultValue);
			}
		}

		public void OpenOrCreateSettingsFile()
		{		
			thisIniParser = new FileIniDataParser();
			settingsFile = FileSystem.settingsPath + "/" + parentMod.Info.Name + ".ini";

			thisIniParser.Parser.Configuration.AllowCreateSectionsOnFly = true;
			//thisIniParser.Parser.Configuration.AssigmentSpacer = "&&";
			thisIniParser.Parser.Configuration.SkipInvalidLines = true;
			thisIniParser.Parser.Configuration.OverrideDuplicateKeys = true;
			thisIniParser.Parser.Configuration.AllowDuplicateKeys = true;

			if (!File.Exists(settingsFile))
			{
				MelonLogger.Msg("[" + parentMod.Info.Name + "] Creating new settings file ...");
				MelonLogger.Msg("[" + settingsFile + "]");

				iniData = new IniData();
				thisIniParser.WriteFile(settingsFile, iniData);
			}
			else
			{
				MelonLogger.Msg("[" + parentMod.Info.Name + "] Loading settings file ...");
				MelonLogger.Msg("[" + settingsFile + "]");

				iniData = thisIniParser.ReadFile(settingsFile);				
			}
		}

		public bool GetValueString(string name, string section, out string result)
		{
			string tempResult;
			iniData.TryGetKey(section + "|" + name, out tempResult);

			if (tempResult == "")
			{
				result = "";
				MelonLogger.Msg("[" + parentMod.Info.Name + "] String value [" + name + "] not found in section ["+ section +"]");

				return false;
			}
			else
			{
				result = tempResult;
				return true;
			}
		}

		public bool GetValueFloat(string name, string section, out float result)
		{
			string tempResult;
			iniData.TryGetKey(section + "|" + name, out tempResult);

			if (tempResult == "")
			{
				MelonLogger.Msg("[" + parentMod.Info.Name + "] Float value [" + name + "] not found in section [" + section + "]");
				result = 0f;
				return false;
			}
			else
			{
				result = float.Parse(tempResult);
				return true;
			}			
		}

		public bool GetValueBool(string name, string section, out bool result)
		{
			string tempResult;
			iniData.TryGetKey(section + "|" + name, out tempResult);

			if (tempResult == "")
			{
				MelonLogger.Msg("[" + parentMod.Info.Name + "] Bool value [" + name + "] not found in section [" + section + "]");
				result = false;
				return false;
			}
			else
			{
				result = bool.Parse(tempResult);
				return true;
			}
		}

		public bool GetValueKeyCode(string name, string section, out KeyCode result)
		{
			string tempResult;
			iniData.TryGetKey(section + "|" + name, out tempResult);
				
			if (tempResult == "")
			{
				MelonLogger.Msg("[" + parentMod.Info.Name + "] KeyCode value [" + name + "] not found in section [" + section + "]");
				result = KeyCode.None; 
				return false;
			}
			else
			{
				result = ConvertToKeyCode(tempResult);
				return true;
			}
		}

		public KeyCode ConvertToKeyCode(string keyCodeString)
		{
			return (KeyCode)System.Enum.Parse(typeof(KeyCode), keyCodeString);
		}

		public bool GetValueInt(string name, string section, out int result)
		{
			string tempResult;
			iniData.TryGetKey(section + "|" + name, out tempResult);

			if (tempResult == "")
			{
				MelonLogger.Msg("[" + parentMod.Info.Name + "] Int value [" + name + "] not found in section [" + section + "]");
				result = 0;
				return false;
			}
			else
			{
				float floatResult = float.Parse(tempResult);
				int intResult = int.Parse(tempResult);

				result = intResult;
				return true;
			}
		}

		public void SetValueString(string name, string section, string value)
		{
			CheckIfSectionExist(section);

			if (!iniData[section].ContainsKey(name))
			{
				iniData[section].AddKey(name, value);
			}
			else
			{
				iniData[section][name] = value;
			}
		}

		public void SetValueFloat(string name, string section, float value)
		{
			SetValueString(name, section, value.ToString());
		}

		public void SetValueBool(string name, string section, bool value)
		{
			SetValueString(name, section, value.ToString());
		}

		public void SetValueInt(string name, string section, int value)
		{
			SetValueFloat(name, section, value);
		}

		public void SetValueKeyCode(string name, string section, KeyCode value)
		{
			SetValueString(name, section, value.ToString());
		}

		public void SaveToFile()
		{
			MelonLogger.Msg("[" + parentMod.Info.Name + "] Writing settings to file ...");
			MelonLogger.Msg("[" + settingsFile + "]");

			thisIniParser.WriteFile(settingsFile, iniData);
		}


		public void CheckIfSectionExist(string section)
		{
			if (!iniData.Sections.ContainsSection(section))
			{
				MelonLogger.Msg("[" + parentMod.Info.Name + "] Section [" + section + "] not found in settings. Creating ...");
				iniData.Sections.AddSection(section);				
			}			
		}		

		public void Toggle()
		{
			if (!UIAnimation.isAnimatingSub)
			{
				if (!isOpen)
				{
					MelonCoroutines.Start(UIAnimation.PanelFadeIn(subRect));
					isOpen = true;
					UIManager.titleField.text = parentMod.Info.Name;
				}
				else
				{
					MelonCoroutines.Start(UIAnimation.PanelFadeOut(subRect));
					isOpen = false;
					UIManager.titleField.text = "ModUI";
				}
			}
		}
	}
}
