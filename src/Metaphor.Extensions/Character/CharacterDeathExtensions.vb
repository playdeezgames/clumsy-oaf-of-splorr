Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module CharacterDeathExtensions
#Region "Death/Respawn"
    <Extension>
    Public Function IsDead(character As ICharacter) As Boolean
        Return character.IsCounterMinimum(Counters.HEALTH)
    End Function
    <Extension>
    Friend Sub Die(character As ICharacter)
        character.AddMessage($"{character.Name} dies.")
        character.MinimizeCounter(Counters.HEALTH)
    End Sub
    <Extension>
    Public Sub Respawn(character As ICharacter)
        If Not character.IsAvatar Then
            Return
        End If
        'TODO: drop yer stuff in a gravestone?
        character.Location = character.GetCheckpoint().Location
        character.AddMessage($"{character.Name} respawns in {character.Location.Name}.")
        character.MaximumCounter(Counters.HEALTH)
    End Sub
#End Region
End Module
