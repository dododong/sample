using Enums_Common;
using Enums_Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class Data
{
	public int score;
	public GameScoreData data;
	public GameScoreData _cricketdata;
	public bool isSum;      //점수를 더할것인지 안더할것인지.
}

public class WinnerData
{
	public string name;
	public int score;
	public string ppd;
}

public abstract class GameBase : UIBase
{
	/// <summary>
	/// 버스트시 점수규약
	/// </summary>
	public static readonly int BUST = -1000;

	/// <summary>
	/// 유저가 자기턴에 다트를 던지는 횟수 3번
	/// </summary>
	public static readonly int DARTSHOTLENGTH = 3;

	private enum ParamEnum
	{
		TURN = 0,   //유저턴 Player1,Player2,
		NOWROUND,   //현재 라운드
		DARTNUMBER, //몇번째 다트인지
		USERTURN,   //유저턴 계산저장.
		ISSUM,   //
	}

	protected GameBattleData _battle;
	private int _userTurn;  //턴이 들어올때마다 1씩 체크
	private int _checkTurn;
	public int _playerCount; //참여 유저숫자.
	public int _gameNowRound; //현재 라운드.
	public int _gameRound; //게임별 총 라운드.
	public int _targetScore; //제로원 모드용 타겟점수.

	private List<object[]> _param = new List<object[]>();  //데이터 저장용

	private List<WinnerData> _winner = new List<WinnerData>();  //데이터 저장용

	private Dictionary<int, Data>[,] _user;

	private int _turn;

	public UIDartBoardScreen _board;
	public UIScreen_BottomNumber _bottom;
	public GameObject _wait_board;
	private GameObject[] _board_single_hit;
	private GameObject[] _board_double_hit;
	private GameObject[] _board_triple_hit;
	private GameObject[] _board_bull_hit;
	private GameObject _rev_board_hit;

	/// <summary>
	/// AWARD 모음
	/// </summary>
	private GameScoreData[] LEGENDSHOT = new GameScoreData[3] { GameScoreData.DBull, GameScoreData.DBull, GameScoreData.DBull };

	private GameScoreData[][] HATTRICK = new GameScoreData[3][];

	public static readonly int LOWTON_MIN = 100;
	public static readonly int LOWTON_MAX = 150;

	/// <summary>
	/// 팀을 나타내는턴
	/// </summary>
	public int TURN
	{
		get
		{
			return (_turn - 1) % CHECKTURN;
		}
		set
		{
			_turn = value;
		}
	}

	/// <summary>
	/// 실제 유저가 몇번째인지
	/// </summary>
	public int PLAYER
	{
		get
		{
			return ((_userTurn - 1) % _playerCount);
		}
	}

	/// <summary>
	/// 마지막 턴 체크를 위한값 %(나머지자리) 구하는값
	/// 게임대전 방식 별 팀 숫자.
	/// </summary>
	public int CHECKTURN
	{
		get
		{
			return _checkTurn;
		}
		set
		{
			_checkTurn = value;
		}
	}

	public abstract void GameInit();

	public abstract void GameSetPoint(int score, GameScoreData data);

	public abstract void TurnStart(int turn);

	/// <summary>
	/// TODO : 테이블에서 데이터 읽어올수있게 추후 구조변경
	/// </summary>
	/// <returns></returns>
	public bool Init()
	{
		_battle = DataManager.instance.MODEDATA.GAMEBATTLE_DATA;

		switch (DataManager.instance.MODEDATA.GAMEBATTLE_DATA)
		{
			case GameBattleData.Solo:
				_playerCount = 1;
				break;

			case GameBattleData.SoloTwo:
				_playerCount = 2;
				break;

			case GameBattleData.SoloThree:
				_playerCount = 3;
				break;

			case GameBattleData.SoloFour:
			case GameBattleData.TeamTwo:
				_playerCount = 4;
				break;

			case GameBattleData.TeamThree:
				_playerCount = 6;
				break;

			case GameBattleData.TeamFour:
				_playerCount = 8;
				break;
		}

		int rnd = UnityEngine.Random.Range(0, 4);
		switch (rnd)
		{
			case 0:
				UIManager.Instance.OpenPopup<UIVideoPlayer>(Enums_Common.UIRootType.Screen_Background).PlayeVideo("Movie/4");

				break;

			case 1:
				UIManager.Instance.OpenPopup<UIVideoPlayer>(Enums_Common.UIRootType.Screen_Background).PlayeVideo("Movie/11");

				break;

			case 2:
				UIManager.Instance.OpenPopup<UIVideoPlayer>(Enums_Common.UIRootType.Screen_Background).PlayeVideo("Movie/12");

				break;

			case 3:
				UIManager.Instance.OpenPopup<UIVideoPlayer>(Enums_Common.UIRootType.Screen_Background).PlayeVideo("Movie/13");

				break;
		}

		switch (_battle)
		{
			case GameBattleData.Solo:
				CHECKTURN = 1;
				break;

			case GameBattleData.SoloTwo:
			case GameBattleData.TeamTwo:
				CHECKTURN = 2;
				break;

			case GameBattleData.SoloThree:
			case GameBattleData.TeamThree:
				CHECKTURN = 3;
				break;

			case GameBattleData.SoloFour:
			case GameBattleData.TeamFour:
				CHECKTURN = 4;
				break;
		}

		SoundManager.PlayMusic(Enums_Common.SoundType.Dart_Ingame_BGM_Loop1.ToString());
		SoundManager.SetMusicVolume(DataManager.instance.SOUND_DATA.GetValue(SoundType.Dart_Ingame_BGM_Loop1));

		GameManager.instance.ISPLAY = true;
		GameManager.instance.ISENDTURN = false;

		HATTRICK[0] = new GameScoreData[3] { GameScoreData.SBull, GameScoreData.DBull, GameScoreData.DBull };
		HATTRICK[1] = new GameScoreData[3] { GameScoreData.SBull, GameScoreData.SBull, GameScoreData.DBull };
		HATTRICK[2] = new GameScoreData[3] { GameScoreData.DBull, GameScoreData.DBull, GameScoreData.DBull };

		UIDartBoardScreen loadBoard = Resources.Load<UIDartBoardScreen>("Effect/Dartboard_Number");
		_board = Instantiate(loadBoard);

		UIScreen_BottomNumber loadBottom = Resources.Load<UIScreen_BottomNumber>("Effect/Bottom_Number");
		_bottom = Instantiate(loadBottom);

		string[] tableData;
		GameObject load;
		tableData = DataManager.instance.GAME_DATA.GetValue("Single");

		_board_single_hit = new GameObject[tableData.Length];
		for (int i = 0; i < tableData.Length; i++)
		{
			load = Resources.Load<GameObject>("Effect/" + tableData[i]);
			_board_single_hit[i] = Instantiate(load);
		}

		tableData = DataManager.instance.GAME_DATA.GetValue("Double");
		_board_double_hit = new GameObject[tableData.Length];
		for (int i = 0; i < tableData.Length; i++)
		{
			load = Resources.Load<GameObject>("Effect/" + tableData[i]);
			_board_double_hit[i] = Instantiate(load);
		}

		tableData = DataManager.instance.GAME_DATA.GetValue("Triple");
		_board_triple_hit = new GameObject[tableData.Length];
		for (int i = 0; i < tableData.Length; i++)
		{
			load = Resources.Load<GameObject>("Effect/" + tableData[i]);
			_board_triple_hit[i] = Instantiate(load);
		}

		tableData = DataManager.instance.GAME_DATA.GetValue("Bull");
		_board_bull_hit = new GameObject[tableData.Length];
		for (int i = 0; i < tableData.Length; i++)
		{
			load = Resources.Load<GameObject>("Effect/" + tableData[i]);
			_board_bull_hit[i] = Instantiate(load);
		}

		_wait_board = new GameObject();
		load = Resources.Load<GameObject>("Effect/next_turn");
		_wait_board = Instantiate(load);

		return true;
	}

	/// <summary>
	/// 모드별 Init 후 데이터 받아온다음 초기화.
	/// </summary>
	public void AfterInit()
	{
		int div = 1;
		if (_battle >= GameBattleData.TeamTwo)
		{
			div = 2;
		}
		//딕셔너리 초기화.
		Debug.Log("_gameRound -> " + _gameRound);
		_user = new Dictionary<int, Data>[_playerCount / div, _gameRound];
		for (int i = 0; i < _playerCount / div; i++)
		{
			for (int j = 0; j < _gameRound; j++)
			{
				_user[i, j] = new Dictionary<int, Data>();
				_user[i, j].Clear();
			}
		}
	}

	/// <summary>
	/// 다트 맞았을때 들어오는 파티클 함수.
	/// </summary>
	/// <param name="index"></param>
	/// <param name="data"></param>
	public void DartBoardAnimation(int index, GameScoreData data)
	{
		_board.PlayDartBoardAnimation(index);
		_bottom.PlayBottomNumberAnimation(index, data);

		if (_rev_board_hit != null)
		{
			CommonMethod.ParticleStop(_rev_board_hit);
			_rev_board_hit = null;
		}

		int rnd = 0;
		try
		{
			switch (data)
			{
				case GameScoreData.None:
					break;

				case GameScoreData.Single:
					rnd = UnityEngine.Random.Range(0, _board_single_hit.Length);
					_rev_board_hit = _board_single_hit[rnd];
					break;

				case GameScoreData.Double:
					rnd = UnityEngine.Random.Range(0, _board_double_hit.Length);
					_rev_board_hit = _board_double_hit[rnd];
					break;

				case GameScoreData.Triple:
					rnd = UnityEngine.Random.Range(0, _board_triple_hit.Length);
					_rev_board_hit = _board_triple_hit[rnd];

					break;

				case GameScoreData.SBull:
				case GameScoreData.DBull:
					rnd = UnityEngine.Random.Range(0, _board_bull_hit.Length);
					_rev_board_hit = _board_bull_hit[rnd];
					break;
			}

			Animator _hit_anim = null;
			_hit_anim = _rev_board_hit.GetComponent<Animator>();
			if (_hit_anim != null)
			{
				_hit_anim.Play(_hit_anim.name, 0, 0.0f);
			}

			CommonMethod.ParticlePlayer(_rev_board_hit);
		}
		catch
		{
			Debug.Log("Find not Particle");
		}
		SoundManager.PlaySound(SoundType.InGame_FX_ScoreEffect.ToString()).SetVolume(DataManager.instance.SOUND_DATA.GetValue(SoundType.InGame_FX_ScoreEffect));

		switch (data)
		{
			case GameScoreData.None:
				break;

			case GameScoreData.Single:
				SoundManager.PlaySoundUI(SoundType.InGame_FX_Sig_Hit.ToString()).SetVolume(DataManager.instance.SOUND_DATA.GetValue(SoundType.InGame_FX_Sig_Hit));
				break;

			case GameScoreData.Double:
				SoundManager.PlaySoundUI(SoundType.InGame_FX_Dub_Hit.ToString()).SetVolume(DataManager.instance.SOUND_DATA.GetValue(SoundType.InGame_FX_Dub_Hit));
				break;

			case GameScoreData.Triple:
				SoundManager.PlaySoundUI(SoundType.InGame_FX_Tri_Hit.ToString()).SetVolume(DataManager.instance.SOUND_DATA.GetValue(SoundType.InGame_FX_Tri_Hit));
				break;

			case GameScoreData.DBull:
				SoundManager.PlaySoundUI(SoundType.InGame_FX_Bull_Hit.ToString()).SetVolume(DataManager.instance.SOUND_DATA.GetValue(SoundType.InGame_FX_Bull_Hit));
				break;

			case GameScoreData.SBull:
				SoundManager.PlaySoundUI(SoundType.InGame_FX_Bull_Hit.ToString()).SetVolume(DataManager.instance.SOUND_DATA.GetValue(SoundType.InGame_FX_Bull_Hit));
				break;
		}
	}

	public void SetTurnStart(bool isRemove = false)
	{
		UIManager.Instance.ClosePopup<UIInput>();
		int _turn = (_userTurn % _playerCount) + 1;
		if (_userTurn == _gameRound * CHECKTURN)
		{
			GameEnd();
		}
		else
		{
			TURN = _turn;
			UIManager.Instance.ClosePopup<UIAwardPlayer>();
			for (int j = 0; j < _gameRound; j++)
			{
				if (_user[TURN, j].Count < DARTSHOTLENGTH)
				{
					bool isBust = false;
					for (int i = 0; i < DARTSHOTLENGTH; i++)
					{
						if (_user[TURN, j].ContainsKey(i) == true && _user[TURN, j][i].score < 0)
						{
							isBust = true;
						}
					}
					if (isBust == false)
					{
						_gameNowRound = j;
						break;
					}
				}
			}
			if (_userTurn > 0 && isRemove == false)
			{
				UIManager.Instance.OpenPopup<UIIngameMessage>(Enums_Common.UIRootType.Screen_Local).SetIngameMessageState(IngameMessageState.TURN_END);
			}
			else
			{
				if (isRemove == false)
				{
					TurnStart(0);
				}
				else
				{
					TurnStart(_turn - 1);
				}
			}
			_userTurn++;
		}
	}

	public void PlayAwardVideo(bool isEnd = false)
	{
		float time = _userTurn > 0 ? 2.0f : 0.0f;
		GameManager.instance.ISPLAY = _userTurn > 0 ? false : true;
		StartCoroutine(CommonMethod.WaitTime(time, () =>
		{
			UIManager.Instance.OpenPopup<UIAwardPlayer>(Enums_Common.UIRootType.Screen_Local).PlayeVideo("movie/" + GetAward().ToString(), () =>
			{
				if (isEnd)
				{
					GameEnd();
				}
				else
					SetTurnStart();
			});
		}));
	}

	/// <summary>
	/// 게임결과 확인
	/// </summary>
	public void GameEnd()
	{
		GameManager.instance.ISPLAY = false;
		StartCoroutine(CommonMethod.WaitTime(2.0f, () =>
		{
			ScreenEffect.instance.ScreenFadeEffect(ScreenEffect.ScreenEffectType.Screen, Color.red, true, 0, 1f, 1,
		   result =>
		   {
			   UIManager.Instance.OpenPopup<UIResult>(Enums_Common.UIRootType.Kiosk_Local);
			   UIManager.Instance.OpenPopup<UIResultMessage>(Enums_Common.UIRootType.Screen_Local);

			   //   SceneController.instance.GotoScene(Enums_Common.SceneType.Scene_Intro);
		   });
		}));
	}

	/// <summary>
	/// 현재 점수 저장.
	/// </summary>
	/// <param name="score"></param>
	public void AddUserScore(int score, GameScoreData data)
	{
		if (TURN < 0)
			return;
		object[] objList = new object[5];
		for (int i = 0; i < DARTSHOTLENGTH; i++)
		{
			if (_user[TURN, _gameNowRound].ContainsKey(i) == false)
			{
				Data addData = new Data();
				addData.score = score;
				addData.data = data;
				if (DataManager.instance.MODEDATA.GAMEMODE_DATA != Enums_Game.GameModeData.CRICKET)
				{
					addData.isSum = true;
				}
				else
				{
					addData._cricketdata = data;

					if (GetCricketMarkScore(score) < 3)
					{
						addData.isSum = false;
					}
					else
					{
						addData.isSum = true;
					}

					int temp = GetCricketMarkScore(score) + (int)data;
					if (temp > 3 && addData.isSum == false)
					{
						temp = temp - 3;
						addData.isSum = true;
						addData.data = (GameScoreData)temp;
					}
				}
				_user[TURN, _gameNowRound].Add(i, addData);

				objList[(int)ParamEnum.TURN] = TURN;
				objList[(int)ParamEnum.NOWROUND] = _gameNowRound;
				objList[(int)ParamEnum.DARTNUMBER] = i;
				objList[(int)ParamEnum.USERTURN] = _userTurn - 1;
				objList[(int)ParamEnum.ISSUM] = addData.isSum;
				break;
			}
		}
		_param.Add(objList);
	}

	/// <summary>
	/// 현재 유저의 진행하고있는라운드.
	/// </summary>
	public int GetUserRound
	{
		get
		{
			return _gameNowRound;
		}
	}

	/// <summary>
	/// 각 라운드별 3발의 다트 정보
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public string GetUserRoundDart(int index)
	{
		if (_user[TURN, _gameNowRound].ContainsKey(index) == true)
			if (_user[TURN, _gameNowRound][index].score < 0)
			{
				return "0";
			}
			else
			{
				int temp = _user[TURN, _gameNowRound][index].score;

				if (DataManager.instance.MODEDATA.GAMEMODE_DATA == Enums_Game.GameModeData.CRICKET)
				{
					switch (_user[TURN, _gameNowRound][index]._cricketdata)
					{
						case GameScoreData.Double:
							temp = temp * 2;
							break;

						case GameScoreData.Triple:
							temp = temp * 3;
							break;

						case GameScoreData.SBull:
						case GameScoreData.DBull:
							temp = temp * 2;
							break;
					}
				}
				else
				{
					switch (_user[TURN, _gameNowRound][index].data)
					{
						case GameScoreData.Double:
							temp = temp * 2;
							break;

						case GameScoreData.Triple:
							temp = temp * 3;
							break;

						case GameScoreData.SBull:
						case GameScoreData.DBull:
							temp = temp * 2;
							break;
					}
				}
				return Mathf.Abs(temp).ToString();
			}
		else
			return "";
	}

	/// <summary>
	/// 현재 유저가 던진횟수.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public int GetUserThrowCount(int index, int? turn = null)
	{
		int sum = 0;

		if (turn == null)
		{
			turn = TURN;
		}

		for (int i = 0; i < DARTSHOTLENGTH; i++)
		{
			if (_user[(int)turn, index].ContainsKey(i) == true)
				sum += 1;
		}
		return sum;
	}

	/// <summary>
	/// 현재 유저 라운드별 점수합계
	/// </summary>
	/// <returns></returns>
	public int GetUserRoundSum(int index, int? turn = null)
	{
		int sum = 0;
		bool isBust = false;
		if (turn == null)
		{
			turn = TURN;
		}

		for (int i = 0; i < DARTSHOTLENGTH; i++)
		{
			if (_user[(int)turn, index].ContainsKey(i) == true)
			{
				if (_user[(int)turn, index][i].score < 0)
					isBust = true;
				else
				{
					if (_user[(int)turn, index][i].isSum == true)
					{
						int temp = _user[(int)turn, index][i].score;

						switch (_user[(int)turn, index][i].data)
						{
							case GameScoreData.Double:
								temp = temp * 2;
								break;

							case GameScoreData.Triple:
								temp = temp * 3;
								break;

							case GameScoreData.SBull:
								break;

							case GameScoreData.DBull:
								break;
						}
						sum += temp;
					}
				}
			}
		}
		return isBust == false ? sum : 0;
	}

	/// <summary>
	/// 현재 유저의 전체점수합계
	/// </summary>
	/// <returns></returns>
	public int GetUserTotalSum()
	{
		int sum = 0;
		for (int i = 0; i < _gameNowRound + 1; i++)
		{
			int value = GetUserRoundSum(i);
			if (value > 0)
			{
				sum += value;
			}
		}
		return sum;
	}

	/// <summary>
	/// 전체점수
	/// </summary>
	/// <param name="turn"> 특정 유저의 전체점수 </param>
	/// <returns></returns>
	public int GetUserTotalSum(int turn)
	{
		int sum = 0;
		for (int i = 0; i < _gameNowRound + 1; i++)
		{
			int value = GetUserRoundSum(i, turn);
			if (value > 0)
			{
				sum += value;
			}
		}

		switch (DataManager.instance.MODEDATA.GAMEMODE_DATA)
		{
			case Enums_Game.GameModeData.COUNTUP:
				return sum;

			case Enums_Game.GameModeData.ZEROONE:
				return _targetScore - sum;

			case Enums_Game.GameModeData.CRICKET:
				return sum;

			default:
				return 0;
		}
	}

	/// <summary>
	/// 라운드별 Single/Double/Triple 인지 확인
	/// </summary>
	/// <param name="index"> 라운드 </param>
	/// <param name="turn"> 특정 유저의 라운드 데이터 </param>
	/// <returns></returns>
	public GameScoreData[] GetRoundScoreData(int index, int? turn = null)
	{
		GameScoreData[] data = new GameScoreData[DARTSHOTLENGTH];

		if (turn == null)
		{
			turn = TURN;
		}

		for (int i = 0; i < DARTSHOTLENGTH; i++)
		{
			if (_user[(int)turn, index].ContainsKey(i) == true)
			{
				data[i] = _user[(int)turn, index][i]._cricketdata;
			}
		}
		return data;
	}

	/// <summary>
	/// 크리켓용
	/// 마크 점수를 지웟는지 안지웟는지
	/// </summary>
	/// <param name="score"></param>
	/// <param name="turn"></param>
	/// <returns></returns>
	public int GetCricketMarkScore(int score, int? turn = null)
	{
		int value = 0;
		if (turn == null)
		{
			turn = TURN;
		}

		for (int i = 0; i < _gameNowRound + 1; i++)
		{
			for (int j = 0; j < DARTSHOTLENGTH; j++)
			{
				if (_user[(int)turn, i].ContainsKey(j) == true)
				{
					if (_user[(int)turn, i][j].score == score)
					{
						value += (int)_user[(int)turn, i][j]._cricketdata;

						if (value > (int)GameScoreData.Triple)
						{
							break;
						}
					}
				}
			}
		}

		return value;
	}

	/// <summary>
	/// 현재 유저의 PPD 점수.
	/// </summary>
	/// <returns></returns>
	public string GetUserPPD(int? turn = null)
	{
		float sum = 0;
		float count = 0;

		for (int i = 0; i < _gameNowRound + 1; i++)
		{
			int value = GetUserRoundSum(i, turn);
			if (value > 0)
			{
				sum += value;
			}

			count += GetUserThrowCount(i);
		}

		float average = sum / count;

		if (float.IsNaN(average) == true)
		{
			return "00.00";
		}
		else
			return average.ToString("0.00");
	}

	/// <summary>
	/// 유저가 턴이 종료됫는지 안됫는지 확인
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public bool GetUserTurnCheck(int index = 2)
	{
		if (_user[TURN, _gameNowRound].ContainsKey(index) == true)
		{
			return true;
		}
		else
			return false;
	}

	/// <summary>
	/// 이전다트 취소 기능
	/// </summary>
	/// <returns></returns>
	public void RemoveDartScore()
	{
		if (_param.Count == 0)
		{
			Debug.Log("[데이터가 없습니다.]");
			return;
		}
		object[] param = _param[_param.Count - 1];
		_user[(int)param[(int)ParamEnum.TURN], (int)param[(int)ParamEnum.NOWROUND]].Remove((int)param[(int)ParamEnum.DARTNUMBER]);
		_userTurn = (int)param[(int)ParamEnum.USERTURN];
		_param.Remove(param);
		SetTurnStart(true);
		UIManager.Instance.OpenPopup<UIIngameMessage>(Enums_Common.UIRootType.Screen_Local).SetIngameMessageState(IngameMessageState.ACTIVEFALSE);
		Debug.Log("[이전다트취소]");
	}

	/// <summary>
	/// 넘어가기 더미용 버튼
	/// </summary>
	public void DummyButton()
	{
		UIManager.Instance.OpenPopup<UIIngameMessage>(Enums_Common.UIRootType.Screen_Local).SetIngameMessageState();
	}

	/// <summary>
	/// 게임결과에 누가 이겻는지
	/// 플레이어 이름이랑, 플레이어 점수 ,PPD 점수
	/// </summary>
	/// <returns></returns>
	public List<WinnerData> GetWinnerData()
	{
		_winner.Clear();
		int count = GameManager.instance.GetPlayerCount();
		int[] rank = new int[count];

		for (int i = 0; i < count; i++)
		{
			rank[i] = GameManager.instance.GetUserTotalSum(i);
		}

		switch (DataManager.instance.MODEDATA.GAMEMODE_DATA)
		{
			case Enums_Game.GameModeData.COUNTUP:
				rank = CommonMethod.GetCountUpRankCheck(rank);
				break;

			case Enums_Game.GameModeData.ZEROONE:
				rank = CommonMethod.GetZeroOneRankCheck(rank);
				break;
		}
		int div = _battle >= GameBattleData.TeamTwo ? 2 : 1;

		for (int i = 0; i < div; i++)
		{
			_winner.Add(new WinnerData());
		}

		for (int i = 0; i < count; i++)
		{
			if (rank[i] == 1)
			{
				float throwCount = 0;
				for (int j = 0; j < _gameRound; j++)
				{
					_winner[j % div].score += GetUserRoundSum(j, i);
					throwCount += GetUserThrowCount(j, i);
				}

				_winner[0].name = "Player " + (i + 1);
				if (div == 2)
				{
					throwCount = throwCount / 2;
					_winner[1].name = "Player " + (i + CHECKTURN + 1);
				}
				for (int j = 0; j < _winner.Count; j++)
				{
					_winner[j].ppd = (_winner[j].score / throwCount).ToString("0.00");
				}
			}
		}
		return _winner;
	}

	/// <summary>
	/// 어워드 상태 비교.
	/// </summary>
	/// <returns></returns>
	public AWARD GetAward()
	{
		GameScoreData[] award = new GameScoreData[3];

		if (TURN < 0)
			return AWARD.NONE;

		int score = 0;
		for (int i = 0; i < DARTSHOTLENGTH; i++)
		{
			if (_user[TURN, _gameNowRound].ContainsKey(i) == true)
			{
				award[i] = _user[TURN, _gameNowRound][i].data;

				int temp = _user[TURN, _gameNowRound][i].score;

				switch (award[i])
				{
					case GameScoreData.Double:
						temp = temp * 2;
						break;

					case GameScoreData.Triple:
						temp = temp * 3;
						break;

					case GameScoreData.SBull:
					case GameScoreData.DBull:
						temp = temp * 2;
						break;
				}

				score += temp;
			}
		}

		Array.Sort(award);

		if (award.SequenceEqual(HATTRICK[0]) == true || award.SequenceEqual(HATTRICK[1]) == true || award.SequenceEqual(HATTRICK[2]) == true)
		{
			return AWARD.HAT_TRICK;
		}

		if (score >= LOWTON_MIN && score <= LOWTON_MAX)
		{
			return AWARD.LOW_TON;
		}
		return AWARD.NONE;
	}
}