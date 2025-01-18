using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        //[Header("Character Input Values")]

        public Vector2 move;
        public Vector2 look;
        public bool attack;
        public bool interact;
        public bool pause;
        public bool dash;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED

        public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnInteract(InputValue value)
		{
			InteractInput(value.isPressed);
		}

        public void OnPause(InputValue value)
        {
            PauseInput(value.isPressed);
        }

#endif


        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void InteractInput(bool newInteractState)
        {
            interact = newInteractState;
        }

        public void PauseInput(bool newPauseState)
        {
            pause = newPauseState;
        }

    }

}