using RTMPStreaming;
using UnityEngine;
using UnityEngine.SceneManagement;
using YourBitcoinController;
using YourCommonTools;
using YourEthereumController;
using YourNetworkingTools;

namespace YourRemoteAssistance
{

	/******************************************
	 * 
	 * ScreenNetworkingController
	 * 
	 * ScreenManager controller that handles all the screens's creation and disposal
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenNetworkingController : ScreenController
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string SUB_EVENT_RECONNECT_CLIENT = "SUB_EVENT_RECONNECT_CLIENT";

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static ScreenNetworkingController _instance;

		public static ScreenNetworkingController Instance
		{
			get
			{
				if (!_instance)
				{
					_instance = GameObject.FindObjectOfType(typeof(ScreenNetworkingController)) as ScreenNetworkingController;
				}
				return _instance;
			}
		}

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	

		[Tooltip("The name of the screen that should run when the server starts")]
		public string NamePrefabServerScreen;

		[Tooltip("The name of the screen that should run when the client starts")]
		public string NamePrefabClientScreen;

		[Tooltip("The name of the screen used in the disconnection")]
		public string NamePrefabConnectionScene;

		[Tooltip("The name of the screen used in the disconnection")]
		public string NamePrefabDisconnectedScene;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private bool m_isCustomer;

		// -------------------------------------------
		/* 
		 * Initialitzation listener
		 */
		public override void Start()
		{
			base.Start();

			m_isCustomer = GameConfiguration.IsCustomer();

			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
			UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
			
			CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true);
		}

		// -------------------------------------------
		/* 
		 * Release resources
		 */
		public override void Destroy()
		{
			base.Destroy();

			if (_instance != null)
			{
				UIEventController.Instance.UIEvent -= OnUIEvent;
				NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
				Destroy(_instance);
				_instance = null;
			}
		}
		
		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		protected void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_CONFIRMATION_KICKED_OUT_PLAYER)
			{
				SceneManager.LoadScene(NamePrefabDisconnectedScene);
			}
			if (_nameEvent == NetworkEventController.EVENT_SYSTEM_INITIALITZATION_LOCAL_COMPLETED)
			{
				UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_ALL_SCREEN);
				ScreenNetworkingController.Instance.CreateNewScreen(ScreenAssistanceView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, true, m_isCustomer);
				if (m_isCustomer)
				{
					NetworkEventController.Instance.DelayNetworkEvent(ScreenAssistanceView.EVENT_MAINMENU_PHONE_NUMBER, 0.8f, YourNetworkTools.Instance.GetUniversalNetworkID().ToString(), GameConfiguration.LoadPhoneNumber());
					
#if ENABLE_STREAMING
					RTMPController.Instance.Initialitzation(GameConfiguration.URL_RTMP_SERVER_ASSISTANCE + GameConfiguration.LoadPhoneNumber(), 640, 480, 15, 1200 * 1024);
#endif
				}
				else
				{
					NetworkEventController.Instance.DelayNetworkEvent(ScreenAssistanceView.EVENT_MAINMENU_IS_MANAGER, 0.8f, YourNetworkTools.Instance.GetUniversalNetworkID().ToString());
				}
#if ENABLE_BITCOIN
                NetworkEventController.Instance.DelayNetworkEvent(ScreenAssistanceView.EVENT_MAINMENU_PUBLIC_BITCOIN_ADDRESS, 1f, YourNetworkTools.Instance.GetUniversalNetworkID().ToString(), BitCoinController.Instance.CurrentPublicKey);
#elif ENABLE_ETHEREUM
                NetworkEventController.Instance.DelayNetworkEvent(ScreenAssistanceView.EVENT_MAINMENU_PUBLIC_BITCOIN_ADDRESS, 1f, YourNetworkTools.Instance.GetUniversalNetworkID().ToString(), EthereumController.Instance.CurrentPublicKey);
#endif

            }
            if (_nameEvent == ScreenAssistanceView.EVENT_MAINMENU_REQUEST_PHONE_NUMBERS)
			{
				if (m_isCustomer)
				{
					NetworkEventController.Instance.DispatchNetworkEvent(ScreenAssistanceView.EVENT_MAINMENU_PHONE_NUMBER, YourNetworkTools.Instance.GetUniversalNetworkID().ToString(), GameConfiguration.LoadPhoneNumber());
#if ENABLE_BITCOIN
                NetworkEventController.Instance.DelayNetworkEvent(ScreenAssistanceView.EVENT_MAINMENU_PUBLIC_BITCOIN_ADDRESS, 0.1f, YourNetworkTools.Instance.GetUniversalNetworkID().ToString(), BitCoinController.Instance.CurrentPublicKey);
#elif ENABLE_ETHEREUM
                NetworkEventController.Instance.DelayNetworkEvent(ScreenAssistanceView.EVENT_MAINMENU_PUBLIC_BITCOIN_ADDRESS, 0.1f, YourNetworkTools.Instance.GetUniversalNetworkID().ToString(), EthereumController.Instance.CurrentPublicKey);
#endif
				}
			}
		}

		// -------------------------------------------
		/* 
		 * OnUIEvent
		 */
		protected override void OnUIEvent(string _nameEvent, params object[] _list)
		{
			base.OnUIEvent(_nameEvent, _list);

			if (_nameEvent == ScreenController.EVENT_CONFIRMATION_POPUP)
			{
				GameObject screen = (GameObject)_list[0];
				bool accepted = (bool)_list[1];
				string subnameEvent = (string)_list[2];
				if (subnameEvent == SUB_EVENT_RECONNECT_CLIENT)
				{
					if (accepted)
					{
						SceneManager.LoadScene(NamePrefabConnectionScene, LoadSceneMode.Single);
					}
				}
			}
		}
	}
}