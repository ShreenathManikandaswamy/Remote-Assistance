using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YourCommonTools;
using YourNetworkingTools;

namespace YourRemoteAssistance
{

	/******************************************
	* 
	* ScreenPhoneNumberView
	* 
	* The customer should specify the phone number to call
	* 
	* @author Esteban Gallardo
	*/
	public class ScreenPhoneNumberView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_PHONE_NUMBER";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;

		// -------------------------------------------
		/* 
		* Constructor
		*/
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			m_container.Find("Title").GetComponent<Text>().text = LanguageController.Instance.GetText("message.game.title");

			m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.write.the.phone.number");
			m_container.Find("PhoneNumber").GetComponent<InputField>().text = "";

			GameObject createGame = m_container.Find("Button_SetUpPhone").gameObject;
			createGame.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.set.up.phone.number");
			createGame.GetComponent<Button>().onClick.AddListener(ConfirmPhoneNumber);

			UIEventController.Instance.UIEvent += new UIEventHandler(OnMenuEvent);			
		}

		// -------------------------------------------
		/* 
		* GetGameObject
		*/
		public GameObject GetGameObject()
		{
			return this.gameObject;
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;
			UIEventController.Instance.UIEvent -= OnMenuEvent;
			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		* ConfirmPhoneNumber
		*/
		private void ConfirmPhoneNumber()
		{
			SoundsController.Instance.PlaySingleSound(GameConfiguration.SOUND_SELECTION_FX);
			string phoneNumber = m_container.Find("PhoneNumber").GetComponent<InputField>().text;
			GameConfiguration.SavePhoneNumber(phoneNumber);
			if (phoneNumber.Length < 5)
			{
				MenuScreenController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.error"), LanguageController.Instance.GetText("screen.phone.number.error"), null, "");
			}
			else
			{
#if !ENABLE_BITCOIN && !ENABLE_ETHEREUM
			MenuScreenController.Instance.CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
			MenuScreenController.Instance.CreateOrJoinRoomInServer(false);
#else
            MenuScreenController.Instance.CreateNewScreen(ScreenSetUpBlockchain.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
#endif
			}
		}

		// -------------------------------------------
		/* 
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
		}
	}
}