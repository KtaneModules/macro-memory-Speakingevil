using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class MacroMemoryScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombModule module;
    public GameObject[] buttonobjs;
    public KMSelectable[] buttons;
    public Renderer[] leds;
    public Material[] onoff;
    public TextMesh[] labels;

    private static List<int> values = new List<int> { };
    private static List<int[]> presses = new List<int[]> { };
    private static int[] push = new int[3];
    private static int[] selectedmod = new int[2];
    private static bool[] advance;
    private static int rule;
    private static bool pass;
    private static int[] stage;

    private static int moduleIDCounter;
    private int moduleID;
    private static int min;
    private int modInstance;
    private static bool[] moduleSolved;
    private static bool next = true;
    private bool tpon;

    private void Awake()
    {
        labels[0].text = string.Empty;
        for (int i = 0; i < 4; i++)
            buttonobjs[i].transform.localPosition -= new Vector3(0, 0.03f, 0);
        module.OnActivate = delegate () { StartCoroutine(Activate()); };
    }

    private IEnumerator Activate()
    {
        moduleID = ++moduleIDCounter;
        yield return new WaitForSeconds(0.2f);
        if (moduleID == moduleIDCounter)
        {
            values.Clear();
            presses.Clear();
            selectedmod[0] = moduleID - min;
            advance = new bool[selectedmod[0]];
            moduleSolved = new bool[selectedmod[0]];
            stage = new int[selectedmod[0]];
            for (int i = 0; i < 4 * selectedmod[0]; i++)
                values.Add(i);
            values = values.Shuffle();
            selectedmod[1] = Random.Range(0, selectedmod[0]);
            min = moduleID;
        }
        tpon = TwitchPlaysActive;
        modInstance = moduleIDCounter - moduleID;
        yield return new WaitForSeconds(0.2f);
        for(int i = 0; i < 4; i++)
        {
            Label(values[(4 * modInstance) + i] + 1, i + 1);
            buttonobjs[i].transform.localPosition += new Vector3(0, 0.03f, 0);
            yield return new WaitForSeconds(0.1f);
        }
        if(modInstance == selectedmod[1])
        {
            rule = Random.Range(0, 9);
            labels[0].text = (rule + 1).ToString();
            Debug.LogFormat("[Macro Memory #{0}] Activation 1/Stage 1: The displayed digit is {1}.", moduleID, rule + 1);
        }
        foreach (KMSelectable button in buttons)
        {
            int b = Array.IndexOf(buttons, button);
            button.OnInteract = delegate () { if(moduleSolved.Any(x => x == false) && next) Press(values[(4 * modInstance) + b], modInstance, b, stage[selectedmod[1]]); return false; };
        }
    }

    private void Label(int x, int i)
    {
        labels[i].text = x.ToString();
        labels[i].fontSize = 128 / x.ToString().Length;
        Vector3 labelpos = labels[i].transform.localPosition;
        labels[i].transform.localPosition = new Vector3(x > 1 && x < 10 ? 0.002f : 0.0004f, labelpos.y, labelpos.z);
    }

    private void Press(int l, int m, int b, int s)
    {
        buttons[b].AddInteractionPunch();
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, buttons[b].transform);
        push = new int[3] { l, selectedmod[1], b };
        switch (rule)
        {
            case 0:
                switch (s)
                {
                    default: pass = m == selectedmod[1] && b == 0; break;
                    case 1: pass = m == presses.Last()[1] && b == 1; break;
                    case 2: pass = m == presses[presses.Count() - 2][1] && b == values.IndexOf(0) % 4; break;
                    case 3: pass = m == presses[presses.Count() - 2][1] && b == 1; break;
                    case 4: pass = m == presses.Last()[1] && b == values.IndexOf(1) % 4; break;
                }
                break;
            case 1:
                switch (s)
                {
                    default: pass = l == 1; break;
                    case 1: pass = l == presses.Last()[0]; break;
                    case 2: pass = l == presses.First(x => x[1] == selectedmod[1])[0]; break;
                    case 3: pass = l == presses.Where(x => x[1] == selectedmod[1]).ToArray()[1][0]; break;
                    case 4: pass = m == selectedmod[1] && b == presses.First(x => x[1] == selectedmod[1])[2]; break;
                }
                break;
            case 2:
                switch (s)
                {
                    default: pass = m == selectedmod[1] && b == values.IndexOf(2) % 4; break;
                    case 1: pass = m == presses.Last()[1] && b == presses.First(x => x[1] == selectedmod[1])[2]; break;
                    case 2: pass = m == selectedmod[1] && b == 1; break;
                    case 3: pass = l == presses.Where(x => x[1] == selectedmod[1]).ToArray()[2][0]; break;
                    case 4: pass = m == selectedmod[1] && b == values.IndexOf(presses.Last()[0]) % 4; break;
                }
                break;
            case 3:
                switch (s)
                {
                    default: pass = m == selectedmod[1] && b == 3; break;
                    case 1: pass = m == selectedmod[1] && b == values.IndexOf(presses.First(x => x[1] == selectedmod[1])[0]) % 4; break;
                    case 2: pass = m == presses[presses.Count() - 2][1] && b == values.IndexOf(3) % 4; break;
                    case 3: pass = m == values.IndexOf(2) / 4 && b == presses.Where(x => x[1] == selectedmod[1]).ToArray()[1][2]; break;
                    case 4: pass = l == presses[presses.Count() - 4][0]; break;
                }
                break;
            case 4:
                switch (s)
                {
                    default: pass = m == values.IndexOf(1) / 4 && b == 2; break;
                    case 1: pass = m == values.IndexOf(2) / 4 && b == presses.First(x => x[1] == selectedmod[1])[2]; break;
                    case 2: pass = l == presses.Where(x => x[1] == selectedmod[1]).ToArray()[1][0]; break;
                    case 3: pass = m == values.IndexOf(3) / 4 && b == presses.Last()[2]; break;
                    case 4: pass = m == values.IndexOf(0) / 4 && b == presses.Where(x => x[1] == selectedmod[1]).ToArray()[2][2]; break;
                }
                break;
            case 5:
                switch (s)
                {
                    default: pass = l == 2; break;
                    case 1: pass = m == selectedmod[1] && b == presses.First(x => x[1] == selectedmod[1])[2]; break;
                    case 2: pass = m == selectedmod[1] && b == presses[presses.Count() - 2][2]; break;
                    case 3: pass = m == presses[presses.Count() - 3][1] && b == presses.First(x => x[1] == selectedmod[1])[2]; break;
                    case 4: pass = l == presses.First(x => x[1] == selectedmod[1])[0]; break;
                }
                break;
            case 6:
                switch (s)
                {
                    default: pass = m == selectedmod[1] && b == 1; break;
                    case 1: pass = l == presses.First(x => x[1] == selectedmod[1])[0]; break;
                    case 2: pass = m == selectedmod[1] && b == presses.First(x => x[1] == selectedmod[1])[2]; break;
                    case 3: pass = m == selectedmod[1] && b == values.IndexOf(presses[presses.Count() - 3][0]) % 4; break;
                    case 4: pass = l == presses[presses.Count() - 3][0]; break;
                }
                break;
            case 7:
                switch (s)
                {
                    default: pass = m == selectedmod[1] && b == values.IndexOf(0) % 4; break;
                    case 1: pass = m == selectedmod[1] && b == values.IndexOf(presses.Last()[0]) % 4; break;
                    case 2: pass = m == presses.Last()[1] && b == 1; break;
                    case 3: pass = m == presses[presses.Count() - 2][1] && b == values.IndexOf(0) % 4; break;
                    case 4: pass = m == values.IndexOf(presses.First(x => x[1] == selectedmod[1])[0]) / 4 && b == 3; break;
                }
                break;
            default:
                switch (s)
                {
                    default: pass = m == values.IndexOf(3) / 4 && b == 0; break;
                    case 1: pass = m == values.IndexOf(presses.First(x => x[1] == selectedmod[1])[0]) / 4 && b == 3; break;
                    case 2: pass = m == selectedmod[1] && b == presses[presses.Count() - 2][2]; break;
                    case 3: pass = m == values.IndexOf(presses.Where(x => x[1] == selectedmod[1]).ToArray()[2][0]) / 4 && b == values.IndexOf(2) % 4; break;
                    case 4: pass = m == selectedmod[1] && b == values.IndexOf(presses[presses.Count() - 2][0]) % 4; break;
                }
                break;
        }
        next = false;
        advance = advance.Select(x => true).ToArray();
    }

    private void Update()
    {
        if (advance != null && advance[modInstance])
        {
            advance[modInstance] = false;
            StartCoroutine(Advance());
        }
    }

    private IEnumerator Advance()
    {
        if(!moduleSolved[modInstance])
            Debug.LogFormat("[Macro Memory #{0}] Button {1} in position {2} on module {3} pressed. {4}.", moduleID, push[0] + 1, push[2] + 1, moduleIDCounter - push[1], pass ? "Correct" : "Incorrect");
        if (modInstance == selectedmod[1])
        {
            labels[0].text = string.Empty;
            if (pass)
            {
                presses.Add(push);
                leds[stage[modInstance]].material = onoff[1];
                stage[modInstance]++;
                if (stage[modInstance] > 4)
                {
                    moduleSolved[modInstance] = true;
                    Debug.LogFormat("[Macro Memory #{0}] Module solved.", moduleID);
                    if (!tpon)
                        module.HandlePass();
                }
                else
                    Debug.LogFormat("[Macro Memory #{0}] Advancing to stage {1}.", moduleID, stage[modInstance] + 1);
            }
            else
            {
                module.HandleStrike();
                presses.Clear();
            }
        }
        else if (pass && !moduleSolved[modInstance])
        {
            if (stage[selectedmod[1]] > 4)
                Debug.LogFormat("[Macro Memory #{0}] Module {1} solved.", moduleID, moduleIDCounter - selectedmod[1]);
            else
                Debug.LogFormat("[Macro Memory #{0}] Module {2} advances to stage {1}.", moduleID, stage[selectedmod[1]] + 1, moduleIDCounter - selectedmod[1]);
        }
        for (int i = 0; i < 4; i++)
        {
            yield return new WaitForSeconds(0.1f);
            buttonobjs[i].transform.localPosition -= new Vector3(0, 0.03f, 0);
        }
        if (moduleSolved.Any(x => x == false))
        {
            if (!pass && !moduleSolved[modInstance])
            {
                leds[stage[modInstance]].material = onoff[1];
                stage[modInstance] = 0;
            }
            if (modInstance == selectedmod[1])
            {
                if (!moduleSolved[modInstance])
                    leds[stage[modInstance]].material = onoff[0];
                values = values.Shuffle();
                selectedmod[1] = Random.Range(0, selectedmod[0]);
                rule = Random.Range(0, 9);
                while (moduleSolved[selectedmod[1]])
                    selectedmod[1] = Random.Range(0, selectedmod[0]);
                next = true;
            }
            if (!pass && !moduleSolved[modInstance])
            {
                leds[stage[modInstance]].material = onoff[0];
                Debug.LogFormat("[Macro Memory #{0}] Memory deleted. Reset to stage 1.", moduleID);
            }
            while (!next)
                yield return null;
            yield return new WaitForSeconds(0.2f);
            for (int i = 0; i < 4; i++)
            {
                Label(values[(4 * modInstance) + i] + 1, i + 1);
                buttonobjs[i].transform.localPosition += new Vector3(0, 0.03f, 0);
                yield return new WaitForSeconds(0.1f);
            }
            if (modInstance == selectedmod[1])
            {
                labels[0].text = (rule + 1).ToString();
                Debug.LogFormat("[Macro Memory #{0}] Activation {1}/Stage {2}: The displayed digit is {3}.", moduleID, presses.Count() + 1, stage[modInstance] + 1, rule + 1);
            }
            else if(!moduleSolved[modInstance])
                Debug.LogFormat("[Macro Memory #{0}] Activation {1}: The digit {2} is displayed on module {3}.", moduleID, presses.Count() + 1, rule + 1, moduleIDCounter - selectedmod[1]);
        }
        else
            next = true;
    }

    bool TwitchPlaysActive;

#pragma warning disable 414
    private string TwitchHelpMessage = "!{0} <1-4> [Presses button in the specified position.] !{0} done [Solves module once all stages are complete on every module.]";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string command)
    {
        if(command == "done")
        {
            if(moduleSolved.Any(x => x == false))
            {
                yield return "sendtochaterror!f Incomplete Macro Memory module detected.";
                yield break;
            }
            yield return "solve";
            module.HandlePass();
            yield break;
        }
        if (moduleSolved[modInstance])
        {
            yield return "sendtochaterror!f Module is already complete. Use the \"done\" command once all other Macro Memory modules are complete to solve.";
            yield break;
        }
        int b = Array.IndexOf(new string[] { "1", "2", "3", "4" }, command);
        if(b < 0)
        {
            yield return "sendtochaterror!f " + command + "is not a possible position.";
            yield break;
        }
        bool anim = next;
        if (!next)
            yield return true;
        if (anim)
            yield return new WaitForSeconds(0.5f);
        else
            yield return null;
        yield return "strike";
        buttons[b].OnInteract();
    }
}
