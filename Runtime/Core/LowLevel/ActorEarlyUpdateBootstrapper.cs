using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.LowLevel.PlayerLoopUtils;
using UnityEngine.PlayerLoop;

namespace SAS.StateMachineGraph
{
    internal static class ActorEarlyUpdateBootstrapper
    {
        static PlayerLoopSystem actorEarlyUpdateSystem;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            var subSystem = PlayerLoopUtils.FindSubSystem<EarlyUpdate>(ref currentPlayerLoop);
            if (!InsertActorEarlyUpdateManager<EarlyUpdate>(ref currentPlayerLoop, subSystem.HasValue ? subSystem.Value.subSystemList.Length - 1 : 0))
            {
                Debug.LogWarning("ActorEarlyUpdateBootstrapper not initialized, unable to register ActorEarlyUpdateManager into the EarlyUpdate loop.");
                return;
            }
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);
            PlayerLoopUtils.PrintPlayerLoop(currentPlayerLoop);
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;

            static void OnPlayModeState(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingPlayMode)
                {
                    PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
                    RemoveEarlyUpdateManager<EarlyUpdate>(ref currentPlayerLoop);
                    PlayerLoop.SetPlayerLoop(currentPlayerLoop);

                    ActorEarlyUpdateManager.Clear();
                }
            }
#endif
        }

        static void RemoveEarlyUpdateManager<T>(ref PlayerLoopSystem loop)
        {
            PlayerLoopUtils.RemoveSystem<T>(ref loop, in actorEarlyUpdateSystem);
        }

        static bool InsertActorEarlyUpdateManager<T>(ref PlayerLoopSystem loop, int index)
        {
            actorEarlyUpdateSystem = new PlayerLoopSystem()
            {
                type = typeof(ActorEarlyUpdateManager),
                updateDelegate = ActorEarlyUpdateManager.ActorEarlyUpdates,
                subSystemList = null
            };
            return PlayerLoopUtils.InsertSystem<T>(ref loop, in actorEarlyUpdateSystem, index);
        }
    }
}
