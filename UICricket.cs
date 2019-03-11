using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Enums_Game;
using DG.Tweening;
using System.Linq;

public class UICricket : GameBase
{
	[SerializeField]
	private Text _playerTurn; // 현재 플레이어 정보 (Player 1)

	[SerializeField]
	private Text _round;    //몇라운드인지 ( 1/8 )

	[SerializeField]
	private GameObject[] _roundInfoList = new GameObject[4];    // 각 라운드 정보 리스트

	[SerializeField]
	private Transform _content;    // 스크롤렉트 오브젝트.

	[SerializeField]
	private Text[] _shot;    // 1,2,3 발의 샷 점수

	[SerializeField]
	private Text _ppd;    // 현재 던진거에 평균점수.

	[SerializeField]
	private Text _teamScoreText;    // 팀별 하단 점수 Instantiate용

	[SerializeField]
	private Font[] _playerFontList;    // 플레이어별 폰트

	[SerializeField]
	private Image _teamEdgeImage;    // 팀별 배경

	[SerializeField]
	private Sprite[] _playerEdgeSpriteList;    // 플레이어별 배경 이미지

	[SerializeField]
	private RawImage _playerColorTexture;    // 뎁스 카메라 영상 텍스처

	private GameObject[] _playerEdgeEffectList;     // 플레이어별 배경 연출
	private GameObject _currentPlayerEdgeEffect = null;

	private List<Text> _teamScoreList = new List<Text>();    // 각 팀별 점수 정보 리스트

	private int[] _cricket_Point_Score = new int[7] { 20, 19, 18, 17, 16, 15, 50 };                 //크리켓 포인트를 얻을수있는 배열

	private Text[] _cricket_Text_White_Score;                 //크리켓 가운데 점수 확인용 텍스트 (화이트)
	private Text[] _cricket_Text_Black_Score;                 //크리켓 가운데 점수 확인용 텍스트 (블랙)

	private Transform[] _cricket_User_Point_Transform = new Transform[4];        //User 점수 체크
	private Text[][] _cricket_User_Point_Mark = new Text[4][];        //User 점수 체크

	/// <summary>
	/// 최초 게임시작일때
	/// </summary>
	public override void GameInit()
	{
		Debug.Log("첫 게임 초기화");

		if (DataManager.instance.MODEDATA.CRICKET_DATA != CricketModeData.HIDDEN)
		{
			_gameRound = 15;
		}
		else
		{
			_gameRound = 20;
		}

		_playerTurn.text = "";
		_round.text = "";
		GameObject prefabEffect = null;
		_playerEdgeEffectList = new GameObject[4];
		for (int i = 0; i < 4; i++)
		{
			prefabEffect = Resources.Load<GameObject>(string.Format("Effect/UI/Player_{0}", i + 1));
			_playerEdgeEffectList[i] = Instantiate(prefabEffect, _teamEdgeImage.transform.parent);
			_playerEdgeEffectList[i].SetActive(false);
		}

		//	SetPlayerEdge(0);

		int originalFontSize = 0;
		for (int i = 0; i < _shot.Length; i++)
		{
			originalFontSize = _shot[i].fontSize;
			_shot[i].text = "";
			_shot[i].font = _playerFontList[0];
			_shot[i].fontSize = originalFontSize;
		}

		GameObject playerInfo = GameObject.Find("Player").transform.Find(_battle.ToString()).gameObject;
		playerInfo.SetActive(true);

		for (int i = 0; i < base.CHECKTURN; i++)
		{
			Text obj = (Text)Instantiate(_teamScoreText, new Vector3(0, 0, 0), Quaternion.identity);
			obj.transform.SetParent(playerInfo.transform.GetChild(i).transform.Find("BackGround").transform);
			obj.transform.localPosition = Vector3.zero;
			obj.transform.localScale = Vector3.one;
			obj.transform.rotation = Quaternion.Euler(Vector3.zero);
			obj.text = "";
			obj.font = _playerFontList[i];
			obj.fontSize = 52;
			obj.gameObject.SetActive(true);
			_teamScoreList.Add(obj);
		}

		SetEnableDepthCameraColorTexture(true);

		_cricket_Text_White_Score = GameObject.Find("CricketScore").transform.Find("BackGround").transform.Find("text_w").GetComponentsInChildren<Text>();
		_cricket_Text_Black_Score = GameObject.Find("CricketScore").transform.Find("BackGround").transform.Find("text_b").GetComponentsInChildren<Text>(true);

		for (int i = 0; i < 4; i++)
		{
			_cricket_User_Point_Transform[i] = GameObject.Find("Player_Check").transform.Find("Player" + (i + 1)).transform;
			_cricket_User_Point_Mark[i] = _cricket_User_Point_Transform[i].GetComponentsInChildren<Text>(true);
			_cricket_User_Point_Transform[i].gameObject.SetActive(false);
		}

		for (int i = 0; i < base.CHECKTURN; i++)
		{
			_cricket_User_Point_Transform[i].gameObject.SetActive(true);
		}

		switch (base.CHECKTURN)
		{
			case 1:
				_cricket_User_Point_Transform[0].transform.localPosition = _cricket_User_Point_Transform[1].transform.localPosition;
				break;

			case 2:
				_cricket_User_Point_Transform[0].transform.localPosition = _cricket_User_Point_Transform[1].transform.localPosition;
				_cricket_User_Point_Transform[1].transform.localPosition = _cricket_User_Point_Transform[2].transform.localPosition;
				break;
		}

		for (int i = 0; i < _cricket_Point_Score.Length; i++)
		{
			string str = string.Empty;
			if (_cricket_Point_Score[i] == 50)
			{
				str = "A";
			}
			else
			{
				str = _cricket_Point_Score[i].ToString();
			}

			_cricket_Text_White_Score[i].text = str;
			_cricket_Text_Black_Score[i].text = str;
		}
	}

	/// <summary>
	/// 새로운 턴 시작.
	/// </summary>
	public override void TurnStart(int turn)
	{
		_playerTurn.text = "Player " + (turn + 1);

		int playerIndex = turn % base.CHECKTURN;
		//	SetPlayerEdge(playerIndex);

		int originalFontSize = 0;
		for (int i = 0; i < _shot.Length; i++)
		{
			originalFontSize = _shot[i].fontSize;
			_shot[i].text = "";
			_shot[i].font = _playerFontList[playerIndex];
			_shot[i].fontSize = originalFontSize;
		}
		_round.text = (base.GetUserRound + 1) + "/" + _gameRound;

		for (int i = 0; i < _roundInfoList.Length; i++)
		{
			_roundInfoList[i].transform.DOLocalMoveX(500f, 0.5f).SetEase(Ease.OutExpo).SetDelay(i * 0.25f).SetLoops(2, LoopType.Yoyo);
		}

		SetRoundScore();
	}

	public void SetRoundScore()
	{
		for (int i = 0; i < _teamScoreList.Count; i++)
		{
			_teamScoreList[i].text = base.GetUserTotalSum(i).ToString();
		}

		for (int i = 0; i < _roundInfoList.Length; i++)
		{
			Transform[] tempTransforms = _roundInfoList[i].GetComponentsInChildren<Transform>();

			int index = base.GetUserRound > 2 && base.GetUserRound < _gameRound - 1 ? (base.GetUserRound - 2) + i : i;

			if (base.GetUserRound >= _gameRound - 1)
			{
				index = (_gameRound - 4) + index;
			}
			foreach (Transform child in tempTransforms)
			{
				if (child.name.Equals("Text"))
				{
					string str = string.Empty;
					foreach (GameScoreData score in base.GetRoundScoreData(index))
					{
						if (score >= GameScoreData.Single && score <= GameScoreData.Triple)
							str += ((int)score).ToString();
						else
							str += "0";
					}

					child.GetComponent<Text>().text = str;
				}

				if (child.name.Contains("Round"))
				{
					child.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/Round/R_" + (index));
				}
			}
		}

		for (int i = 0; i < base.GetUserThrowCount(_gameNowRound); i++)
		{
			_shot[base.GetUserThrowCount(_gameNowRound) - i - 1].text = base.GetUserRoundDart(i);
		}

		for (int i = 0; i < _cricket_Point_Score.Length; i++)
		{
			for (int j = 0; j < base.CHECKTURN; j++)
			{
				int score = Mathf.Min(base.GetCricketMarkScore(_cricket_Point_Score[i], j), 3);
				string str = string.Empty;
				if (score != 0)
				{
					str = score.ToString();
				}
				_cricket_User_Point_Mark[j][i].text = str;
			}
		}

		_ppd.text = base.GetUserPPD();
	}

	/// <summary>
	/// 게임 점수 얻기
	/// </summary>
	/// <param name="score"></param>
	/// <param name="data"></param>
	public override void GameSetPoint(int score, GameScoreData data)
	{
		if (_cricket_Point_Score.Contains(score) == false)
		{
			Debug.Log("[크리켓점수에 포함되지않음]");
			base.AddUserScore(0, GameScoreData.None);
		}
		else
		{
			base.AddUserScore(score, data);
		}

		SetRoundScore();
		if (base.GetUserTurnCheck() == true)
		{
			PlayAwardVideo();
		}
	}

	/// <summary>
	/// 뎁스 카메라 영상 on/off
	/// </summary>
	private void SetEnableDepthCameraColorTexture(bool isEnable)
	{
		_playerColorTexture.enabled = isEnable;
	}

	/// <summary>
	/// 플레이어 배경 설정
	/// </summary>
	private void SetPlayerEdge(int index)
	{
		_teamEdgeImage.sprite = _playerEdgeSpriteList[index];

		if (_currentPlayerEdgeEffect != null)
		{
			_currentPlayerEdgeEffect.SetActive(false);
			_currentPlayerEdgeEffect = null;
		}

		_currentPlayerEdgeEffect = _playerEdgeEffectList[index];
		_currentPlayerEdgeEffect.SetActive(true);
	}

	private void Update()
	{
		// 뎁스 카메라 영상 업데이트
		if (_playerColorTexture.texture == null && InputManager.Instance.SensorDepthCamera != null && InputManager.Instance.SensorDepthCamera.IsValid())
		{
			_playerColorTexture.texture = InputManager.Instance.SensorDepthCamera.GetColorTexture();
			_playerColorTexture.rectTransform.localScale = InputManager.Instance.SensorDepthCamera.GetColorTextureScale();
			_playerColorTexture.color = Color.white;
		}
		else if (_playerColorTexture.texture != null && (InputManager.Instance.SensorDepthCamera == null || !InputManager.Instance.SensorDepthCamera.IsValid()))
		{
			_playerColorTexture.texture = null;
			_playerColorTexture.color = new Color(0f, 0f, 0f, 0f);
		}
	}
}