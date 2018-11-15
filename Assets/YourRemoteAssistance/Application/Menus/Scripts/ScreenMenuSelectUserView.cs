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
	 * ScreenMenuSelectUserView
	 * 
	 * Will allow to choose between service provider or a customer
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenMenuSelectUserView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_SELECT_USER";

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

			GameObject btnServiceProvider = m_container.Find("Button_ServiceProvider").gameObject;
			btnServiceProvider.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.user.service.provider");
			btnServiceProvider.GetComponent<Button>().onClick.AddListener(ServiceProviderUser);

			GameObject btnCustomer = m_container.Find("Button_Customer").gameObject;
			btnCustomer.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.user.customer");
			btnCustomer.GetComponent<Button>().onClick.AddListener(CustomerUser);

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
		 * ServiceProviderUser
		 */
		private void ServiceProviderUser()
		{
			SoundsController.Instance.PlaySingleSound(GameConfiguration.SOUND_SELECTION_FX);
			GameConfiguration.SaveUserType(false);
#if !ENABLE_BITCOIN && !ENABLE_ETHEREUM
			MenuScreenController.Instance.CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
			MenuScreenController.Instance.CreateOrJoinRoomInServer(false);
#else
            MenuScreenController.Instance.CreateNewScreen(ScreenSetUpBlockchain.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
#endif
		}

		// -------------------------------------------
		/* 
		 * PlayWithoutSonar
		 */
		private void CustomerUser()
		{
			SoundsController.Instance.PlaySingleSound(SoundsConfiguration.SOUND_SELECTION_FX);
			GameConfiguration.SaveUserType(true);
			MenuScreenController.Instance.CreateNewScreen(ScreenPhoneNumberView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
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