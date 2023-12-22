using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
	[Header("Instances")]
	[SerializeField] private GameObject _controllerMenuIcon;
	[SerializeField] private GameObject _characterSelectionIcon;
	[SerializeField] private Image _imgCharSelectionIcon;
	[SerializeField] private TextMeshProUGUI _playerIndexText;
	[SerializeField] private RectTransform _rectTransform;
	[SerializeField] private List<Color> _playerColors = new List<Color>()
	{
		Color.red,
		Color.blue,
		Color.green,
		Color.yellow
	};

	[HideInInspector] public PlayerInput PlayerInput;// TODO : change this variable for private with getter and setter

	[Header("Game Feel")]
	[SerializeField] private float _speed;

	private Vector2 _movementDir;
	private bool _isSelectingCharacter;
	private bool _characterSelected;
	private int _controllerIndex;

	#region UNITY FUNCTIONS

	private void Start()
	{
		_rectTransform = GetComponent<RectTransform>();
	}

	private void Update()
	{
		transform.Translate(_movementDir * Time.deltaTime * _speed);
	}
	#endregion

	#region CALLED EXTERNALLY
	public void TryPunch()
	{
		Debug.Log(PlayerInput.playerIndex);

		if (_isSelectingCharacter)
			return;

		transform.DOComplete();
		transform.DOPunchScale(Vector3.one * .1f, .2f);
	}

	public void TryMove(Vector2 readValue)
	{
		if (!_isSelectingCharacter)
			return;

		if (_characterSelected)
			return;

		_movementDir = readValue;
	}

	public void TrySelect()
	{
		Debug.Log("J'appuie sur une touche !");
		if (!_isSelectingCharacter)
			return;

		if (_characterSelected)
			return;

		Ray ray = Camera.main.ScreenPointToRay(Camera.main.WorldToScreenPoint(transform.position));

		if (ControllerManager.Instance.CharacterSelectionMenu.HandleCharacterSelectionInput(ray, PlayerInput.playerIndex))
		{
			_characterSelected = true;
			_imgCharSelectionIcon.color /= 2;
		}
	}

	public void TryDeselect()
	{
		if (!_isSelectingCharacter)
			return;

		if (!_characterSelected)
			return;

		if (ControllerManager.Instance.CharacterSelectionMenu.HandleCharacterDeselectionInput(PlayerInput.playerIndex))
		{
			_characterSelected = false;
			_imgCharSelectionIcon.color = Color.white;
		}
	}

	public void ControllerSelectionMode()
	{
		_isSelectingCharacter = false;

		_controllerMenuIcon.SetActive(true);
		_characterSelectionIcon.SetActive(false);
		_playerIndexText.gameObject.SetActive(true);

		transform.position = Vector3.zero;
		transform.localScale = Vector3.one;
	}

	public void CharacterSelectionMode()
	{
		_isSelectingCharacter = true;

		_controllerMenuIcon.SetActive(false);
		_characterSelectionIcon.SetActive(true);
		_playerIndexText.gameObject.SetActive(false);

		_rectTransform.sizeDelta = new Vector2(50f, 50f);
		transform.position = Vector3.zero;
		transform.localScale = Vector3.one;
	}

	public void ResetView()
	{
		_characterSelected = false;
		_imgCharSelectionIcon.color = Color.white;
	}

	public void SetPlayerIndex(int index)
	{
		_playerIndexText.gameObject.SetActive(true);
		_controllerIndex = index;
		_playerIndexText.text = "P" + _controllerIndex;
	}

	public void SetColorVisual(int index)
	{
		_playerIndexText.color = _playerColors[index];
	}
	#endregion
}
