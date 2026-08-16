Imports System.Runtime.CompilerServices
Imports Metaphor.Extensions
Imports Metaphor.Persistence

Friend Module InitializationContextExtensions
    <Extension>
    Friend Function InitializeBlueRoom(context As IInitializationContext) As Persistence.LocationInitializer
        Return Sub(room)
                   Dim checkpoint = room.CreateCheckpoint()
                   room.CreateN00b(context.ChosenName, context.InitializeN00b(checkpoint))

               End Sub
    End Function
    <Extension>
    Private Function InitializeN00b(context As IInitializationContext, checkpoint As IFeature) As CharacterInitializer
        Return Sub(character)
                   character.SetCheckpoint(checkpoint)
                   character.World.Avatar = character
               End Sub
    End Function
End Module
