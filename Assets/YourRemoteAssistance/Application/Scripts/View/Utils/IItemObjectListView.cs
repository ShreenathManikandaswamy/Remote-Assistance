using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using YourCommonTools;
using YourNetworkingTools;

namespace YourRemoteAssistance
{

	/******************************************
	 * 
	 * IItemObjectListView
	 * 
	 * Interface of the items in the list of objects
	 * 
	 * @author Esteban Gallardo
	 */
	public interface IItemObjectListView : IBasicItemView
	{
		// VARIABLE
		int PrefabIndex { get; }
		Vector3 PrefabPosition { get; }
		string NameVariable { get; }
		INetworkVariable NetworkVariable { get; }

		// FUNCTIONS
		void SetReference(INetworkVariable _reference);
		void ApplyAction();
	}
}