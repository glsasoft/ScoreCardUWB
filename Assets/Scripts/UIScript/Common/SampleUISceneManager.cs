using LetsPiu.Common.Component;
using UnityEngine;

namespace LetsPiu.Common.UI
{
    public class SampleUISceneManager : MonoBehaviour
    {
        public Transform menuDialogPosition;

        public void OpenPlainTextDialog() =>
            DialogManager.OpenPlainTextDialog("Plain Text Dialog", "This is a plain text dialog.");

        public void OpenConfirmTextDialog() =>
            DialogManager.OpenConfirmTextDialog("Confirm Text Dialog", "This is a confirm text dialog.",
                "Confirm", () => Debug.Log("Confirmed!"));

        public void OpenBinaryChoiceTextDialog() =>
            DialogManager.OpenBinaryChoiceTextDialog("Binary Choice Text Dialog",
                "This is a binary choice text dialog.", "Confirm", "Cancel",
                confirmed => Debug.Log(confirmed ? "Confirmed!" : "Canceled"));

        public void OpenBinaryChoiceScrollviewDialog() =>
            DialogManager.OpenBinaryChoiceScrollviewDialog("Binary Choice Text Dialog",
                null, "Confirm", "Cancel",
                confirmed => Debug.Log(confirmed ? "Confirmed!" : "Canceled"));
    }
}