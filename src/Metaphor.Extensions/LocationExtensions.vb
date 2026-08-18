Imports System.Runtime.CompilerServices
Imports Metaphor.Persistence

Public Module LocationExtensions
#Region "Characters"
    <Extension>
    Friend Function CreateN00b(location As ILocation, name As String, initializer As CharacterInitializer) As ICharacter
        Return location.CreateCharacter(CharacterSubtypes.N00B, name, initializer)
    End Function
    <Extension>
    Friend Function GetEnemies(location As ILocation) As IEnumerable(Of ICharacter)
        Return location.Characters.Where(Function(x) x.HasTag(Tags.ENEMY))
    End Function
#Region "Slime"
    <Extension>
    Friend Function CreateSlime(location As ILocation) As ICharacter
        Return location.CreateCharacter(CharacterSubtypes.SLIME, "Slime", AddressOf InitializeSlime)
    End Function
    Private Sub InitializeSlime(character As ICharacter)
        character.SetTag(Tags.ENEMY)
        character.InitializeCounter(Counters.HEALTH, 25, 0, 25)
        character.SetMetadata(Metadatas.POSTURE, Postures.REST)
    End Sub
#End Region
#End Region
#Region "Features"
#Region "Doors"
    <Extension>
    Friend Function CreateDoor(location As ILocation, direction As String, destination As ILocation) As IFeature
        Return location.CreateFeature(
            FeatureSubtypes.DOOR,
            $"Door going {direction}",
            Sub(feature)
                feature.SetYoke(Yokes.DESTINATION, destination.EntityId)
                feature.CreateVerb(VerbSubtypes.ENTER, "Enter")
            End Sub)
    End Function
#End Region
#Region "Cactus"
    <Extension>
    Friend Function CreateCactus(location As ILocation) As IFeature
        Return location.CreateFeature(FeatureSubtypes.CACTUS, "Cactus", AddressOf InitializeCactus)
    End Function

    Private Sub InitializeCactus(feature As IFeature)
        feature.CreateVerb(VerbSubtypes.TOUCH, "Touch")
    End Sub
#End Region
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
