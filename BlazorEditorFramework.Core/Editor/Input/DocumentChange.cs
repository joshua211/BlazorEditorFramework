using BlazorEditorFramework.Core.Editor.Abstractions;

namespace BlazorEditorFramework.Core.Editor.Input;

public record DocumentChange(int From, int To, int CharacterShift, ChangeType ChangeType) : IRange
{
}