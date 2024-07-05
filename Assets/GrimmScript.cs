using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GrimmScript : MonoBehaviour {

	public KMAudio mAudio;
	public KMBombModule modSelf;
	public TextMesh resultMesh, centerNumMesh;
	public TextMesh[] outsideNumMesh;
	public KMSelectable yesBtn, noBtn;

	static int modIDCnt;
	int moduleID;
	List<int> numsToViewOutside;
	int centerNumToView, bottomNumToView;

	bool expectedYes = false, moduleSolved, started;

	void QuickLog(string toLog, params object[] args)
    {
		Debug.LogFormat("[{0} #{1}] {2}", modSelf.ModuleDisplayName, moduleID, string.Format(toLog,args));
	}
	// Use this for initialization
	void Start () {
		moduleID = ++modIDCnt;
		ResetModule();
		yesBtn.OnInteract += delegate {
			HandlePress(true);
			return false;
		};
		noBtn.OnInteract += delegate {
			HandlePress(false);
			return false;
		};
		modSelf.OnActivate += ActivateModuleDisplays;
	}
	void HandlePress(bool isYesButton)
    {
		if (moduleSolved || !started) return;
		(isYesButton ? yesBtn : noBtn).AddInteractionPunch();
		mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, (isYesButton ? yesBtn : noBtn).transform);
		if (isYesButton ^ expectedYes)
        {
			QuickLog("Incorrect button pressed! ({0} pressed) Starting over...", isYesButton ? "YES" : "NO");
			modSelf.HandleStrike();
			ResetModule();
		}
		else
        {
			QuickLog("Expected button correctly pressed.");
			mAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
			moduleSolved = true;
			modSelf.HandlePass();
        }
    }
	void ActivateModuleDisplays()
    {
		for (var x = 0; x < outsideNumMesh.Length; x++)
			outsideNumMesh[x].text = numsToViewOutside[x].ToString();
		centerNumMesh.text = centerNumToView.ToString();
		resultMesh.text = bottomNumToView.ToString();
		started = true;
	}

	void ResetModule()
    {
		centerNumToView = Random.Range(1, 10);
		numsToViewOutside = new List<int>();
        for (var x = 0; x < 3; x++)
			numsToViewOutside.Add(Random.Range(0, 10));
		var currentValuesTimesCenterNum = new List<int>();
		foreach (var curNumOutside in numsToViewOutside)
        {
            currentValuesTimesCenterNum.Add((curNumOutside + centerNumToView) * centerNumToView);
			currentValuesTimesCenterNum.Add((curNumOutside - centerNumToView) * centerNumToView);
			currentValuesTimesCenterNum.Add(curNumOutside * centerNumToView * centerNumToView);
			currentValuesTimesCenterNum.Add(curNumOutside);
        }
		var finalResult = 0;
		foreach (var value in currentValuesTimesCenterNum.OrderByDescending(a => a))
        {
			if (finalResult < value)
				finalResult += value;
			else
				finalResult -= value;
        }
		finalResult /= centerNumToView;

		var finalResultMod10 = finalResult % 10;
		expectedYes = Random.value < 0.5f;
		bottomNumToView = expectedYes ? finalResultMod10
			: Enumerable.Range(0, 10).Where(a => a != finalResultMod10).PickRandom();
		QuickLog("Displayed digits to the left, right and above the grimm symbol are {0}.", numsToViewOutside.Join());
		QuickLog("Displayed digit inside the grimm symbol is {0}.", centerNumToView);
		QuickLog("Applying the grimm operation results in {0}.", finalResultMod10);
		QuickLog("Press {0} because the bottom display is {1}.", expectedYes ? "YES" : "NO", bottomNumToView);
		if (started)
		{
			for (var x = 0; x < outsideNumMesh.Length; x++)
				outsideNumMesh[x].text = numsToViewOutside[x].ToString();
			centerNumMesh.text = centerNumToView.ToString();
			resultMesh.text = bottomNumToView.ToString();
		}
	}
	#region Twitch Plays
#pragma warning disable 414
	private readonly string TwitchHelpMessage = "Press Yes with '!{0} press Yes'. Press No with '!{0} press No'.";
	//private readonly string TwitchManualCode = "Digital Root";
#pragma warning restore 414
	public KMSelectable[] ProcessTwitchCommand(string command)
	{
		if (command.Equals("press no", System.StringComparison.InvariantCultureIgnoreCase))
			return new KMSelectable[] { noBtn };
		else if (command.Equals("press yes", System.StringComparison.InvariantCultureIgnoreCase))
			return new KMSelectable[] { yesBtn };
		else
			return null;
	}
	private IEnumerator TwitchHandleForcedSolve()
	{
		if (!moduleSolved)
		{
			yield return null;
			(expectedYes ? yesBtn : noBtn).OnInteract();
		}
	}
    #endregion
}
