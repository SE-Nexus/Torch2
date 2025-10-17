using HarmonyLib;
using IgniteUtils.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IgniteUtils.Services
{

    public class PatchService : ServiceBase
    {
        public Harmony Harmony { get; set; }

        public PatchService()
        {
            Harmony = new Harmony("Torch2");

            //Clean this up
            var assembly = Assembly.GetExecutingAssembly();

            PatchAll(Harmony, assembly);
        }

        public override Task<bool> Init()
        {
            return base.Init();
        }

        public static void PatchAll(Harmony harmony, Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                // Only consider classes that have [HarmonyPatch] on them
                if (type.GetCustomAttributes(typeof(HarmonyPatch), true).Length == 0)
                    continue;

                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    var patchAttrs = method.GetCustomAttributes(typeof(HarmonyPatch), true);
                    if (patchAttrs.Length == 0)
                        continue;

                    foreach (HarmonyPatch patchAttr in patchAttrs)
                    {
                        try
                        {
                            var targetType = patchAttr.info.declaringType;
                            var targetMethodName = patchAttr.info.methodName;
                            if (targetType == null || targetMethodName == null)
                                continue;

                         

                            var targetMethod = AccessTools.Method(targetType, targetMethodName, patchAttr.info.argumentTypes);
                            if (targetMethod == null)
                            {
                                Console.WriteLine("[TorchHarmony] Could not find " + targetType.FullName + "." + targetMethodName);
                                continue;
                            }

                            MethodInfo prefix = null;
                            MethodInfo postfix = null;
                            MethodInfo transpiler = null;

                            if (method.GetCustomAttributes(typeof(HarmonyPrefix), false).Length > 0)
                                prefix = method;
                            if (method.GetCustomAttributes(typeof(HarmonyPostfix), false).Length > 0)
                                postfix = method;
                            if (method.GetCustomAttributes(typeof(HarmonyTranspiler), false).Length > 0)
                                transpiler = method;

                            // If no attribute is defined, default to prefix
                            if (prefix == null && postfix == null && transpiler == null)
                                prefix = method;

                            harmony.Patch(
                                targetMethod,
                                prefix != null ? new HarmonyMethod(prefix) : null,
                                postfix != null ? new HarmonyMethod(postfix) : null,
                                transpiler != null ? new HarmonyMethod(transpiler) : null
                            );

                            Console.WriteLine("[TorchHarmony] Patched " + targetType.FullName + "." + targetMethodName +
                                              " with " + (method.DeclaringType != null ? method.DeclaringType.Name : "?") +
                                              "." + method.Name);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[TorchHarmony] Failed to patch method " + method.Name + ": " + ex);
                        }
                    }
                }
            }
        }
    }
}
