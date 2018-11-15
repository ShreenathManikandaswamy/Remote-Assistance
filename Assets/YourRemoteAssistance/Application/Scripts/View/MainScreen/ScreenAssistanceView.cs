using System;
using System.Collections.Generic;
using RTMPStreaming;
using UnityEngine;
using UnityEngine.UI;
using YourBitcoinController;
using YourBitcoinManager;
using YourCommonTools;
using YourEthereumController;
using YourEthereumManager;
using YourNetworkingTools;

namespace YourRemoteAssistance
{

	/******************************************
	 * 
	 * ScreenAssistanceView
	 * 
	 * Main screen to manage the multiple clients connected
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenAssistanceView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_ASSISTANCE";

		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_MAINMENU_PHONE_NUMBER				= "EVENT_MAINMENU_PHONE_NUMBER";
		public const string EVENT_MAINMENU_PUBLIC_BITCOIN_ADDRESS	= "EVENT_MAINMENU_PUBLIC_BITCOIN_ADDRESS";
		public const string EVENT_MAINMENU_IS_MANAGER				= "EVENT_MAINMENU_IS_MANAGER";
		public const string EVENT_MAINMENU_CHECK_VALID_ORIGIN		= "EVENT_MAINMENU_CHECK_VALID_ORIGIN";
		public const string EVENT_MAINMENU_CHECK_AGAIN				= "EVENT_MAINMENU_CHECK_AGAIN";
		public const string EVENT_MAINMENU_REQUEST_PHONE_NUMBERS	= "EVENT_MAINMENU_REQUEST_PHONE_NUMBERS";

		public const string SUBEVENT_ACTION_CALL_PHONE_NUMBER		= "SUBEVENT_ACTION_CALL_PHONE_NUMBER";

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------
		public GameObject ClientItemPrefab;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private Transform m_container;
		private Button m_clearLog;
		private Button m_payBitcoin;

		// CLIENTS
		private GameObject m_gridClients;
		private Scrollbar m_scrollbarClients;
		private ScrollRect m_scrollViewClients;
		private List<GameObject> m_clients = new List<GameObject>();

		// ACTIONS
		private Dropdown m_colorsNetwork;
		private string m_colorSelected;
		private ItemClientView m_playerSelected;

		// STREAMING WINDOW
		private RawImage m_streamingWindow;
		private Vector3 m_initialPosition;
#if ENABLE_STREAMING
	private UniversalMediaPlayer m_ump;
#endif

		// CAMERA 
		private GameObject m_cameraLocal;

		// CUSTOMER MEMBES
		private bool m_isCustomer = false;
		private List<string> m_messagesOnScreen = new List<string>();
		private float m_timeOutToClean = 0;
		private NetworkVector3 m_forwardCamera;
		private bool m_rotatedTo90 = false;
		private float m_timeToUpdateForward = 0;

		private enum RotationAxes { None = 0, MouseXAndY = 1, MouseX = 2, MouseY = 3, Controller = 4 }
		private RotationAxes m_axes = RotationAxes.MouseXAndY;
		private float m_sensitivityX = 7F;
		private float m_sensitivityY = 7F;
		private float m_minimumY = -60F;
		private float m_maximumY = 60F;
		private float m_rotationY = 0F;

		private string m_publicKeyAddressProvider;
		private string m_publicKeyAddressCustomer;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			m_isCustomer = (bool)_list[0];

			m_cameraLocal = GameObject.FindGameObjectWithTag("MainCamera").gameObject;

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			m_cameraLocal.GetComponent<Camera>().transform.forward = Vector3.right;
#endif

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");
			Utilities.ApplyMaterialOnObjects(m_container.gameObject, ScreenNetworkingController.Instance.MaterialDrawOnTop);

			m_root.GetComponent<Canvas>().worldCamera = m_cameraLocal.GetComponent<Camera>();

			// LIST OF PLAYERS
			m_gridClients = m_container.Find("CanvasClients/ScrollViewClients/Grid").gameObject;
			m_scrollViewClients = m_container.Find("CanvasClients/ScrollViewClients").gameObject.GetComponent<ScrollRect>();
			m_scrollbarClients = m_container.Find("CanvasClients/ScrollbarVerticalClients").gameObject.GetComponent<Scrollbar>();

			// LIST OF ACTIONS
			m_colorsNetwork = m_container.Find("CanvasColors/DropdownColors").GetComponent<Dropdown>();
			m_colorsNetwork.onValueChanged.AddListener(OnActionSelectedChanged);
			m_colorsNetwork.options = new List<Dropdown.OptionData>();
			m_colorsNetwork.options.Add(new Dropdown.OptionData(DrawingLinesController.COLOR_WHITE));
			m_colorsNetwork.options.Add(new Dropdown.OptionData(DrawingLinesController.COLOR_BLACK));
			m_colorsNetwork.options.Add(new Dropdown.OptionData(DrawingLinesController.COLOR_RED));
			m_colorsNetwork.options.Add(new Dropdown.OptionData(DrawingLinesController.COLOR_BLUE));
			m_colorsNetwork.options.Add(new Dropdown.OptionData(DrawingLinesController.COLOR_GREEN));
			m_colorsNetwork.itemText.text = DrawingLinesController.COLOR_RED;
			m_colorSelected = DrawingLinesController.COLOR_RED;
			DrawingLinesController.Instance.ColorSelected = m_colorSelected;
			m_colorsNetwork.value = -1;
			m_colorsNetwork.value = 0;

			// LOG
			if (m_container.Find("ClearColors") != null)
			{
				m_clearLog = m_container.Find("ClearColors").GetComponent<Button>();
				m_clearLog.gameObject.GetComponent<Button>().onClick.AddListener(ClearColors);
			}

			// LOG
			if (m_container.Find("PayBlockchain") != null)
			{
				m_payBitcoin = m_container.Find("PayBlockchain").GetComponent<Button>();
                m_container.Find("PayBlockchain/IconBitcoin").gameObject.SetActive(false);
                m_container.Find("PayBlockchain/IconEthereum").gameObject.SetActive(false);

                m_payBitcoin.gameObject.GetComponent<Button>().onClick.AddListener(PayBitcoin);
#if !ENABLE_BITCOIN && !ENABLE_ETHEREUM
				m_payBitcoin.gameObject.SetActive(false);
#elif ENABLE_BITCOIN
                m_container.Find("PayBlockchain/IconBitcoin").gameObject.SetActive(true);
#elif ENABLE_ETHEREUM
                m_container.Find("PayBlockchain/IconEthereum").gameObject.SetActive(true);
#endif
            }

            // GET STREAMING PANEL
            m_streamingWindow = m_root.transform.Find("StreamingWindow").GetComponent<RawImage>();
			m_initialPosition = Utilities.Clone(m_streamingWindow.gameObject.transform.position);
			m_streamingWindow.color = new Color(1, 1, 1, 0);

			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
			UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);

#if ENABLE_BITCOIN
			BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBitcoinEvent);
#elif ENABLE_ETHEREUM
            EthereumEventController.Instance.EthereumEvent += new EthereumEventHandler(OnEthereumEvent);
#endif

            if (m_isCustomer)
			{
				m_container.Find("CanvasClients").gameObject.SetActive(false);

#if UNITY_EDITOR
				// m_container.gameObject.SetActive(false);
#endif
			}
			else
			{
				UIEventController.Instance.DelayUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_REQUEST_LIST_USERS, 1);
			}

			UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMAINCOMMANDCENTER_ACTIVATION_VISUAL_INTERFACE);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;

			if (m_isCustomer)
			{
				RTMPController.Instance.StopStream();
			}
			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
			UIEventController.Instance.UIEvent -= OnUIEvent;

#if ENABLE_BITCOIN
            BitcoinEventController.Instance.BitcoinEvent -= OnBitcoinEvent;
#elif ENABLE_ETHEREUM
            EthereumEventController.Instance.EthereumEvent -= OnEthereumEvent;
#endif

            DisposeClientsList(true);
			GameObject.Destroy(this.gameObject);
			return false;
		}

		// -------------------------------------------
		/* 
		 * Clear the current list of clients
		 */
		private void DisposeClientsList(bool _destroy)
		{
			if (m_clients == null) return;

			for (int i = 0; i < m_clients.Count; i++)
			{
				if (m_clients[i] != null)
				{
					if (m_clients[i].GetComponent<ItemClientView>() != null)
					{
						if (_destroy)
						{
							m_clients[i].GetComponent<ItemClientView>().Destroy();
						}
						else
						{
							m_clients[i].GetComponent<ItemClientView>().ReleaseMemory();
						}
					}
					GameObject.Destroy(m_clients[i]);
				}
			}

			m_clients.Clear();
		}

		// -------------------------------------------
		/* 
		 * Check if the connection id is in the list of displayed clients
		 */
		private bool CheckDisplayedClientsID(int _connectionID)
		{
			for (int i = 0; i < m_clients.Count; i++)
			{
				if (m_clients[i] != null)
				{
					if (m_clients[i].GetComponent<ItemClientView>().ConnectionData.Id == _connectionID)
					{
						return true;
					}
				}
			}
			return false;
		}

		// -------------------------------------------
		/* 
		 * We create all the visual instances of the items
		 */
		private void FillClientList(List<PlayerConnectionData> _listClients)
		{
			for (int i = 0; i < _listClients.Count; i++)
			{
				PlayerConnectionData client = _listClients[i];
				if (client != null)
				{
					if (!CheckDisplayedClientsID(client.Id))
					{
						GameObject instance = Utilities.AddChild(m_gridClients.transform, ClientItemPrefab);
						Utilities.ApplyMaterialOnObjects(instance, ScreenNetworkingController.Instance.MaterialDrawOnTop);
						instance.GetComponent<ItemClientView>().Initialize(m_gridClients, _listClients[i].Reference, _listClients[i]);
						m_clients.Add(instance);
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * CancelPressed
		 */
		private void CancelPressed()
		{
			Destroy();
		}

		// -------------------------------------------
		/* 
		 * OnActionSelectedChanged
		 */
		private void OnActionSelectedChanged(int _index)
		{
			m_colorSelected = m_colorsNetwork.options[_index].text;
			DrawingLinesController.Instance.ColorSelected = m_colorSelected;
			DrawingLinesController.Instance.IsPressed = false;
		}

		// -------------------------------------------
		/* 
		 * Clear all the network objects drawn by the administrator
		 */
		private void ClearColors()
		{
			DrawingLinesController.Instance.DestroyMyLines(YourNetworkTools.Instance.GetUniversalNetworkID());
		}

		// -------------------------------------------
		/* 
		 * Opens the payment screen with bitcoin
		 */
		private void PayBitcoin()
		{
#if ENABLE_BITCOIN
			if (m_isCustomer)
			{
				ScreenBitcoinController.Instance.InitializeBitcoin(ScreenBitcoinSendView.SCREEN_NAME, m_publicKeyAddressProvider);
			}
			else
			{
				ScreenBitcoinController.Instance.InitializeBitcoin(ScreenBitcoinSendView.SCREEN_NAME, m_publicKeyAddressCustomer);				
			}
#elif ENABLE_ETHEREUM
            if (m_isCustomer)
            {
                ScreenEthereumController.Instance.InitializeEthereum(ScreenEthereumSendView.SCREEN_NAME, m_publicKeyAddressProvider);
            }
            else
            {
                ScreenEthereumController.Instance.InitializeEthereum(ScreenEthereumSendView.SCREEN_NAME, m_publicKeyAddressCustomer);
            }
#endif
        }

        // -------------------------------------------
        /* 
		 * CheckForwardVectorRemote
		 */
        private void CheckForwardVectorRemote()
		{
			for (int i = 0; i < m_clients.Count; i++)
			{
				if (m_clients[i] != null)
				{
					ItemClientView itemClientView = m_clients[i].GetComponent<ItemClientView>();
					if (itemClientView != null)
					{
						if (itemClientView.Forward == null)
						{
							for (int j = 0; j < NetworkVariablesController.Instance.NetworkVariables.Count; j++)
							{
								INetworkVariable variable = NetworkVariablesController.Instance.NetworkVariables[j];
								if (itemClientView.ConnectionData.Id == variable.Owner)
								{
									if (variable is NetworkVector3)
									{
										itemClientView.Forward = (NetworkVector3)variable;
										NetworkEventController.Instance.DelayNetworkEvent(EVENT_MAINMENU_REQUEST_PHONE_NUMBERS, 0.2f);
									}
								}
							}							
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * We will apply the movement to the camera		
		 */
		private void PlayUMP(string _phoneNumber)
		{
#if ENABLE_STREAMING
			if (m_ump == null)
			{
				m_ump = GameObject.FindObjectOfType<UniversalMediaPlayer>();
				m_ump.RenderingObjects = new GameObject[1];
			}
			else
			{
				if (m_ump.IsPlaying)
				{
					m_ump.Stop();
				}
			}
			if (m_ump != null)
			{
				m_ump.RenderingObjects[0] = m_streamingWindow.gameObject;
				m_ump.Path = GameConfiguration.URL_RTMP_SERVER_ASSISTANCE + _phoneNumber;
				m_ump.Prepare();
				m_ump.AddPreparedEvent(OnPreparedVideo);
			}
#endif
		}

		// -------------------------------------------
		/* 
		 * The video is prepared and ready to play
		 */
		private void OnPreparedVideo(Texture2D _event)
		{
#if ENABLE_STREAMING
			m_streamingWindow.color = new Color(1, 1, 1, 1);
			m_ump.Play();
#endif
		}

		// -------------------------------------------
		/* 
		 * We will apply the movement to the camera		
		 */
		private void MoveCameraWithMouse()
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				m_axes = RotationAxes.None;

				if ((Input.GetAxis("Mouse X") != 0) || (Input.GetAxis("Mouse Y") != 0))
				{
					m_axes = RotationAxes.MouseXAndY;
				}

				// USE MOUSE TO ROTATE VIEW
				if ((m_axes != RotationAxes.Controller) && (m_axes != RotationAxes.None))
				{
					if (m_axes == RotationAxes.MouseXAndY)
					{
						float rotationX = m_cameraLocal.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * m_sensitivityX;

						m_rotationY += Input.GetAxis("Mouse Y") * m_sensitivityY;
						m_rotationY = Mathf.Clamp(m_rotationY, m_minimumY, m_maximumY);

						m_cameraLocal.transform.localEulerAngles = new Vector3(-m_rotationY, rotationX, 0);
					}
					else if (m_axes == RotationAxes.MouseX)
					{
						m_cameraLocal.transform.Rotate(0, Input.GetAxis("Mouse X") * m_sensitivityX, 0);
					}
					else
					{
						m_rotationY += Input.GetAxis("Mouse Y") * m_sensitivityY;
						m_rotationY = Mathf.Clamp(m_rotationY, m_minimumY, m_maximumY);

						m_cameraLocal.transform.localEulerAngles = new Vector3(-m_rotationY, transform.localEulerAngles.y, 0);
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * We rotate with the gyroscope
		 */
		private void GyroModifyCamera()
		{
			// Rotate the parent object by 90 degrees around the x axis
			if (!m_rotatedTo90)
			{
				m_rotatedTo90 = true;
				Input.gyro.enabled = true;
				m_cameraLocal.transform.parent.transform.Rotate(Vector3.right, 90);
			}

			// Invert the z and w of the gyro attitude
			Quaternion rotFix = new Quaternion(Input.gyro.attitude.x, Input.gyro.attitude.y, -Input.gyro.attitude.z, -Input.gyro.attitude.w);

			// Now set the local rotation of the child camera object
			m_cameraLocal.transform.localRotation = rotFix;
		}

		// -------------------------------------------
		/* 
		 * We will update the forward camera vector
		 */
		private void UpdateForwardVector()
		{
			if ((YourNetworkTools.Instance.GetUniversalNetworkID() != -1) && (m_cameraLocal != null))
			{
				if (m_forwardCamera == null)
				{
					m_forwardCamera = new NetworkVector3();
					m_forwardCamera.InitRemote(YourNetworkTools.Instance.GetUniversalNetworkID(), "Vector" + YourNetworkTools.Instance.GetUniversalNetworkID(), m_cameraLocal.GetComponent<Camera>().transform.forward);
					DrawingLinesController.Instance.ForwardPlayer = m_forwardCamera;
				}
				else
				{
					m_timeToUpdateForward += Time.deltaTime;
					if (m_timeToUpdateForward > 0.3f)
					{
						m_timeToUpdateForward = 0;
						m_forwardCamera.SetValue(m_cameraLocal.GetComponent<Camera>().transform.forward);
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * OnNetworkEvent
		 */
		private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == EVENT_MAINMENU_PHONE_NUMBER)
			{
				if (!m_isCustomer)
				{
					int universalID = int.Parse((string)_list[0]);
					string phoneNumber = (string)_list[1];
					for (int i = 0; i < m_clients.Count; i++)
					{
						if (m_clients[i].GetComponent<ItemClientView>().ConnectionData.Id == universalID)
						{
							m_clients[i].GetComponent<ItemClientView>().PhoneNumber = phoneNumber;
							m_clients[i].SetActive(true);
						}
					}
				}
				else
				{
					PlayUMP(GameConfiguration.LoadPhoneNumber());
				}
			}
			if (_nameEvent == EVENT_MAINMENU_PUBLIC_BITCOIN_ADDRESS)
			{
				int universalID = int.Parse((string)_list[0]);
				if (YourNetworkTools.Instance.GetUniversalNetworkID() != universalID)
				{
					if (!m_isCustomer)
					{
						string publicBlockchainAddress = (string)_list[1];
						for (int i = 0; i < m_clients.Count; i++)
						{
							if (m_clients[i].GetComponent<ItemClientView>().ConnectionData.Id == universalID)
							{
								m_clients[i].GetComponent<ItemClientView>().PublicBlockchainAddress = publicBlockchainAddress;
							}
						}
					}
					else
					{
						m_publicKeyAddressProvider = (string)_list[1];
					}
				}
			}
			if (_nameEvent == NetworkEventController.EVENT_PLAYERCONNECTIONDATA_USER_DISCONNECTED)
			{
#if (!UNITY_WSA && !UNITY_EDITOR) || UNITY_EDITOR
				if (m_playerSelected != null)
				{
					if (m_playerSelected.ConnectionData.Id == (int)_list[0])
					{
						Debug.Log("DISCONNECTED PLAYER, STOP STREAMING");
					}
				}
#endif
			}
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_TRANSACTION_COMPLETED)
			{
				string transactionID = (string)_list[0];
				string publicKeyTarget = (string)_list[1];
				decimal amountTransaction = decimal.Parse((string)_list[2]);
				float balanceInCurrency = (float)(amountTransaction * BitCoinController.Instance.GetCurrentExchange());
				string amountInCurrency = balanceInCurrency + " " + BitCoinController.Instance.CurrentCurrency;
				if (BitCoinController.Instance.CurrentPublicKey == publicKeyTarget)
				{
					string description = LanguageController.Instance.GetText("screen.bitcoin.transaction.in.favor.received", amountInCurrency, transactionID);
					ScreenNetworkingController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), description, null, "");
				}
			}
            if (_nameEvent == EthereumController.EVENT_ETHEREUMCONTROLLER_TRANSACTION_COMPLETED)
            {
                string transactionID = (string)_list[0];
                string publicKeyTarget = (string)_list[1];
                decimal amountTransaction = decimal.Parse((string)_list[2]);
                float balanceInCurrency = (float)(amountTransaction * EthereumController.Instance.GetCurrentExchange());
                string amountInCurrency = balanceInCurrency + " " + EthereumController.Instance.CurrentCurrency;
                if (EthereumController.Instance.CurrentPublicKey == publicKeyTarget)
                {
                    string description = LanguageController.Instance.GetText("screen.bitcoin.transaction.in.favor.received", amountInCurrency, transactionID);
                    ScreenNetworkingController.Instance.CreateNewInformationScreen(ScreenInformationView.SCREEN_INFORMATION, UIScreenTypePreviousAction.KEEP_CURRENT_SCREEN, LanguageController.Instance.GetText("message.info"), description, null, "");
                }
            }
        }

		// -------------------------------------------
		/* 
		 * OnUIEvent
		 */
		private void OnUIEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_MAINMENU_CHECK_VALID_ORIGIN)
			{
				int xPos = (int)_list[0];
				int yPos = (int)_list[1];
				bool isValid = !m_container.GetComponent<RectTransform>().rect.Contains(new Vector2(xPos, yPos));
				UIEventController.Instance.DispatchUIEvent(DrawingLinesController.EVENT_DRAWINGLINES_VALID_POINT, isValid);
			}

			if (m_isCustomer) return;

			if (_nameEvent == UIEventController.EVENT_SCREENMAINCOMMANDCENTER_LIST_USERS)
			{
				m_playerSelected = null;
				FillClientList((List<PlayerConnectionData>)_list[0]);
			}
			if (_nameEvent == UIEventController.EVENT_SCREENMAINCOMMANDCENTER_TEXTURE_REMOTE_STREAMING_DATA)
			{
				int idPlayer = (int)_list[0];
				if (m_playerSelected.ConnectionData.Id == idPlayer)
				{
					int textWidth = (int)_list[1];
					int textHeight = (int)_list[2];
					byte[] textureData = (byte[])_list[3];
					ImageUtils.LoadBytesImage(m_streamingWindow, textWidth, textHeight, textureData);
				}
			}
			if (_nameEvent == BaseItemView.EVENT_ITEM_SELECTED)
			{
				GameObject itemSelected = (GameObject)_list[0];
				if (itemSelected == null) return;

				// CLIENTS
				if (itemSelected.GetComponent<IBasicItemView>().ContainerParent == m_gridClients)
				{
					if (itemSelected.GetComponent<ItemClientView>() != null)
					{
						bool selected = (bool)_list[1];
						int indexClicked = -1;
						for (int i = 0; i < m_clients.Count; i++)
						{
							if (m_clients[i].gameObject == itemSelected)
							{
								indexClicked = i;
							}
							else
							{
								if (selected)
								{
									m_clients[i].GetComponent<BaseItemView>().Selected = false;
								}
							}
						}

						if (indexClicked != -1)
						{
							if (selected)
							{
								m_playerSelected = m_clients[indexClicked].GetComponent<ItemClientView>();
								if (m_playerSelected.Forward != null)
								{
									DrawingLinesController.Instance.ForwardPlayer = m_playerSelected.Forward;
									PlayUMP(m_playerSelected.PhoneNumber);
									m_publicKeyAddressCustomer = m_playerSelected.PublicBlockchainAddress;
									ScreenNetworkingController.Instance.CreatePopUpScreenConfirmation(LanguageController.Instance.GetText("text.call.me"), LanguageController.Instance.GetText("text.do.you.want.to.call") + " " + m_playerSelected.PhoneNumber, SUBEVENT_ACTION_CALL_PHONE_NUMBER);
								}
							}
							else
							{
								m_playerSelected = null;
#if ENABLE_STREAMING
							if (m_ump != null) m_ump.Stop();
#endif
								DrawingLinesController.Instance.ForwardPlayer = null;
							}
						}
					}
				}
			}
			if (_nameEvent == ScreenController.EVENT_CONFIRMATION_POPUP)
			{
				GameObject screen = (GameObject)_list[0];
				bool accepted = (bool)_list[1];
				string subnameEvent = (string)_list[2];
				if (subnameEvent == SUBEVENT_ACTION_CALL_PHONE_NUMBER)
				{
					if (m_playerSelected != null)
					{
						if (accepted)
						{
							Application.OpenURL("tel://" + m_playerSelected.PhoneNumber);
						}
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * OnBitcoinEvent
		 */
		private void OnBitcoinEvent(string _nameEvent, object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_TRANSACTION_COMPLETED)
			{
				if ((bool)_list[0])
				{
					string transactionID = (string)_list[1];
					string publicKeyTarget = (string)_list[2];
					string amountTransaction = (string)_list[3];
					NetworkEventController.Instance.DispatchNetworkEvent(BitCoinController.EVENT_BITCOINCONTROLLER_TRANSACTION_COMPLETED, transactionID, publicKeyTarget, amountTransaction);
				}
			}
		}

        // -------------------------------------------
        /* 
		 * OnEthereumEvent
		 */
        private void OnEthereumEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == EthereumController.EVENT_ETHEREUMCONTROLLER_TRANSACTION_COMPLETED)
            {
                if ((bool)_list[0])
                {
                    string transactionID = (string)_list[1];
                    string publicKeyTarget = (string)_list[2];
                    string amountTransaction = (string)_list[3];
                    NetworkEventController.Instance.DispatchNetworkEvent(EthereumController.EVENT_ETHEREUMCONTROLLER_TRANSACTION_COMPLETED, transactionID, publicKeyTarget, amountTransaction);
                }
            }
        }

        // -------------------------------------------
        /* 
		 * If it's the provider of services it updates the direction of the camera,
		 * if it's the customer then allows to move the camera
		 */
        void Update()
		{
			if (!m_isCustomer)
			{
				if (DrawingLinesController.Instance.ForwardPlayer != null)
				{
					m_cameraLocal.transform.forward = (Vector3)DrawingLinesController.Instance.ForwardPlayer.GetValue();
				}

				CheckForwardVectorRemote();
			}
			else
			{
				if (m_timeOutToClean > 0)
				{
					m_timeOutToClean -= Time.deltaTime;
					if (m_timeOutToClean <= 0)
					{
						m_messagesOnScreen.Clear();
					}
				}

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
				MoveCameraWithMouse();
#else
		GyroModifyCamera();
#endif

				UpdateForwardVector();
			}
		}


	}
}