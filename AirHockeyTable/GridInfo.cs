using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        //---------------------------------------------------------------//
        // Grid Info                                                     //
        //---------------------------------------------------------------//
        // holds some basic info about the grid and other useful stuff   //
        // to have globally can also report changes to variables and     //
        // send them to other grids.                                     //
        //---------------------------------------------------------------//
        // add to Program():                                             //
        // GridInfo.Init("Program Name",GridTerminalSystem,IGC,Me,Echo); //
        // if(Storage != "") GridInfo.Load(Storage);                     //
        //                                                               //
        // add to Save():                                                //
        // GridInfo.Save();                                              //
        //                                                               //
        // add to Main():                                                //
        // GridInfo.CheckMessages();                                     //
        //                                                               //
        // usage:                                                        //
        // GridInfo.SetVar("varname","value");                           //
        // GridInfo.GetVarAs<T>("varname","optionalDefault");            //
        //                                                               //
        // change listener:                                              //
        // GridInfo.AddVarChangedHandler("varname",MyHandler);           //
        //                                                               //
        // change broadcasting:                                          //
        // GridInfo.AddChangeBroadcaster("program","varname");           //
        // GridInfo.AddChangeUnicaster(igcAddress,"varname");            //
        //---------------------------------------------------------------//
        public class GridInfo
        {
            public static long RunCount = 0; // to store how many times the script has run since compiling
            public static string ProgramName = "Program"; // the name of the program
            public static IMyGridTerminalSystem GridTerminalSystem; // so it can be globally available
            public static IMyIntergridCommunicationSystem IGC; // so it can be globally available
            public static IMyProgrammableBlock Me; // so it can be globally available... lol
            public static Action<string> EchoAction; // EchoAction?.Invoke("hello");
            private static IMyBroadcastListener broadcastListener; // so it can be globally available
            private static List<IMyBroadcastListener> listeners = new List<IMyBroadcastListener>(); // so it can be globally available
            private static string bound_vars = ""; // a list of vars that have been bound to the grid
            private static Dictionary<string, string> broadcast_vars = new Dictionary<string, string>(); // a list of vars that have been bound to the grid
            private static Dictionary<string, long> unicast_vars = new Dictionary<string, long>(); // a list of vars that have been bound to the grid
            public static bool handleUnicastMessages = false;
            public static void Echo(string message)
            {
                EchoAction?.Invoke(message);
            }
            public static Dictionary<string, string> GridVars = new Dictionary<string, string>();
            //-------------------------------------------//
            // setup GridInfo                            //
            //-------------------------------------------//
            public static void Init(string name, IMyGridTerminalSystem gts, IMyIntergridCommunicationSystem igc, IMyProgrammableBlock me, Action<string> echo)
            {
                ProgramName = name;
                GridTerminalSystem = gts;
                IGC = igc;
                Me = me;
                EchoAction = echo;
                broadcastListener = IGC.RegisterBroadcastListener(ProgramName);
                Me.CustomName = "Program: " + ProgramName + " @" + IGC.Me.ToString();
            }

            //-------------------------------------------//
            // handle broadcast messages                 //
            //-------------------------------------------//
            public static List<MyIGCMessage> CheckMessages()
            {
                List<MyIGCMessage> messages = new List<MyIGCMessage>();
                while (broadcastListener.HasPendingMessage)
                {
                    MyIGCMessage message = broadcastListener.AcceptMessage();
                    Echo(message.Tag + ": " + message.As<string>());
                    string[] data = message.As<string>().Split('║');
                    if (data.Length == 2)
                    {
                        SetVar(data[0], data[1]);
                    }
                    else messages.Add(message);
                }
                while (IGC.UnicastListener.HasPendingMessage)
                {
                    messages.Add(IGC.UnicastListener.AcceptMessage());
                }
                foreach (IMyBroadcastListener listener in listeners)
                {
                    while (listener.HasPendingMessage)
                    {
                        messages.Add(listener.AcceptMessage());
                    }
                }
                return messages;
            }
            public static IMyBroadcastListener AddBroadcastListener(string name)
            {
                IMyBroadcastListener listener = IGC.RegisterBroadcastListener(name);
                listeners.Add(listener);
                return listener;
            }
            //-------------------------------------------//
            // Get a var as a specific type of variable  //
            //                                           //
            // key - the id of the variable to get       //
            // defaultValue - the value to return if     //
            //                the variable doesn't exist //
            //-------------------------------------------//    
            public static T GetVarAs<T>(string key, T defaultValue = default(T))
            {
                if (!GridVars.ContainsKey(key)) return defaultValue; //(T)Convert.ChangeType(null,typeof(T));
                return (T)Convert.ChangeType(GridVars[key], typeof(T));
            }
            public static Vector3D GetVarAsVector3D(string key, Vector3D defaultValue = default(Vector3D))
            {
                if (!GridVars.ContainsKey(key)) return defaultValue;
                string[] data = GridVars[key].Split(',');
                if (data.Length == 3)
                {
                    double x = double.Parse(data[0]);
                    double y = double.Parse(data[1]);
                    double z = double.Parse(data[2]);
                    return new Vector3D(x, y, z);
                }
                return defaultValue;
            }
            //-------------------------------------------//
            // set a grid info var                       //
            //                                           //
            // key - the id of the variable to set       //
            // value - the value (converted to a string) //
            //-------------------------------------------//
            public static void SetVar(string key, string value)
            {
                if (GridVars.ContainsKey(key)) GridVars[key] = value;
                else GridVars.Add(key, value);
                if (bound_vars.Contains(key + "║")) OnVarChanged(key, value);
                if (broadcast_vars.ContainsKey(key)) IGC.SendBroadcastMessage(broadcast_vars[key], key + "║" + value);
                if (unicast_vars.ContainsKey(key)) IGC.SendUnicastMessage(unicast_vars[key], key, value);
            }
            public static void SetVar(string key, Vector3D value)
            {
                SetVar(key, value.X.ToString() + "," + value.Y.ToString() + "," + value.Z.ToString());
            }
            //------------------------------------------------------------//
            // converts the grid info vars to a string to save in Storage //
            //------------------------------------------------------------//
            public static string Save()
            {
                StringBuilder storage = new StringBuilder();
                foreach (KeyValuePair<string, string> var in GridVars)
                {
                    storage.Append(var.Key + "║" + var.Value + "\n");
                }
                return storage.ToString();
            }
            //----------------------------------------------//
            // parse the Storage string into grid info vars //
            //----------------------------------------------//
            public static void Load(string storage)
            {
                string[] lines = storage.Split('\n');
                foreach (string line in lines)
                {
                    string[] var = line.Split('║');
                    if (var.Length == 2)
                    {
                        GridVars.Add(var[0], var[1]);
                    }
                }
            }
            //----------------------------------//
            // event for when a var is changed  //
            //----------------------------------//
            public static event Action<string, string> VarChanged;
            private static void OnVarChanged(string key, string value)
            {
                VarChanged?.Invoke(key, value);
            }
            public static void AddChangeListener(string key, Action<string, string> handler)
            {
                bound_vars += key + "║";
                VarChanged += handler;
            }
            // send changes to a prog by its name
            public static void AddChangeBroadcaster(string progName, string key)
            {
                broadcast_vars.Add(key, progName);
            }
            // send changes to a prog by its igc address
            public static void AddChangeUnicaster(string key, long id)
            {
                unicast_vars.Add(key, id);
                handleUnicastMessages = true;
            }

            //----------------------------------//
            // the world position for the block //
            //----------------------------------//
            public static Vector3D BlockWorldPosition(IMyFunctionalBlock block, Vector3D offset = new Vector3D())
            {
                return Vector3D.Transform(offset, block.WorldMatrix);
            }
        }
        //---------------------------------------------------------------//
        // GridInfo End                                                  //
        //---------------------------------------------------------------//
    }
}
