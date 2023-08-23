using UnityEngine;
using System.Collections.Generic;
using System;
using TMPro;

public class LSConsole : MonoBehaviour, ILogHandler
{
    public static LSConsole instance;

    public GameObject loggerCanvas;
    public TMP_InputField logText;
    public TMP_InputField input;
    public TextMeshProUGUI fpsText;
    public UnityEngine.UI.Image bg;
    public bool useLogger = true;

    bool _visible = false;
    bool selectNextFrame = false;
    bool showFPS = false;

    public delegate string CommandAction(params string[] parameters);
    public struct Command
    {
        public string commandName;
        public string helpText;
        public CommandAction action;
    }

    static SortedDictionary<string, Command> commandList = new SortedDictionary<string, Command>();

    public static bool visible { get { if (instance == null) return false; return instance._visible;  } set
        {
            instance._visible = value;
            instance.bg.enabled = value;
            instance.loggerCanvas.SetActive(value);
            instance.logText.gameObject.SetActive(value);// = new Color(1f, 1f, 1f, (value ? 1f : 0f));
            instance.input.gameObject.SetActive(value);
            if (value)
                instance.selectNextFrame = true;
        }
    }
    private ILogHandler m_DefaultLogHandler = Debug.unityLogger.logHandler;

    public void LogException(Exception exception, UnityEngine.Object context)
    {
        //m_DefaultLogHandler.LogException(exception, context);
        //logText.text += "error";
        Log((context == null ? "unknown exception" : context.name) + ":" + (exception == null ? "" : exception.ToString()), true);
    }

    public static void Log(string message, bool isError)
    {
        if (isError)
            instance.logText.text = "<color=red>" + message + "</color>\n" + instance.logText.text;
        else
            instance.logText.text = message + "\n" + instance.logText.text;

    }
    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
        Log(String.Format(format, args), false);
    }

    string ShowCommandList(string[] parameters)
    {
        if (parameters.Length > 0)
        {
            if (commandList.ContainsKey(parameters[0]))
                Log(parameters[0] + ": " + commandList[parameters[0]].helpText, false);
            else
                return "unknown command: " + parameters[0];
        }
        else
        {
            var txt = "Command List: \n";
            foreach (var key in commandList.Keys)
            {
                txt += "  " + key + ": " + commandList[key].helpText + "\n";
            }
            Log(txt, false);
        }
        return "";
    }
    private void Awake()
    {
		instance = this;
        if (useLogger)
            Debug.unityLogger.logHandler = this;

        AddCommandHook("help", "shows this list of commands. [brackets] denote parameters", ShowCommandList);
        AddCommandHook("fps", "shows FPS while console is hidden, or set [frameRate]", (p) => {
            if (p.Length == 0) showFPS = !showFPS;
            else Application.targetFrameRate = int.Parse(p[0]);
            return "";
        });
        
    }
    public static void AddCommandHook(string cmdName, string cmdHelp, CommandAction cmdAction)
    {
        if (instance == null || !instance.useLogger) return;
        if (commandList.ContainsKey(cmdName))
        {
            Log("Error adding duplicate command: " + cmdName, true);
            return;
        }
        var cmd = new Command { commandName = cmdName, helpText = cmdHelp, action = cmdAction };
        commandList.Add(cmdName, cmd);

    }
    // Use this for initialization
    void Start()
	{
        visible = false;
	}

	// Update is called once per frame
	void Update()
	{
        if (visible || showFPS)
            fpsText.text = Mathf.FloorToInt(FPSCalc.fps) + " fps";
        else if (fpsText.text != "")
            fpsText.text = "";

        if (!useLogger) return;
        if(selectNextFrame)
        {
            //delete backtick from invoking logger
            if (input.text.Length > 0 && input.text[input.text.Length - 1] == '`')
                input.text = input.text.Substring(0, input.text.Length - 1);
            input.ActivateInputField();
            selectNextFrame = false;
        }
        if (Input.GetKeyDown(KeyCode.BackQuote))
            visible = !visible;
        if(visible && Input.GetKeyDown(KeyCode.Return))
        {
            string command = input.text;
            input.text = "";
            
            if (command == "") return;
            var vars = command.Split(' ');
            if(commandList.ContainsKey(vars[0]))
            {
                var cmd = commandList[vars[0]];
                string errorText;
                if (vars.Length == 1)
                    errorText = cmd.action.Invoke(new string[0]);
                else
                    errorText = cmd.action.Invoke(vars[1..vars.Length]);
                if(errorText != "")
                {
                    Log(vars[0] + ": " + errorText, true);
                }
            } else
            {
                Log("Unknown command: " + vars[0], true);
            }

            selectNextFrame = true;
        }
	}
}

