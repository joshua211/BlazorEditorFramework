namespace BlazorEditorFramework.Core.Editor.Input;

public enum InputType
{
    InsertText,
    DeleteContentBackward,
    DeleteContentForward,
    DeleteWordBackward,
    DeleteWordForward,
    InsertFromPaste,
    InsertParagraph,
    InsertLineBreak,
    InsertTab,
    ChangeSelection,
}