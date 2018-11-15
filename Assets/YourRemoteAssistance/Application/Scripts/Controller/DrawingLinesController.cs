using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using VolumetricLines;
using YourBitcoinManager;
using YourCommonTools;
using YourEthereumManager;
using YourNetworkingTools;

namespace YourRemoteAssistance
{

	public class DrawingLinesController : MonoBehaviour
	{
		// ----------------------------------------------
		// EVENTS
		// ----------------------------------------------	
		public const string EVENT_DRAWINGLINES_DRAW_LINE = "EVENT_DRAWINGLINES_DRAW_LINE";
		public const string EVENT_DRAWINGLINES_DESTROY_LINE = "EVENT_DRAWINGLINES_DESTROY_LINE";
		public const string EVENT_DRAWINGLINES_VALID_POINT = "EVENT_DRAWINGLINES_VALID_POINT";

		// ----------------------------------------------
		// CONSTANTS
		// ----------------------------------------------	
		public const string COLOR_WHITE = "WHITE";
		public const string COLOR_BLACK = "BLACK";
		public const string COLOR_RED = "RED";
		public const string COLOR_BLUE = "BLUE";
		public const string COLOR_GREEN = "GREEN";

		public const float DISTANCE_FROM_CAMERA_TO_DRAW = 10f;

		public const char CHAR_SEPARATOR = '&';
		public const char CHAR_ENDOFLINE = ';';

		// ----------------------------------------------
		// SINGLETON
		// ----------------------------------------------	
		private static DrawingLinesController instance;

		public static DrawingLinesController Instance
		{
			get
			{
				if (!instance)
				{
					instance = GameObject.FindObjectOfType(typeof(DrawingLinesController)) as DrawingLinesController;
				}
				return instance;
			}
		}

		// ----------------------------------------------
		// PUBLIC MEMBERS
		// ----------------------------------------------	
		public GameObject LineStripPrefab;

		// ----------------------------------------------
		// PRIVATE MEMBERS
		// ----------------------------------------------	
		private List<Vector2> m_pointsTemporalLine = new List<Vector2>();
		private List<Vector2> m_pointsFinalLine = new List<Vector2>();
		private Dictionary<Vector2, GameObject> m_linesPainted = new Dictionary<Vector2, GameObject>();
		private int m_counterLinesPainted = 0;
		private string m_colorSelected = COLOR_RED;

		// MOUSE ACTIONS
		private bool m_isPressed = false;
		private Vector2 m_anchor = Vector2.zero;
		private bool m_movementHasBeenDetected = false;
		private GameObject m_temporalLineStrip;

		private NetworkVector3 m_forwardPlayer;

		public string ColorSelected
		{
			get { return m_colorSelected; }
			set { m_colorSelected = value; }
		}
		public NetworkVector3 ForwardPlayer
		{
			get { return m_forwardPlayer; }
			set { m_forwardPlayer = value; }
		}
		public bool IsPressed
		{
			get { return m_isPressed; }
			set { m_isPressed = value; }
		}


		// -------------------------------------------
		/* 
		 * Add listener
		 */
		void Start()
		{
			UIEventController.Instance.UIEvent += new UIEventHandler(OnUIEvent);
			NetworkEventController.Instance.NetworkEvent += new NetworkEventHandler(OnNetworkEvent);
		}

		// -------------------------------------------
		/* 
		* Remove listener
		*/
		void OnDestroy()
		{
			UIEventController.Instance.UIEvent -= OnUIEvent;
			NetworkEventController.Instance.NetworkEvent -= OnNetworkEvent;
		}

		// -------------------------------------------
		/*
		 * OnFingerDown
		 */
		private void OnFingerDown(Vector2 _position)
		{
#if !ENABLE_BITCOIN && !ENABLE_ETHEREUM
			if (m_forwardPlayer == null)
			{
				return;
			}
#elif ENABLE_BITCOIN
            if ((m_forwardPlayer == null) || (ScreenBitcoinController.Instance.ScreensEnabled > 0))
			{
				return;
			}
#elif ENABLE_ETHEREUM
            if ((m_forwardPlayer == null) || (ScreenEthereumController.Instance.ScreensEnabled > 0))
			{
				return;
			}
#endif

            m_anchor = new Vector2((int)_position.x, (int)_position.y);
			UIEventController.Instance.DispatchUIEvent(ScreenAssistanceView.EVENT_MAINMENU_CHECK_VALID_ORIGIN, (int)_position.x, (int)_position.y);
		}

		// -------------------------------------------
		/*
		* OnFingerMove
		*/
		private void OnFingerMove(Vector2 _position)
		{
			if (m_isPressed)
			{
				Vector2 currPos = new Vector2(_position.x, _position.y);

				if (Vector2.Distance(currPos, m_anchor) > 10)
				{
					m_anchor = new Vector2(currPos.x, currPos.y);
					m_movementHasBeenDetected = true;
					m_pointsTemporalLine.Add(new Vector2(currPos.x, currPos.y));
					m_pointsFinalLine.Add(new Vector2(currPos.x, currPos.y));

					CreateLine(BuildLine(m_pointsTemporalLine, false), true);
				}
			}
		}

		// -------------------------------------------
		/*
		 * OnFingerUp
		 */
		private void OnFingerUp(Vector2 _position)
		{
			if (m_isPressed && (m_forwardPlayer != null) && (m_pointsFinalLine.Count > 2))
			{
				m_isPressed = false;
				m_movementHasBeenDetected = false;

				if (m_temporalLineStrip != null)
				{
					GameObject.Destroy(m_temporalLineStrip);
					m_temporalLineStrip = null;
				}

				m_pointsTemporalLine.Clear();
				NetworkEventController.Instance.DispatchNetworkEvent(EVENT_DRAWINGLINES_DRAW_LINE, BuildLine(m_pointsFinalLine, true));
			}
		}

		// -------------------------------------------
		/*
		 * BuildLine
		 */
		private string BuildLine(List<Vector2> _pointsLine, bool _clearPoints)
		{
			Vector3 forward = DISTANCE_FROM_CAMERA_TO_DRAW * (Vector3)m_forwardPlayer.GetValue();
			Vector3 finalPosition = Vector3.zero + forward;

			string messageVertex = "" + YourNetworkTools.Instance.GetUniversalNetworkID() + CHAR_SEPARATOR + m_counterLinesPainted + CHAR_SEPARATOR + m_colorSelected + CHAR_ENDOFLINE;
			messageVertex += "" + forward.x + CHAR_SEPARATOR + forward.y + CHAR_SEPARATOR + forward.z + CHAR_ENDOFLINE;
			messageVertex += "" + finalPosition.x + CHAR_SEPARATOR + finalPosition.y + CHAR_SEPARATOR + finalPosition.z + CHAR_ENDOFLINE;
			for (int i = 0; i < _pointsLine.Count; i++)
			{
				Vector2 pos = _pointsLine[i];
				float factorX = (0.23f * (float)Screen.width);
				float factorY = 0.38f * (float)Screen.height;
				float xPos = (pos.x - (Screen.width / 2)) / factorX;
				float yPos = (pos.y - (Screen.height / 2)) / factorY;
				messageVertex += "" + xPos + CHAR_SEPARATOR + yPos;
				if (i + 1 < _pointsLine.Count)
				{
					messageVertex += CHAR_ENDOFLINE;
				}
			}
			if (_clearPoints)
			{
				_pointsLine.Clear();
			}

			return messageVertex;
		}

		// -------------------------------------------
		/* 
		 * We will create a line with the data received
		 */
		public void CreateLine(string _data, bool _isTemporal)
		{
			string[] data = _data.Split(';');

			string[] sMainConfig = data[0].Split(CHAR_SEPARATOR);
			int universalID = int.Parse(sMainConfig[0]);
			int counterObject = int.Parse(sMainConfig[1]);
			string colorSelected = sMainConfig[2];

			string[] sforward = data[1].Split(CHAR_SEPARATOR);
			Vector3 forward = new Vector3(float.Parse(sforward[0]), float.Parse(sforward[1]), float.Parse(sforward[2]));
			string[] sPosition = data[2].Split(CHAR_SEPARATOR);
			Vector3 finalPosition = new Vector3(float.Parse(sPosition[0]), float.Parse(sPosition[1]), float.Parse(sPosition[2]));

			GameObject lineStripPrefab = Utilities.AddChild(this.gameObject.transform, LineStripPrefab);
			List<Vector3> vertexs = new List<Vector3>();
			for (int i = 3; i < data.Length; i++)
			{
				string[] sVertex = data[i].Split(CHAR_SEPARATOR);
				Vector3 screenPosition = new Vector3(float.Parse(sVertex[0]), float.Parse(sVertex[1]), 0);
				vertexs.Add(screenPosition);
			}
			Vector3[] linesVertices = vertexs.ToArray();
			lineStripPrefab.GetComponent<VolumetricLineStripBehavior>().UpdateLineVertices(linesVertices);

			switch (colorSelected)
			{
				case COLOR_WHITE:
					lineStripPrefab.GetComponent<VolumetricLineStripBehavior>().LineColor = Color.grey;
					break;

				case COLOR_BLACK:
					lineStripPrefab.GetComponent<VolumetricLineStripBehavior>().LineColor = Color.black;
					break;

				case COLOR_RED:
					lineStripPrefab.GetComponent<VolumetricLineStripBehavior>().LineColor = Color.red;
					break;

				case COLOR_BLUE:
					lineStripPrefab.GetComponent<VolumetricLineStripBehavior>().LineColor = Color.blue;
					break;

				case COLOR_GREEN:
					lineStripPrefab.GetComponent<VolumetricLineStripBehavior>().LineColor = Color.green;
					break;

				default:
					lineStripPrefab.GetComponent<VolumetricLineStripBehavior>().LineColor = Color.black;
					break;
			}
			lineStripPrefab.transform.position = finalPosition;
			lineStripPrefab.transform.forward = forward;

			if (!_isTemporal)
			{
				m_linesPainted.Add(new Vector2(universalID, m_counterLinesPainted), lineStripPrefab);
				m_counterLinesPainted++;
			}
			else
			{
				if (m_temporalLineStrip != null)
				{
					GameObject.Destroy(m_temporalLineStrip);
					m_temporalLineStrip = null;
				}
				m_temporalLineStrip = lineStripPrefab;
			}
		}


		// -------------------------------------------
		/* 
		 * Send an event to delete all the lines drawn by a user
		 */
		public void DestroyMyLines(int _universalID)
		{
			NetworkEventController.Instance.DispatchNetworkEvent(EVENT_DRAWINGLINES_DESTROY_LINE, _universalID.ToString());
		}

		// -------------------------------------------
		/* 
		 * Check the user drawing lines
		 */
		void Update()
		{
			if (!m_isPressed)
			{
				if (Input.GetMouseButtonDown(0))
				{
					Vector2 posDown = Input.mousePosition;
					OnFingerDown(posDown);
				}
			}
			else
			{
				if (Input.GetMouseButtonUp(0))
				{
					Vector2 posUp = Input.mousePosition;
					OnFingerUp(posUp);
				}
				else
				{
					if (Input.GetMouseButton(0))
					{
						Vector2 posMoved = Input.mousePosition;
						OnFingerMove(posMoved);
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * Manager of global events
		 */
		private void OnNetworkEvent(string _nameEvent, bool _isLocalEvent, int _networkOriginID, int _networkTargetID, params object[] _list)
		{
			if (_nameEvent == EVENT_DRAWINGLINES_DRAW_LINE)
			{
				CreateLine((string)_list[0], false);
			}
			if (_nameEvent == EVENT_DRAWINGLINES_DESTROY_LINE)
			{
				int universalID = int.Parse((string)_list[0]);
				foreach (KeyValuePair<Vector2, GameObject> item in m_linesPainted)
				{
					if (item.Key.x == universalID)
					{
						GameObject.Destroy(item.Value);
						m_linesPainted.Remove(item.Key);
						return;
					}
				}
			}
		}

		// -------------------------------------------
		/* 
		 * OnUIEvent
		 */
		private void OnUIEvent(string _nameEvent, params object[] _list)
		{
			if (_nameEvent == EVENT_DRAWINGLINES_VALID_POINT)
			{
				m_isPressed = (bool)_list[0];
				if (m_isPressed)
				{
					m_movementHasBeenDetected = false;
					m_pointsTemporalLine.Clear();
					m_pointsFinalLine.Clear();
					m_pointsTemporalLine.Add(new Vector2(m_anchor.x, m_anchor.y));
					m_pointsFinalLine.Add(new Vector2(m_anchor.x, m_anchor.y));
				}
			}
		}


	}
}