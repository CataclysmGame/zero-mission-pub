using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextButton : Button
{
    public bool syncImageColor = true;

    private Text childText;

    private void GetComponents()
    {
        childText = GetComponentInChildren<Text>();
    }

    protected override void Awake()
    {
        base.Awake();
        GetComponents();
    }

    private void LateUpdate()
    {
        var color = GetCurrentColor();
        color.a = childText.color.a;
        childText.color = color;
    }

    private Color GetCurrentColor()
    {
        if (transition != Transition.ColorTint)
        {
            return targetGraphic.color;
        }

        if (!interactable)
        {
            return MixColor(colors.disabledColor);
        }

        if (IsPressed())
        {
            return MixColor(colors.pressedColor);
        }

        if (currentSelectionState == SelectionState.Selected)
        {
            return MixColor(colors.selectedColor);
        }

        if (IsHighlighted())
        {
            return MixColor(colors.highlightedColor);
        }

        return targetGraphic.color;
    }

    private Color MixColor(Color color)
    {
        return targetGraphic.color * color * colors.colorMultiplier;
    }
}
