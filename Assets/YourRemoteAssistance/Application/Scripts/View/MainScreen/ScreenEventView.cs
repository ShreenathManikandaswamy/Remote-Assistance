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
	 * ScreenEventView
	 * 
	 * @author Esteban Gallardo
	 */
	public class ScreenEventView : ScreenBaseView, IBasicView
	{
		public const string SCREEN_EVENT = "SCREEN_EVENT";

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private AppEventData m_appEventData;
		private PlayerConnectionData m_playerConnectionData;
		private GameObject m_root;
		private Transform m_container;
		private Button m_okButton;
		private Button m_cancelButton;
		private Text m_textDescription;
		private Text m_title;
		private InputField m_inputParameters;
		private Image m_imageContent;

		// -------------------------------------------
		/* 
		 * Constructor
		 */
		public override void Initialize(params object[] _list)
		{
			m_playerConnectionData = (PlayerConnectionData)_list[0];
			m_appEventData = (AppEventData)_list[1];

			m_root = this.gameObject;
			m_container = m_root.transform.Find("Content");

			if (m_container.Find("Button_OK") != null)
			{
				m_okButton = m_container.Find("Button_OK").GetComponent<Button>();
				m_okButton.gameObject.GetComponent<Button>().onClick.AddListener(OkPressed);
			}
			if (m_container.Find("Button_Cancel") != null)
			{
				m_cancelButton = m_container.Find("Button_Cancel").GetComponent<Button>();
				m_cancelButton.gameObject.GetComponent<Button>().onClick.AddListener(CancelPressed);
			}

			if (m_container.Find("TextEvent") != null)
			{
				m_textDescription = m_container.Find("TextEvent").GetComponent<Text>();
				m_textDescription.text = m_appEventData.NameEvent;
			}
			if (m_container.Find("Title") != null)
			{
				m_title = m_container.Find("Title").GetComponent<Text>();
				m_title.text = "EVENT";
			}
			if (m_container.Find("InputField") != null)
			{
				m_inputParameters = m_container.Find("InputField").GetComponent<InputField>();
				m_inputParameters.text = m_appEventData.ToStringParameters();
			}

			UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
		}

		// -------------------------------------------
		/* 
		 * Destroy
		 */
		public override bool Destroy()
		{
			if (base.Destroy()) return true;
			UIEventController.Instance.UIEvent -= OnUIEvent;
			GameObject.Destroy(this.gameObject);
			return false;
		}

		// -------------------------------------------
		/* 
		 * OkPressed
		 */
		private void OkPressed()
		{
			string[] parameters = m_inputParameters.text.Split(',');
			NetworkEventController.Instance.DispatchCustomNetworkEvent(m_appEventData.NameEvent, false, CommunicationsController.Instance.NetworkID, m_playerConnectionData.Id, parameters);
			Destroy();
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
		 * SetTitle
		 */
		public void SetTitle(string _text)
		{
			if (m_title != null)
			{
				m_title.text = _text;
			}
		}

		// -------------------------------------------
		/* 
		 * OnUIEvent
		 */
		private void OnUIEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == ScreenController.EVENT_FORCE_DESTRUCTION_POPUP)
			{
				Destroy();
			}
			if (_nameEvent == ScreenController.EVENT_FORCE_TRIGGER_OK_BUTTON)
			{
				OkPressed();
			}
		}
	}
}