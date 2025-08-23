using UnityEngine;
using System.Text;
using Zenject;

public class LogicTimer : MonoBehaviour {
	public float timeInterval = 0.35f; //save
	public float randomMin = 5f; //save
	public float randomMax = 10f; //save
	public bool useRandomTimes = false; //save
	public bool active = true; //save
	[HideInInspector] public float intervalFinished; //save
	public string target; //save
	public string argvalue; //save
	private static StringBuilder s1 = new StringBuilder(100 *200);

	[Inject] private Const _consts;
	[Inject] private PauseScript _pauseScript;

	void Start() {
		intervalFinished = _pauseScript.relativeTime + (useRandomTimes ? Random.Range(randomMin,randomMax) : timeInterval);
	}

	void Update() {
		if (!_pauseScript.Paused() && !_pauseScript.MenuActive() && active) {
			if (intervalFinished < _pauseScript.relativeTime) {
				if (useRandomTimes) {
					intervalFinished = _pauseScript.relativeTime
									   + Random.Range(randomMin,randomMax);
				} else {
					intervalFinished = _pauseScript.relativeTime
									   + timeInterval;
				}
				UseTargets();
			}
		}
	}

	public void Targetted (UseData ud) {
		active = !active;
	}

	public void UseTargets () {
		UseData ud = new UseData();
		ud.argvalue = argvalue;
		_consts.UseTargets(gameObject,ud,target);
	}

	public static string Save(GameObject go) {
		LogicTimer lt = go.GetComponent<LogicTimer>();
		s1.Clear();
		s1.Append(Utils.SaveRelativeTimeDifferential(lt._pauseScript,lt.intervalFinished,"intervalFinished"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(lt.timeInterval,"timeInterval"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(lt.randomMin,"randomMin"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.FloatToString(lt.randomMax,"randomMax"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(lt.useRandomTimes,"useRandomTimes"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.BoolToString(lt.active,"active"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveString(lt.target,"target"));
		s1.Append(Utils.splitChar);
		s1.Append(Utils.SaveString(lt.argvalue,"argvalue"));
		return s1.ToString();
	}

	public static int Load(GameObject go, ref string[] entries, int index) {
		LogicTimer lt = go.GetComponent<LogicTimer>();
		lt.intervalFinished = Utils.LoadRelativeTimeDifferential(lt._pauseScript,entries[index],"intervalFinished"); index++;
		lt.timeInterval = Utils.GetFloatFromString(entries[index],"timeInterval"); index++;
		lt.randomMin = Utils.GetFloatFromString(entries[index],"randomMin"); index++;
		lt.randomMax = Utils.GetFloatFromString(entries[index],"randomMax"); index++;
		lt.useRandomTimes = Utils.GetBoolFromString(entries[index],"useRandomTimes"); index++;
		lt.active = Utils.GetBoolFromString(entries[index],"active"); index++;
		lt.target = Utils.LoadString(entries[index],"target"); index++;
		lt.argvalue = Utils.LoadString(entries[index],"argvalue"); index++;
		return index;
	}
}
