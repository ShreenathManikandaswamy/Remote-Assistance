using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using YourCommonTools;
using YourNetworkingTools;

namespace YourRemoteAssistance
{

	/******************************************
	 * 
	 * ItemClientView
	 * 
	 * Display an item that represents a connected client
	 * 
	 * @author Esteban Gallardo
	 */
	public class ItemClientView : NetworkItemView, IBasicView
	{
		// ----------------------
		// PRIVATE MEMBERS
		// ----------------------
		private Text m_clientName;
		private Text m_clientPosition;
		private PlayerConnectionData m_connectionData;
		private GameObject m_playerGO;
		private NetworkVector3 m_forward;
		private string m_phoneNumber;
		private string m_publicBlockchainAddress;

		public PlayerConnectionData ConnectionData
		{
			get { return m_connectionData; }
		}
		public GameObject PlayerGO
		{
			get { return m_playerGO; }
		}
		public NetworkVector3 Forward
		{
			get { return m_forward; }
			set { m_forward = value; }
		}
		public string PhoneNumber
		{
			get { return m_phoneNumber; }
			set { m_phoneNumber = value; }
		}
		public string PublicBlockchainAddress
		{
			get { return m_publicBlockchainAddress; }
			set { m_publicBlockchainAddress = value; }
		}

		// -------------------------------------------
		/* 
		 * Initialitzation of all the references to the graphic resources
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);
			m_playerGO = ((_list[1] != null) ? (GameObject)_list[1] : null);
			m_connectionData = (PlayerConnectionData)_list[2];
			m_clientName = this.gameObject.transform.Find("Text").GetComponent<Text>();
			m_clientName.text = m_connectionData.Name;
			m_clientPosition = this.gameObject.transform.Find("Position").GetComponent<Text>();
			m_clientPosition.text = "PROVIDER";
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override void Destroy()
		{
			NetworkEventController.Instance.DispatchNetworkEvent(NetworkEventController.EVENT_PLAYERCONNECTIONCONTROLLER_KICK_OUT_PLAYER, m_connectionData.Id.ToString());
			ReleaseMemory();
		}

		// -------------------------------------------
		/* 
		 * SetActivation
		 */
		public void SetActivation(bool _activation)
		{
			
		}

		// -------------------------------------------
		/* 
		 * ReleaseMemory
		 */
		public void ReleaseMemory()
		{
			m_connectionData = null;
			m_playerGO = null;
		}

		// -------------------------------------------
		/* 
		 * Update the quaternion information
		 */
		void Update()
		{
			if (m_forward != null)
			{
				m_clientPosition.text = "[" + ((Vector3)m_forward.GetValue()).ToString() + "]";
			}
		}


	}
}