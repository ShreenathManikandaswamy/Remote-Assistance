using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace YourRemoteAssistance
{
	/******************************************
	 * 
	 * GameConfiguration
	 * 
	 * We keep the global information 
	 * 
	 * @author Esteban Gallardo
	 */
	public static class GameConfiguration
	{
		public const string SOUND_MAIN_MENU = "SOUND_MAIN_MENU";
		public const string SOUND_SELECTION_FX = "SOUND_FX_SELECTION";

		public const string URL_RTMP_SERVER_ASSISTANCE = "rtmp://46.101.241.31/live/remoteassistance";
		// public const string URL_RTMP_SERVER_ASSISTANCE = "rtmp://localhost/live/remoteassistance";

		private const string USER_TYPE_COOCKIE = "USER_TYPE_COOCKIE";
		private const string PHONE_NUMBER_COOCKIE = "PHONE_NUMBER_COOCKIE";

		// -------------------------------------------
		/* 
		 * Will save the data for the user type
		 */
		public static void SaveUserType(bool _isCustomer)
		{
			PlayerPrefs.SetInt(USER_TYPE_COOCKIE, (_isCustomer ? 1 : 0));
		}

		// -------------------------------------------
		/* 
		 * Will load the data of the user type
		 */
		public static bool IsCustomer()
		{
			return (PlayerPrefs.GetInt(USER_TYPE_COOCKIE, -1) == 1);
		}

		// -------------------------------------------
		/* 
		 * Save a the phone number
		 */
		public static void SavePhoneNumber(string _phoneNumber)
		{
			PlayerPrefs.SetString(PHONE_NUMBER_COOCKIE, _phoneNumber);
		}

		// -------------------------------------------
		/* 
		 * Get the phone number
		 */
		public static string LoadPhoneNumber()
		{
			return PlayerPrefs.GetString(PHONE_NUMBER_COOCKIE, "");
		}
	}

}