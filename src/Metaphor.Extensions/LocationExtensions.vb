Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module LocationExtensions
#Region "Characters"
    <Extension>
    Friend Function CreateN00b(location As ILocation, name As String, initializer As CharacterInitializer) As ICharacter
        Return location.CreateCharacter(CharacterSubtypes.N00B, name, initializer)
    End Function
#End Region
#Region "Features"
#Region "Checkpoint"
    <Extension>
    Friend Function CreateCheckpoint(location As ILocation) As IFeature
        Return location.CreateFeature(FeatureSubtypes.CHECKPOINT, "Checkpoint", AddressOf InitializeCheckpoint)
    End Function
    Private Sub InitializeCheckpoint(feature As IFeature)
        feature.CreateVerb(VerbSubtypes.SET_CHECKPOINT, "Set Checkpoint")
    End Sub
#End Region
#End Region
End Module
