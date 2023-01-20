using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputState : MonoBehaviour
{
	[Header("Character Input Values")]
	public Vector2 Move;
	public Vector2 Look;
	public bool Jump;
	public bool Sprint;
	public bool PrimaryAction;
	public bool SecondaryAction;

	[Header("Movement Settings")]
	public bool AnalogMovement;

	[Header("Mouse Cursor Settings")]
	public bool CursorLocked = true;
	public bool CursorInputForLook = true;

	public void OnMove(InputValue value)
	{
		Move = value.Get<Vector2>();
	}
	
	public void OnLook(InputValue value)
	{
		if (CursorInputForLook)
		{
			Look = value.Get<Vector2>();
		}
	}

	public void OnJump(InputValue value)
	{
		Jump = value.isPressed;
	}

	public void OnSprint(InputValue value)
	{
		Sprint = value.isPressed;
	}

	public void OnPrimaryAction(InputValue value)
	{
		PrimaryAction = value.isPressed;
	}

	public void OnSecondaryAction(InputValue value)
	{
		SecondaryAction = value.isPressed;
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		Cursor.lockState = CursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
	}
}