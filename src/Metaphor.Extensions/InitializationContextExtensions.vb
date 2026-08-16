Imports System.Runtime.CompilerServices
Imports Metaphor.Extensions
Imports Metaphor.Persistence

Friend Module InitializationContextExtensions
    <Extension>
    Friend Function InitializeBlueRoom(context As IInitializationContext) As Persistence.LocationInitializer
        Return Sub(room)
                   room.CreateN00b(context.ChosenName, context.InitializeAvatar())
               End Sub
    End Function
    <Extension>
    Private Function InitializeAvatar(context As IInitializationContext) As CharacterInitializer
        Return Sub(character)
                   character.World.Avatar = character
               End Sub
    End Function
End Module
