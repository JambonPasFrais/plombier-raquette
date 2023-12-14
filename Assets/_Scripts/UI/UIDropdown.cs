using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDropdown : MonoBehaviour, ISelectHandler
{
    private ScrollRect _scrollRect;
    private float _scrollPosition = 1;

	private void Start()
	{
		_scrollRect = GetComponentInParent<ScrollRect>(true);
		int childcount = _scrollRect.content.transform.childCount - 1;
		int childIndex = transform.GetSiblingIndex();

		childIndex = childIndex < ((float)childcount / 2f) ? childIndex - 1 : childIndex;

		_scrollPosition = 1 - ((float)childIndex / childcount);
	}

	public void OnSelect(BaseEventData eventData)
	{
		if (_scrollRect)
			_scrollRect.verticalScrollbar.value = _scrollPosition;
	}
}
