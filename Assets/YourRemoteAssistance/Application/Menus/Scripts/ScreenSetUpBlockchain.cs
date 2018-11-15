using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
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
	* ScreenSetUpBlockchain
	* 
	* Screen to set up the Bitcoin wallet
	* 
	* @author Esteban Gallardo
	*/
    public class ScreenSetUpBlockchain : ScreenBaseView, IBasicView
	{
		public const string SCREEN_NAME = "SCREEN_SETUP_WALLET";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private GameObject m_root;
		private GameObject m_blockchain;
		private GameObject m_startSession;
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

			m_container.Find("Description").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.set.up.your.bitcoin.wallet.before.start");

			m_blockchain = m_container.Find("Button_Blockchain").gameObject;
			m_blockchain.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.no.bitcoin.wallet.available");
			m_blockchain.GetComponent<Button>().onClick.AddListener(SetUpBitcoinAddress);

            m_blockchain.transform.Find("IconBitcoin").gameObject.SetActive(false);
            m_blockchain.transform.Find("IconEthereum").gameObject.SetActive(false);

            m_startSession = m_container.Find("Button_StartService").gameObject;
			m_startSession.transform.Find("Text").GetComponent<Text>().text = LanguageController.Instance.GetText("screen.bitcoin.wallet.ok.then.continue");
			m_startSession.GetComponent<Button>().onClick.AddListener(StartSession);
			m_startSession.SetActive(false);

#if ENABLE_BITCOIN
            BitcoinEventController.Instance.BitcoinEvent += new BitcoinEventHandler(OnBitcoinEvent);
            m_blockchain.transform.Find("IconBitcoin").gameObject.SetActive(true);
#elif ENABLE_ETHEREUM
            EthereumEventController.Instance.EthereumEvent += new EthereumEventHandler(OnEthereumEvent);
            m_blockchain.transform.Find("IconEthereum").gameObject.SetActive(true);
#endif
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

#if ENABLE_BITCOIN
            BitcoinEventController.Instance.BitcoinEvent -= OnBitcoinEvent;
#elif ENABLE_ETHEREUM
            EthereumEventController.Instance.EthereumEvent -= OnEthereumEvent;
#endif
            UIEventController.Instance.DispatchUIEvent(UIEventController.EVENT_SCREENMANAGER_DESTROY_SCREEN, this.gameObject);

			return false;
		}

		// -------------------------------------------
		/* 
		* SetUpBitcoinAddress
		*/
		private void SetUpBitcoinAddress()
		{
#if ENABLE_BITCOIN
            ScreenBitcoinController.Instance.InitializeBitcoin(ScreenBitcoinPrivateKeyView.SCREEN_NAME);
#elif ENABLE_ETHEREUM
            ScreenEthereumController.Instance.InitializeEthereum(ScreenEthereumPrivateKeyView.SCREEN_NAME);
#endif
        }

        // -------------------------------------------
        /* 
		* StartSession
		*/
        private void StartSession()
		{
			MenuScreenController.Instance.CreateNewScreen(ScreenLoadingView.SCREEN_NAME, UIScreenTypePreviousAction.DESTROY_ALL_SCREENS, false, null);
			MenuScreenController.Instance.CreateOrJoinRoomInServer(false);
		}

		// -------------------------------------------
		/* 
		* OnMenuBasicEvent
		*/
		protected override void OnMenuEvent(string _nameEvent, params object[] _list)
		{
		}

		// -------------------------------------------
		/* 
		* Bitcoin Event Manager
		*/
		private void OnBitcoinEvent(string _nameEvent, object[] _list)
		{
			if (_nameEvent == BitCoinController.EVENT_BITCOINCONTROLLER_BALANCE_WALLET)
			{
				decimal balanceValue = (decimal)((float)_list[0]);
				float balanceInCurrency = (float)(balanceValue * BitCoinController.Instance.GetCurrentExchange());
				m_blockchain.transform.Find("Text").GetComponent<Text>().text = balanceValue.ToString() + " BTC" + " /\n" + balanceInCurrency + " " + BitCoinController.Instance.CurrentCurrency;
				m_startSession.SetActive(true);
			}
		}

        // -------------------------------------------
        /* 
		* Bitcoin Event Manager
		*/
        private void OnEthereumEvent(string _nameEvent, object[] _list)
        {
            if (_nameEvent == ScreenEthereumPrivateKeyView.EVENT_SCREENETHEREUMPRIVATEKEY_WALLET_BALANCE)
            {
                if ((bool)_list[1])
                {
                    decimal balanceValue = (decimal)_list[2];
                    float balanceInCurrency = (float)(balanceValue * EthereumController.Instance.GetCurrentExchange());
                    m_blockchain.transform.Find("Text").GetComponent<Text>().text = balanceValue.ToString() + " ETH" + " /\n" + balanceInCurrency + " " + EthereumController.Instance.CurrentCurrency;
                    m_startSession.SetActive(true);
                }
            }
        }
    }
}