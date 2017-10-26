/* <copyright file="Dot.cs" company="Playdots, Inc.">
 * Copyright (C) 2016 Playdots, Inc.
 * </copyright>
 * ----------------------------
 */
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// ...
/// </summary>
public class Dot : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler
{
    public Color connectedColor, disconnectedColor;

    public void OnPointerDown(PointerEventData eventData)
    {
        GetComponentInParent<Connector>().OnMouseDown(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        GetComponentInParent<Connector>().OnMouseEnterDot(this);
    }

    public void OnConnected() {
        transform.localScale = Vector3.one * 1.2f;
        GetComponentInChildren<Image>().color = connectedColor;
    }

    public void OnDisconnected() {
        transform.localScale = Vector3.one;
        GetComponentInChildren<Image>().color = disconnectedColor;
    }
}
