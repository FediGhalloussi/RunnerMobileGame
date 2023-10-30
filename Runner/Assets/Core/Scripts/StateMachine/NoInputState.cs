using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HyperCasual.Runner; // Assuming your InputManager is in this namespace

namespace HyperCasual.Core
{
    /// <summary>
    /// This game loop is paused while this state is active.
    /// </summary>
    public class NoInputState : AbstractState
    {
        readonly Action m_OnPause;
        bool m_InputWasEnabled;

        public override string Name => $"{nameof(PauseState)}";

        /// <param name="onPause">The action that is invoked when the game loop is paused</param>
        public NoInputState(Action onPause)
        {
            m_OnPause = onPause;
        }

        public override void Enter()
        {
            if (InputManager.Instance != null) { 
            m_InputWasEnabled = InputManager.Instance.enabled; // Store current input state from your InputManager
            InputManager.Instance.enabled = false; // Deactivate input using your InputManager
            GameManager.Instance.NoInputStart(); // Deactivate input using your InputManager
                                                 // Disabling the ability to move
            PlayerController.Instance.CanMove = false;
            }
            m_OnPause?.Invoke();
        }

        public override IEnumerator Execute()
        {
            yield return null;
        }

        public override void Exit()
        {
            if (InputManager.Instance != null && m_InputWasEnabled)
            {
                InputManager.Instance.enabled = true; // Re-enable input using your InputManager if it was enabled before entering PauseState
                GameManager.Instance.NoInputStop();
                PlayerController.Instance.CanMove = true;

            }
        }
    }
}
