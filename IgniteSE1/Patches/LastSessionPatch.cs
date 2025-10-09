using HarmonyLib;
using Sandbox.Engine.Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.News.NewContentNotification;

namespace IgniteSE1.Patches
{

    [HarmonyPatch]
    public class LastSessionPatch
    {


        //Patch to prevent lastsession file from being created. WE handle this ourselves
        [HarmonyPrefix]
        [HarmonyPatch(typeof(MyLocalCache), "SaveLastSessionInfo", new Type[] { typeof(string), typeof(bool), typeof(bool), typeof(string), typeof(string), typeof(int) })]
        private static bool LoadLastSession_Prefix()
        {
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MyNewContentNotificationsBase), "SaveViewedContentInfo", new Type[] { })]
        private static bool SaveViewedContentInfo_Prefix()
        {
            //Prevent keen from saving viewed content info
            return false;
        }
    }
}
