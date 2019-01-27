using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Wrapper Class for Json and List integration
/// </summary>
public static class JsonHelper
{
    public static string ToJson<T>(List<T> list)
    {
        Wrapper<T> wrapper = new Wrapper<T>
        {
            GameSession = list
        };
        return JsonUtility.ToJson(wrapper, true);
    }

    [Serializable]
    private partial class Wrapper<T>
    {
        public List<T> GameSession;
    }
}
