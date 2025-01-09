using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct PhraseAction
{
    [SerializeField]
    private string phrase;

    [SerializeField]
    private UnityEvent action;

    public string Phrase => phrase;

    public UnityEvent Action => action;
}
