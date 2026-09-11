using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEditor;

namespace Poolcore.Editor
{
    public sealed class InputValidation : MonoBehaviour
    {
        public static string Run()
        {
            if(!Application.isPlaying) return "Play Mode required";
            new GameObject("Input Verification").AddComponent<InputValidation>();
            return "Input verification started";
        }
        private IEnumerator Start()
        {
            var results=new List<string>();
            var player=FindAnyObjectByType<FirstPersonController>();
            var menu=FindAnyObjectByType<ExperienceMenu>();
            menu.Resume(); yield return null;
            results.Add((player.IsCaptured?"PASS":"FAIL")+" resume captures cursor");
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.Escape));
            yield return null; yield return null;
            results.Add((!player.IsCaptured && menu.MenuVisible?"PASS":"FAIL")+" Escape releases cursor and displays menu");
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState()); yield return null;
            var before=player.transform.position;
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.W));
            yield return new WaitForSeconds(0.15f);
            results.Add((Vector3.Distance(before,player.transform.position)<0.001f?"PASS":"FAIL")+" menu blocks movement");
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState()); yield return null;
            menu.Resume(); yield return null;
            before=player.transform.position;
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(Key.W));
            yield return new WaitForSeconds(0.3f);
            results.Add((Vector3.Distance(before,player.transform.position)>0.1f?"PASS":"FAIL")+" W drives movement");
            InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState()); yield return null;
            player.Release(); player.ResetToSpawn();
            File.WriteAllLines("artifacts/input-validation.txt",results);
            Destroy(gameObject);
        }
    }
}
