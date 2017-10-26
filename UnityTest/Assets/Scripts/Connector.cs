/* <copyright file="Connector.cs" company="Playdots, Inc.">
 * Copyright (C) 2016 Playdots, Inc.
 * </copyright>
 * ----------------------------
 */
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// ...
/// </summary>
public class Connector : MonoBehaviour {
    private List<Dot> connectedDots = new List<Dot>();
    private bool mouseIsDown;

    public DotSoundPlayer player;

    void Update() {
        if (mouseIsDown)
        {
            if (Input.GetMouseButtonUp(0))
            {
                mouseIsDown = false;
                for (int i = 0; i < connectedDots.Count; i++)
                {
                    connectedDots[i].OnDisconnected();
                }
                connectedDots.Clear();
            }
        }
    }

    public void OnMouseDown( Dot dot ) {
        mouseIsDown = true;
        connectedDots.Clear();
        ConnectDot(dot);
    }

    public void OnMouseEnterDot( Dot dot ) {
        if (mouseIsDown)
            ConnectDot(dot);
    }

    private void ConnectDot( Dot dot ) {
        if (!connectedDots.Contains(dot))
        {
            connectedDots.Add(dot);
            dot.OnConnected();

            // play sounds
            player.PlayDotConnectedSound(connectedDots.Count - 1);
        }
    }
}
