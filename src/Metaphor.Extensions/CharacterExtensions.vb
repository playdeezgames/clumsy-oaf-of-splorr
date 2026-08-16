Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module CharacterExtensions
    <Extension>
    Public Function IsAvatar(character As ICharacter) As Boolean
        Return character.World.Avatar.EntityId = character.EntityId
    End Function
#Region "Show Status"
    <Extension>
    Public Sub ShowStatus(character As ICharacter)
        character.AddMessage($"Status:")
    End Sub
#End Region
#Region "Look"
    <Extension>
    Public Sub Look(character As ICharacter)
        Dim location = character.Location
        character.AddMessage($"{character.Name} is in {location.Name}.")
        DescribeFeatures(location)
    End Sub
    Private Sub DescribeFeatures(location As ILocation)
        If Not location.HasFeatures Then
            Return
        End If
        location.AddMessage($"Features:")
        For Each feature In location.Features
            location.AddMessage($"- {feature.Name}")
        Next
    End Sub
#End Region
#Region "Checkpoint"
    <Extension>
    Public Sub SetCheckpoint(character As ICharacter, checkpoint As IFeature)
        character.SetYoke(Yokes.CHECKPOINT, checkpoint.EntityId)
    End Sub
    <Extension>
    Public Function GetCheckpoint(character As ICharacter) As IFeature
        Return character.World.GetFeature(character.GetYoke(Yokes.CHECKPOINT))
    End Function
    <Extension>
    Public Function IsCurrentCheckpoint(character As ICharacter, checkpoint As IFeature) As Boolean
        Return character.GetCheckpoint().EntityId = checkpoint.EntityId
    End Function
#End Region
End Module
