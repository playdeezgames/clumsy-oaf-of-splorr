Imports Metaphor.Extensions
Imports Metaphor.Persistence

Friend Module BlueRoomInitializer
    Friend Function Initialize(context As IInitializationContext) As Persistence.LocationInitializer
        Return Sub(room)
                   room.CreateN00b(context.ChosenName, InitializeAvatar(context))
               End Sub
    End Function
    Private Function InitializeAvatar(context As IInitializationContext) As CharacterInitializer
        Return Sub(character)
                   character.World.Avatar = character
               End Sub
    End Function
End Module
