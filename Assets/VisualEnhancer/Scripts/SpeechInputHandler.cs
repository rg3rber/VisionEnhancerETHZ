using MixedReality.Toolkit;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SpeechInputHandler : MonoBehaviour
{
    [SerializeField]
    private List<PhraseAction> phraseActions;

    private void Start()
    {
        // Get the first running phrase recognition subsystem.
        // var keywordRecognitionSubsystem = XRSubsystemHelpers.GetFirstRunningSubsystem<KeywordRecognitionSubsystem>();

        // // If we found one...
        // if (keywordRecognitionSubsystem != null)
        // {
        //     // Register a keyword and its associated action with the subsystem
        //     keywordRecognitionSubsystem.CreateOrGetEventForKeyword("hi").AddListener(() => Debug.Log("Keyword recognized"));
        // }

        var phraseRecognitionSubsystem = XRSubsystemHelpers.KeywordRecognitionSubsystem;
        if (phraseRecognitionSubsystem != null) {
            phraseRecognitionSubsystem.CreateOrGetEventForKeyword("hi").AddListener(() => Debug.Log("Keyword recognized!"));
        }

        foreach (var phraseAction in phraseActions)
        {
            if (!string.IsNullOrEmpty(phraseAction.Phrase) && phraseAction.Action.GetPersistentEventCount() > 0)
            {
                phraseRecognitionSubsystem.CreateOrGetEventForKeyword(phraseAction.Phrase).AddListener(() => phraseAction.Action.Invoke());
            }
        }
    }

    private void OnValidate()
    {
        var multipleEntries = phraseActions.GroupBy(p => p.Phrase).Where(p => p.Count() > 1).ToList();
        if (multipleEntries.Any())
        {
            var errorMessage = new StringBuilder();
            errorMessage.AppendLine("Some phrases defined are more than once , this is not allowed");
            foreach (var phraseGroup in multipleEntries)
            {
                errorMessage.AppendLine($"- {phraseGroup.Key}");
            }
            Debug.LogError(errorMessage);
        }
    }
}