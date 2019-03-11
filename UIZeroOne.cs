using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Enums_Game;
using DG.Tweening;

public class UIZeroOne : GameBase
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
	private Text _allScore;    // User총점

	[SerializeField]
	private Text[] _shot;    // 1,2,3 발의 샷 점수

	[SerializeField]
	private Text _ppd;    // 현재 던진거에 평균점수.

	private int _nowRound;    // 현재 라운드.

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

	/// <summary>
	/// 최초 게임시작일때
	/// </summary>
	public override void GameInit()
	{
		Debug.Log("첫 게임 초기화");

		switch (DataManager.instance.MODEDATA.ZEROONE_DATA)
		{
			case ZeroOneModeData.ZONE301:
				_gameRound = 10;
				_targetScore = 301;
				break;

			case ZeroOneModeData.ZONE501:
				_gameRound = 10;
				_targetScore = 501;
				break;

			case ZeroOneModeData.ZONE701:
				_gameRound = 15;
				_targetScore = 701;
				break;

			case ZeroOneModeData.ZONE901:
				_gameRound = 20;
				_targetScore = 901;
				break;

			case ZeroOneModeData.ZONE1101:
				_gameRound = 20;
				_targetScore = 1101;
				break;

			case ZeroOneModeData.ZONE1501:
				_gameRound = 20;
				_targetScore = 1501;
				break;
		}

		_playerTurn.text = "";
		_round.text = "";
		_allScore.text = "";
		GameObject prefabEffect = null;
		_playerEdgeEffectList = new GameObject[4];
		for (int i = 0; i < 4; i++)
		{
			prefabEffect = Resources.Load<GameObject>(string.Format("Effect/UI/Player_{0}", i + 1));
			_playerEdgeEffectList[i] = Instantiate(prefabEffect, _teamEdgeImage.transform.parent);
			_playerEdgeEffectList[i].SetActive(false);
		}

		SetPlayerEdge(0);

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
	}

	/// <summary>
	/// 새로운 턴 시작.
	/// </summary>
	public override void TurnStart(int turn)
	{
		_playerTurn.text = "Player " + (turn + 1);

		int playerIndex = turn % base.CHECKTURN;
		SetPlayerEdge(playerIndex);

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

	/// <summary>
	/// 각 라운드 점수 갱신
	/// </summary>
	public void SetRoundScore()
	{
		_allScore.text = (_targetScore - base.GetUserTotalSum()).ToString();

		for (int i = 0; i < _teamScoreList.Count; i++)
		{
			_teamScoreList[i].text = (base.GetUserTotalSum(i)).ToString();
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
					if (base.GetUserRoundSum(index) >= 0)
					{
						child.GetComponent<Text>().text = base.GetUserRoundSum(index).ToString();
					}
					else
					{
						child.GetComponent<Text>().text = "A";
					}
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

		_ppd.text = base.GetUserPPD();
	}

	/// <summary>
	/// 게임 점수 얻기
	/// </summary>
	/// <param name="score"></param>
	/// <param name="data"></param>
	public override void GameSetPoint(int score, GameScoreData data)
	{
		int value = (int.Parse(_allScore.text) - score);
		if (value < 0)
		{
			base.AddUserScore(-score, data);
			PlayAwardVideo();
			return;
		}
		else if (value > 0)
			base.AddUserScore(score, data);
		else
		{   //0점됫을경우 [게임승리조건].
			base.AddUserScore(score, data);
			PlayAwardVideo(true);
			return;
		}

		if (base.GetUserTurnCheck() == true)
		{
			PlayAwardVideo();
		}
		SetRoundScore();
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