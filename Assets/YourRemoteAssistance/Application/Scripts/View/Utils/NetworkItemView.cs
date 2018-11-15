using UnityEngine;
using YourCommonTools;
using YourNetworkingTools;

namespace YourRemoteAssistance
{

	/******************************************
	 * 
	 * NetworkItemView
	 * 
	 * Base class for all network items
	 * 
	 * @author Esteban Gallardo
	 */
	public class NetworkItemView : BaseItemView, IBasicView
	{
		// PROTECTED
		protected INetworkVariable m_networkVariable = null;
		private GameObject m_networkObject = null;
		protected string m_nameVariable;
		protected int m_prefabIndex = -1;
		protected Vector3 m_prefabPosition = Vector3.zero;

		public INetworkVariable NetworkVariable
		{
			get { return m_networkVariable; }
		}
		public IGameNetworkActor NetworkObject
		{
			get { return m_networkObject.GetComponent<IGameNetworkActor>(); }
		}
		public string NameVariable
		{
			get { return m_nameVariable; }
		}
		public int PrefabIndex
		{
			get { return m_prefabIndex; }
		}
		public virtual Vector3 PrefabPosition
		{
			get { return m_prefabPosition; }
		}

		// -------------------------------------------
		/* 
		 * Initialitzation of all the references to the graphic resources
		 */
		public override void Initialize(params object[] _list)
		{
			base.Initialize(_list);
		}

		// -------------------------------------------
		/* 
		 * Destroy all the references		
		 */
		public override void Destroy()
		{
			base.Destroy();
			if (m_networkVariable != null)
			{
				m_networkVariable.Destroy();
			}
			m_networkVariable = null;
			if (m_networkObject != null)
			{
				NetworkObject.Destroy();
			}
			m_networkObject = null;
		}

		// -------------------------------------------
		/* 
		 * Set reference
		 */
		public virtual void SetReference(INetworkVariable _reference)
		{
			m_networkVariable = _reference;
		}


		// -------------------------------------------
		/* 
		 * SetNetworkObject
		 */
		protected void SetNetworkObject(GameObject _value)
		{
			m_networkObject = _value;
		}

		// -------------------------------------------
		/* 
		 * Runs an action
		 */
		public override void ApplyAction()
		{
		}
	}
}