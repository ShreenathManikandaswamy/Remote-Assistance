using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RTMPStreaming
{
	/******************************************
	 * 
	 * RTMPStreaming
	 * 
	 * Manage the RTMP stream
	 * 
	 * @author Esteban Gallardo
	 */
	public class RTMPController : MonoBehaviour
	{
		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static RTMPController instance;

		public static RTMPController Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(RTMPController)) as RTMPController;
					if (!instance)
					{
						GameObject container = new GameObject();
						container.name = "RTMPController";
						instance = container.AddComponent(typeof(RTMPController)) as RTMPController;
					}
				}
				return instance;
			}
		}

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private AndroidJavaObject m_streamingAndroid;
		private bool m_runningStream = false;
		private WebCamTexture m_webCamTexture;
		private string m_urlStream;

		// -------------------------------------------
		/* 
		 * Initialitzation url streaming
		 */
		// rtmp://46.101.241.31/live/android;
		// private int m_width = 640;
		// private int m_height = 480;
		// private int m_fps = 30;
		// private int m_bitRate = 1200 * 1024;
		public void Initialitzation(string _urlStream, int _width, int _height, int _fps, int _bitRate)
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			m_urlStream = _urlStream;
			m_runningStream = false;
			m_streamingAndroid = new AndroidJavaObject("com.yourvrexperience.androidcamerastream.RTMPStream");
			m_streamingAndroid.Call("InitRtmpClient", m_urlStream, _width, _height, _fps, _bitRate);
#endif
		}

		// -------------------------------------------
		/* 
		 * StopStream
		 */
		public void StopStream()
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			m_streamingAndroid.Call("StopStreaming");
#endif
		}

		// -------------------------------------------
		/* 
		 * Starting the stream in the update function
		 */
		void Update()
		{
#if UNITY_ANDROID && !UNITY_EDITOR
			if (!m_runningStream)
			{
				// START STREAM WHEN READY
				if (m_streamingAndroid != null)
				{
					if (m_streamingAndroid.Call<System.Boolean>("StartStreaming"))
					{
						m_runningStream = true;
					}
				}
			}
#endif
		}
	}
}