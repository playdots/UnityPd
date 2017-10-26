/* <copyright file="PDTest.cs" company="Playdots, Inc.">
 * Copyright (C) 2017 Playdots, Inc.
 * </copyright>
 * ----------------------------
 */
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// ...
/// </summary>
public class PDTest : MonoBehaviour {

    IEnumerator Start() {
        yield return new WaitForSeconds (1f);

        UnityPD.Init ();
    }

    void OnApplicationQuit() {
        UnityPD.Deinit ();
    }
}
