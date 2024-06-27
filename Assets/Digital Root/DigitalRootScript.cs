using System.Collections;
using UnityEngine;

public class DigitalRootScript : MonoBehaviour {
	#region Global Variables
	public KMAudio Audio;
	public KMBombModule Module;
	public KMRuleSeedable RuleSeedable;

	public KMSelectable YesButton, NoButton;
	public TextMesh Screen1, Screen2, Screen3, AnswerScreen;

	private static int _moduleIdCounter = 1;
	private int _moduleId = 0;

	private int Screen1Num, Screen2Num, Screen3Num;
	private bool DigitalRoot;

	private KMSelectable YesAns, NoAns;
	private MonoRandom rnd;

	private bool _isSolved = false, _lightsOn = false;
	#endregion

	#region Answer Calculation
	private void Start ()
	{
		rnd = RuleSeedable.GetRNG();
		int seed = rnd.Seed;
		if (seed == 1)
		{
			YesAns = YesButton;
			NoAns = NoButton;
			UsesVanillaRuleModifierAPI = false;
		}
		else
		{
			int yes = rnd.Next(0, 2);
			int no = rnd.Next(0, 2);
			switch (yes)
			{
				case 0:
					YesAns = NoButton;
					break;
				default:
					YesAns = YesButton;
					break;
			}

			switch (no)
			{
				case 0:
					NoAns = YesButton;
					break;
				default:
					NoAns = NoButton;
					break;
			}

			UsesVanillaRuleModifierAPI = true;
		}
		_moduleId = _moduleIdCounter++;
		Module.OnActivate += Activate;
	}

	private void Activate()
	{
		Init();
		_lightsOn = true;
	}

	void Init()
	{
		int random = Random.Range(0, 2);
		Screen1Num = Random.Range(0, 10);
		Screen2Num = Random.Range(0, 10);
		Screen3Num = Random.Range(0, 10);
		int AnswerNum;
		DigitalRoot = random == 1;
		int value = (((Screen1Num * 100 + Screen2Num * 10 + Screen3Num) - 1) % 9) + 1;
		if (DigitalRoot)
		{
			AnswerNum = value;
		}
		else
		{
			AnswerNum = Random.Range(0, 10);
			while (AnswerNum == value)
			{
				AnswerNum = Random.Range(0, 10);
			}
		}
		Screen1.text = Screen1Num.ToString();
		Screen2.text = Screen2Num.ToString();
		Screen3.text = Screen3Num.ToString();
		AnswerScreen.text = AnswerNum.ToString();
		Debug.LogFormat("[Digital Root #{0}] The numbers are {1}, {2} and {3}", _moduleId, Screen1Num, Screen2Num, Screen3Num);
		Debug.LogFormat("[Digital Root #{0}] The answer is {1}", _moduleId, AnswerNum);
		Debug.LogFormat("[Digital Root #{0}] Digital root: {1}", _moduleId, DigitalRoot);
		Debug.LogFormat("[Digital Root #{0}] Press the button labeled \"{1}\"", _moduleId, DigitalRoot && YesAns == YesButton || !DigitalRoot && NoAns == YesButton ? "YES" : "NO");
	}
	#endregion

	#region Button Handling
	private void Awake()
	{
		YesButton.OnInteract += delegate ()
		{
			YesHandler();
			return false;
		};
		NoButton.OnInteract += delegate ()
		{
			NoHandler();
			return false;
		};
	}

	private void YesHandler()
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, YesButton.transform);
		YesButton.AddInteractionPunch();
		if (!_lightsOn || _isSolved) return;
		Debug.LogFormat("[Digital Root #{0}] Button with the label \"YES\" pressed", _moduleId);
		if (DigitalRoot && YesAns == YesButton || !DigitalRoot && NoAns == YesButton)
		{
			Debug.LogFormat("[Digital Root #{0}] Module Passed!", _moduleId);
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, YesButton.transform);
			Module.HandlePass();
			_isSolved = true;
		} else
		{
			Debug.LogFormat("[Digital Root #{0}] Strike!", _moduleId);
			Module.HandleStrike();
		}
	}

	private void NoHandler()
	{
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, NoButton.transform);
		NoButton.AddInteractionPunch();
		if (!_lightsOn || _isSolved) return;
		Debug.LogFormat("[Digital Root #{0}] Button with the label \"NO\" pressed", _moduleId);
		if (DigitalRoot && YesAns == NoButton || !DigitalRoot && NoAns == NoButton)
		{
			Debug.LogFormat("[Digital Root #{0}] Module Passed!", _moduleId);
			Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, NoButton.transform);
			Module.HandlePass();
			_isSolved = true;
		}
		else
		{
			Debug.LogFormat("[Digital Root #{0}] Strike!", _moduleId);
			Module.HandleStrike();
		}
	}
	#endregion

	#region Twitch Plays
#pragma warning disable 414
	private readonly string TwitchHelpMessage = "Press Yes with '!{0} press Yes'. Press No with '!{0} press No'.";
	private readonly string TwitchManualCode = "Digital Root";
	private bool UsesVanillaRuleModifierAPI;
#pragma warning restore 414
	public KMSelectable[] ProcessTwitchCommand(string command)
	{
		if (command.Equals("press no", System.StringComparison.InvariantCultureIgnoreCase))
			return new KMSelectable[] { NoButton };
		else if (command.Equals("press yes", System.StringComparison.InvariantCultureIgnoreCase))
			return new KMSelectable[] { YesButton };
		else
			return null;
	}
	private IEnumerator TwitchHandleForcedSolve()
	{
		if (!_isSolved)
		{
			yield return null;
			Debug.LogFormat("[Digital Root #{0}] Module forcibly solved", _moduleId);
			if (DigitalRoot && YesAns == YesButton || !DigitalRoot && NoAns == YesButton) YesHandler();
			else NoHandler();
		}
	}
	#endregion
}
